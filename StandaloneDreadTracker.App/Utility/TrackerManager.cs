using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using DreadRemoteConnector;
using DreadRemoteConnector.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.Extensions;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.App.Utility;

public sealed class TrackerManager(
	IOptionsMonitor<ApplicationSettings> settingsMonitor,
	IAppSettingsManager settingsManager,
	ILogger<TrackerManager> logger,
	ILoggerFactory loggerFactory
)
{
	private CancellationTokenSource?               initializationCancelSource;
	private ObservableCollection<TrackerViewModel> allTrackers = [];

	public ObservableCollection<TrackerViewModel> AllTrackers => this.allTrackers;

	private ApplicationSettings Settings => settingsMonitor.CurrentValue;

	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		using var thisCancelSource = new CancellationTokenSource();
		using var combinedCancelSource = CancellationTokenSource.CreateLinkedTokenSource(thisCancelSource.Token, cancellationToken);
		var combinedToken = combinedCancelSource.Token;

		try
		{
			if (Interlocked.Exchange(ref this.initializationCancelSource, thisCancelSource) is { } previousCancelSource)
				await previousCancelSource.TryCancelAsync();

			var previousTrackers = Interlocked.Exchange(ref this.allTrackers, []);

			// Clean up previous trackers
			foreach (var tracker in previousTrackers)
				await DisposeTrackerAsync(tracker);

			// Load new trackers
			foreach (var (index, trackerSettings) in Settings.Trackers.Pairs())
			{
				if (!TryCreateTracker(trackerSettings, out var tracker, out var errorMessage))
				{
					logger.LogWarning("Tracker configuration #{Index} was invalid: {ErrorMessage}", index, errorMessage);
					continue;
				}

				AllTrackers.Add(tracker);
				InitializeTracker(tracker, combinedToken);
			}

			logger.LogInformation("Loaded {TrackerCount} trackers", AllTrackers.Count);
		}
		finally
		{
			// Remove this CTS from field if nobody else overwrote it
			Interlocked.CompareExchange(ref this.initializationCancelSource, null, thisCancelSource);
		}
	}

	public async Task AddTrackerAsync(TrackerSettings trackerSettings)
	{
		if (AllTrackers.Any(t => string.Equals(t.Id, trackerSettings.Id, StringComparison.OrdinalIgnoreCase)))
			throw new ArgumentException("A tracker with the specified ID already exists", nameof(trackerSettings));

		if (!TryCreateTracker(trackerSettings, out var tracker, out var errorMessage))
			throw new ArgumentException(errorMessage, nameof(trackerSettings));

		await settingsManager.ModifyAsync(settings => {
			settings.Trackers.Add(trackerSettings);
		});

		AllTrackers.Add(tracker);
		InitializeTracker(tracker, CancellationToken.None);
	}

	public async Task UpdateTrackerAsync(TrackerViewModel tracker, Action<TrackerSettings>? updateSettings = null)
	{
		var foundSettings = false;
		var originalType = TrackerTargetType.Unknown;
		var originalAddress = default(string);

		await settingsManager.ModifyAsync(settings => {
			var trackerSettings = settings.Trackers.Find(t => string.Equals(t.Id, tracker.Id));

			if (trackerSettings != null)
			{
				foundSettings = true;
				originalType = trackerSettings.TargetType;
				originalAddress = trackerSettings.IpAddress;

				if (updateSettings != null)
					updateSettings(trackerSettings);
				else
					tracker.CopySettingsTo(trackerSettings);
			}
		});

		if (foundSettings && ( tracker.TargetType != originalType || tracker.TargetAddress != originalAddress ))
			// Need to re-initialize the tracker so it re-connects to the new target.
			await ReInitializeTrackerAsync(tracker);
	}

	public async Task RemoveTrackerAsync(TrackerViewModel tracker)
	{
		try
		{
			AllTrackers.Remove(tracker);

			await settingsManager.ModifyAsync(settings => {
				settings.Trackers.RemoveAll(t => string.Equals(t.Id, tracker.Id, StringComparison.OrdinalIgnoreCase));
			});
		}
		finally
		{
			await DisposeTrackerAsync(tracker);
		}
	}

	private async Task ReInitializeTrackerAsync(TrackerViewModel tracker)
	{
		if (this.initializationCancelSource != null)
			throw new InvalidOperationException($"Cannot re-initialize an individual tracker while {nameof(TrackerManager)} is still initializing itself");

		await DisposeTrackerAsync(tracker);

		if (!TryCreateConnector(tracker.TargetType, tracker.TargetAddress, out var newConnector, out var errorMessage))
			throw new ArgumentException(errorMessage, nameof(tracker));

		tracker.Connector = newConnector;
		InitializeTracker(tracker, CancellationToken.None);
	}

	private async void InitializeTracker(TrackerViewModel tracker, CancellationToken cancellationToken)
	{
		try
		{
			if (tracker.Connector is not { } connector)
				return;

			await connector.StartAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error initializing tracker \"{Name}\"", tracker.Name);
		}
	}

	private async Task DisposeTrackerAsync(TrackerViewModel tracker)
	{
		if (tracker.Connector is not { } connector)
			return;

		try
		{
			await connector.DisposeAsync();
		}
		catch
		{
			// Ignore exceptions during clean-up
		}

		tracker.Connector = null;
	}

	private bool TryCreateTracker(TrackerSettings trackerSettings, [NotNullWhen(true)] out TrackerViewModel? tracker, [NotNullWhen(false)] out string? errorMessage)
	{
		if (!TryCreateConnector(trackerSettings.TargetType, trackerSettings.IpAddress, out var connector, out errorMessage))
		{
			tracker = null;
			return false;
		}

		tracker = TrackerViewModel.Create(trackerSettings, connector);
		return true;
	}

	private bool TryCreateConnector(TrackerTargetType targetType, string? ipAddressString, [NotNullWhen(true)] out DreadConnector? connector, [NotNullWhen(false)] out string? errorMessage)
	{
		IPAddress? ipAddress;

		connector = null;
		errorMessage = null;

		if (targetType == TrackerTargetType.LocalEmulator)
		{
			ipAddress = IPAddress.Loopback;
		}
		else
		{
			if (string.IsNullOrEmpty(ipAddressString))
			{
				errorMessage = "No IP address was configured";
				return false;
			}

			if (!IPAddress.TryParse(ipAddressString, out ipAddress))
			{
				errorMessage = $"Invalid IP address: {ipAddressString}";
				return false;
			}
		}

		connector = new DreadConnector(ipAddress) {
			Logger = loggerFactory.CreateLogger<DreadConnector>(),
		};
		return true;
	}
}