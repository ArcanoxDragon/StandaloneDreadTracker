using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using DreadRemoteConnector;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.Extensions;
using StandaloneDreadTracker.App.Utility;

namespace StandaloneDreadTracker.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
	public MainViewModel() : this(null) { }

	public MainViewModel(ILogger<MainViewModel>? logger)
	{
		CreateTrackersCommand.HandleExceptionsWith(ex => {
			logger?.LogError(ex, "Could not create trackers!");
			return Observable.Empty<Unit>();
		});

		this.WhenActivated(disposables => {
			CreateTrackersCommand.Execute(disposables);
		});
	}

	[Reactive]
	public partial List<TrackerTargetViewModel> TrackerTargets { get; set; } = [];

	[ReactiveCommand]
	private async Task CreateTrackersAsync(CompositeDisposable disposables)
	{
		var targets = new List<TrackerTargetViewModel>();
		var connector = new DreadConnector("192.168.86.230").DisposeWith(disposables);

		connector.ConnectionInterests = ConnectionInterests.Logging | ConnectionInterests.Multiworld;
		connector.SleepTimeBeforeReconnect = TimeSpan.FromSeconds(5);

		var target = new TrackerTargetViewModel {
			Connector = connector,
			Name = "My Switch",
			TargetType = TrackerTargetType.Remote,
			TargetAddress = "192.168.86.230",
		};

		targets.Add(target);
		TrackerTargets = targets;

		await connector.StartAsync();
	}
}