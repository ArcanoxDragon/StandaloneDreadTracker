using ReactiveUI.SourceGenerators;

namespace StandaloneDreadTracker.App.ViewModels;

public partial class TrackerSettingsViewModel : ViewModelBase
{
	[Reactive]
	public partial TrackerViewModel Tracker { get; set; } = new();
}