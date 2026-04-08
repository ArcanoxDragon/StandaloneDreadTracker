using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.Services;
using StandaloneDreadTracker.App.Utility;

namespace StandaloneDreadTracker.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
	public MainViewModel() : this(null, null) { }

	public MainViewModel(IServiceProvider? serviceProvider, TrackerManager? trackerManager, IDialogs? dialogs = null)
	{
		ServiceProvider = serviceProvider;
		TrackerManager = trackerManager;
		Dialogs = dialogs;

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

	private TrackerManager? TrackerManager { get; }
	private IDialogs?       Dialogs        { get; }

	[Reactive]
	public partial ObservableCollection<TrackerViewModel> TrackerTargets { get; set; } = [];

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