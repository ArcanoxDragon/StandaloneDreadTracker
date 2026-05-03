using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Arcanox.AppCore.Reactive;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.Services;
using StandaloneDreadTracker.App.Utility;

namespace StandaloneDreadTracker.App.ViewModels;

public partial class MainViewModel : BaseViewModel
{
	public MainViewModel()
		: this(null, null, null, null) { }

	public MainViewModel(
		IServiceProvider? serviceProvider,
		TrackerManager? trackerManager,
		IAppSettingsManager? settingsManager,
		IDialogs? dialogs)
	{
		ServiceProvider = serviceProvider;
		CanResolveServices = serviceProvider != null;
		TrackerManager = trackerManager;
		SettingsManager = settingsManager;
		Dialogs = dialogs;
		HasServices = TrackerManager != null && SettingsManager != null && Dialogs != null;

		this.WhenActivated(disposables => {
			if (trackerManager != null)
			{
				TaskPoolScheduler.ScheduleAsync(async (_, cancellationToken) => {
					await trackerManager.InitializeAsync(cancellationToken);

					TrackerTargets = trackerManager.AllTrackers;
				}).DisposeWith(disposables);
			}

			Disposable.Create(() => {
				TrackerTargets = [];
			}).DisposeWith(disposables);
		});
	}

	[AllowNull]
	public IServiceProvider ServiceProvider
	{
		get => field ?? throw new InvalidOperationException("The view model was not initialized with a service provider");
		private set;
	}

	public bool CanResolveServices { get; }

	private TrackerManager?      TrackerManager  { get; }
	private IAppSettingsManager? SettingsManager { get; }
	private IDialogs?            Dialogs         { get; }

	[MemberNotNullWhen(true, nameof(TrackerManager))]
	[MemberNotNullWhen(true, nameof(SettingsManager))]
	[MemberNotNullWhen(true, nameof(Dialogs))]
	private bool HasServices { get; }

	[Reactive]
	public partial ObservableCollection<TrackerViewModel> TrackerTargets { get; set; } = [];

	[ReactiveCommand]
	private async Task AddTrackerAsync()
	{
		if (!HasServices)
			return;

		var newTracker = new TrackerViewModel(ServiceProvider);
		var didSave = await Dialogs.EditTrackerAsync(newTracker, "New Tracker");

		if (!didSave)
			return;

		var newTrackerSettings = new TrackerSettings();

		newTracker.CopySettingsTo(newTrackerSettings);

		try
		{
			// Add new tracker to TrackerManager (it will handle adding to settings)
			await TrackerManager.AddTrackerAsync(newTrackerSettings);
		}
		catch
		{
			// Ignore all exceptions here
		}
	}

	[ReactiveCommand]
	private async Task EditTrackerAsync(TrackerViewModel tracker)
	{
		if (!HasServices)
			return;

		var didEdit = await Dialogs.EditTrackerAsync(tracker);

		if (!didEdit)
			return;

		try
		{
			await TrackerManager.UpdateTrackerAsync(tracker);
		}
		catch
		{
			// Ignore all exceptions here
		}
	}

	[ReactiveCommand]
	private async Task DeleteTrackerAsync(TrackerViewModel tracker)
	{
		if (TrackerManager is null)
			return;

		if (Dialogs != null)
		{
			var confirmed = await Dialogs.ConfirmAsync(
				"Delete Tracker",
				$"Are you sure you want to delete the tracker named \"{tracker.Name}\"?",
				"Yes",
				"No");

			if (!confirmed)
				return;
		}

		await TrackerManager.RemoveTrackerAsync(tracker);
	}
}