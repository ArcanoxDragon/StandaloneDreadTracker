using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using DreadRemoteConnector;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.Extensions;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.App.Utility;

public sealed class TrackerManager(
	IOptionsMonitor<ApplicationSettings> settingsMonitor,
	ISettingsManager settingsManager,
	ILogger<TrackerManager> logger
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
		if (AllTrackers.Any(t => string.Equals(t.Name, trackerSettings.Name, StringComparison.OrdinalIgnoreCase)))
			throw new ArgumentException("A tracker with the specified name already exists", nameof(trackerSettings));

		if (!TryCreateTracker(trackerSettings, out var tracker, out var errorMessage))
			throw new ArgumentException(errorMessage, nameof(trackerSettings));

		await settingsManager.ModifyAsync(settings => {
			settings.Trackers.Add(trackerSettings);
		});

		AllTrackers.Add(tracker);
		InitializeTracker(tracker, CancellationToken.None);
	}

	public async Task RemoveTrackerAsync(TrackerViewModel tracker)
	{
		try
		{
			AllTrackers.Remove(tracker);

			await settingsManager.ModifyAsync(settings => {
				settings.Trackers.RemoveAll(t => string.Equals(t.Name, tracker.Name, StringComparison.OrdinalIgnoreCase));
			});
		}
		finally
		{
			await DisposeTrackerAsync(tracker);
		}
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
	}

	private bool TryCreateTracker(TrackerSettings trackerSettings, [NotNullWhen(true)] out TrackerViewModel? tracker, [NotNullWhen(false)] out string? errorMessage)
	{
		IPAddress? ipAddress;

		tracker = null;
		errorMessage = null;

		if (trackerSettings.TargetType == TrackerTargetType.LocalEmulator)
		{
			ipAddress = IPAddress.Loopback;
		}
		else
		{
			if (string.IsNullOrEmpty(trackerSettings.IpAddress))
			{
				errorMessage = "No IP address was configured";
				return false;
			}

			if (!IPAddress.TryParse(trackerSettings.IpAddress, out ipAddress))
			{
				errorMessage = $"Invalid IP address: {trackerSettings.IpAddress}";
				return false;
			}
		}

		tracker = TrackerViewModel.Create(trackerSettings, new DreadConnector(ipAddress));
		return true;
	}
}