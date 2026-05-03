using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.State;
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
	private void OpenPreviousTrackerSession(TrackerViewModel tracker)
		=> OpenTrackerWindow(tracker, loadPreviousSession: true);

	[ReactiveCommand]
	private void StartNewTrackerSession(TrackerViewModel tracker)
		=> OpenTrackerWindow(tracker, loadPreviousSession: false);

	private void OpenTrackerWindow(TrackerViewModel tracker, bool loadPreviousSession)
	{
		if (ViewModel is null or { CanResolveServices: false })
			return;

		var scope = ViewModel.ServiceProvider.CreateScope();
		var window = new TrackerWindow(scope, tracker);

		if (tracker.Id != null && loadPreviousSession)
		{
			var appState = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ApplicationState>>().Value;

			if (appState.TrackerSessions.TryGetValue(tracker.Id, out var session))
				tracker.LoadStateFrom(session);
		}
		else if (!loadPreviousSession)
		{
			tracker.ResetState();
		}

		window.Show();
	}
}