using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
	public MainView()
	{
		InitializeComponent();
	}

	[ReactiveCommand]
	private void OpenTrackerWindow(TrackerViewModel tracker)
	{
		var window = new TrackerWindow(tracker);

		window.Show();
	}
}