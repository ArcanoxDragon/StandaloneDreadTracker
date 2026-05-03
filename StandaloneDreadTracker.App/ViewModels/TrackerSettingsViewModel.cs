using Arcanox.AppCore.Reactive;
using ReactiveUI.SourceGenerators;

namespace StandaloneDreadTracker.App.ViewModels;

public partial class TrackerSettingsViewModel : BaseViewModel
{
	[Reactive]
	public partial TrackerViewModel Tracker { get; set; } = new();
}