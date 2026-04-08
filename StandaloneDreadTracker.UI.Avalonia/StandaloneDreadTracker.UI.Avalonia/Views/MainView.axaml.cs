using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.ViewModels;
using StandaloneDreadTracker.UI.Avalonia.Views.Tracker;

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
		if (ViewModel is null)
			return;

		var scope = ViewModel.ServiceProvider.CreateScope();
		var window = new TrackerWindow(scope, tracker);

		window.Show();
	}
}