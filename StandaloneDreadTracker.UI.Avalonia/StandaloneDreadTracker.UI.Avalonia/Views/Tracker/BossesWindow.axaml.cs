using System;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.Utility;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Tracker;

public partial class BossesWindow : ReactiveWindow<TrackerViewModel>
{
	private static readonly TimeSpan SaveSettingsThrottleTime = TimeSpan.FromSeconds(1.0);

	public BossesWindow()
		: this(null, null) { }

	public BossesWindow(TrackerManager? trackerManager, TrackerViewModel? viewModel)
	{
		TrackerManager = trackerManager;
		ViewModel = viewModel;

		if (ViewModel is { LastBossWindowPosition: { IsEmpty: false } position })
		{
			// We don't necessarily know the exact window size (it will not have measured yet).
			// All we really need to be sure of is that the window's title bar isn't way off screen somewhere,
			// so we can use a 100x100 square starting at the proposed X/Y coordinates. If that square is
			// entirely within a screen's bounds, it's "good enough".
			var proposedRect = new PixelRect(position.X, position.Y, 100, 100);
			var anyScreensFit = Screens.All.Any(s => s.Bounds.Contains(proposedRect));

			if (anyScreensFit)
			{
				Position = proposedRect.TopLeft;
				WindowStartupLocation = WindowStartupLocation.Manual;
			}
		}

		// Allow the window to take focus so global key events can occur
		Focusable = true;

		InitializeComponent();

		this.WhenActivated(disposables => {
			Observable.FromEventPattern<PixelPointEventArgs>(
					h => PositionChanged += h,
					h => PositionChanged -= h)
				.Select(p => p.EventArgs.Point)
				.DistinctUntilChanged()
				.Throttle(SaveSettingsThrottleTime)
				.ObserveOn(RxSchedulers.MainThreadScheduler)
				.InvokeCommand(SaveWindowPositionCommand)
				.DisposeWith(disposables);
		});
	}

	private TrackerManager? TrackerManager { get; }

	[ReactiveCommand]
	private async Task SaveWindowPositionAsync(PixelPoint position)
	{
		if (ViewModel is not { } tracker)
			return;

		var systemPoint = new System.Drawing.Point(position.X, position.Y);

		tracker.LastBossWindowPosition = systemPoint;

		if (TrackerManager is null)
			return;

		try
		{
			await TrackerManager.UpdateTrackerAsync(tracker, settings => {
				settings.LastBossWindowPosition = tracker.LastBossWindowPosition;
			});
		}
		catch
		{
			// Ignore all exceptions here
		}
	}
}