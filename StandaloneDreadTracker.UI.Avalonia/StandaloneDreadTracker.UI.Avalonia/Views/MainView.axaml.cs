using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.Utility;
using StandaloneDreadTracker.App.ViewModels;
using StandaloneDreadTracker.UI.Avalonia.Views.Dialogs;
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
		if (ViewModel is null or { CanResolveServices: false })
			return;

		var scope = ViewModel.ServiceProvider.CreateScope();
		var window = new TrackerWindow(scope, tracker);

		window.Show();
	}

	[ReactiveCommand]
	private async Task EditTrackerAsync(TrackerViewModel tracker)
	{
		if (ViewModel is null)
			return;

		if (TopLevel.GetTopLevel(this) is not Window window)
			// TODO: Android?
			return;

		var dialog = new EditTrackerDialog(tracker) {
			Title = "Edit Tracker",
		};
		var result = await dialog.ShowDialog<bool>(window);

		if (!result)
			return;

		// Apply dialog edits to the original VM, and then save the tracker.
		// We need to keep track of the original values, first so that we can
		// locate the matching TrackerSettings while saving the changes, and
		// also so that we can re-create the connector if needed.
		var originalName = tracker.Name;
		var originalType = tracker.TargetType;
		var originalAddress = tracker.TargetAddress;

		dialog.ApplySettingsTo(tracker);

		if (Application.Current is not App { CanResolveServices: true } app)
			return;

		try
		{
			var settingsManager = app.ServiceProvider.GetRequiredService<ISettingsManager>();

			await settingsManager.ModifyAsync(settings => {
				var trackerSettings = settings.Trackers.Find(t => string.Equals(t.Name, originalName));

				if (trackerSettings != null)
					tracker.CopySettingsTo(trackerSettings);
			});

			if (tracker.TargetType != originalType || tracker.TargetAddress != originalAddress)
			{
				// Need to re-initialize TrackerManager so it re-connects to the new target.
				var trackerManager = app.ServiceProvider.GetRequiredService<TrackerManager>();

				await trackerManager.InitializeAsync();
			}
		}
		catch
		{
			// Ignore all exceptions here
		}
	}
}