using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.Extensions;
using StandaloneDreadTracker.App.Utility;
using StandaloneDreadTracker.App.ViewModels;
using StandaloneDreadTracker.UI.Avalonia.Views.Dialogs;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Tracker;

public partial class TrackerWindow : ReactiveWindow<TrackerViewModel>
{
	private static readonly TimeSpan SaveSettingsThrottleTime = TimeSpan.FromSeconds(1.0);

	private readonly IServiceScope?                       serviceScope;
	private readonly SerialSubscription<TrackerViewModel> trackerSubscription;

	private BossesWindow? bossesWindow;

	public TrackerWindow()
		: this(null, null) { }

	public TrackerWindow(IServiceScope? serviceScope, TrackerViewModel? viewModel)
	{
		this.serviceScope = serviceScope;
		this.trackerSubscription = new SerialSubscription<TrackerViewModel>(SubscribeTracker);

		TrackerManager = serviceScope?.ServiceProvider.GetRequiredService<TrackerManager>();
		ViewModel = viewModel;

		if (ViewModel is { LastMainWindowSize: { IsEmpty: false } size })
			ClientSize = new Size(size.Width, size.Height);
		else
			ClientSize = new Size(600, 400);

		if (ViewModel is { LastMainWindowPosition: { IsEmpty: false } position })
		{
			var proposedRect = new PixelRect(position.X, position.Y, (int) ClientSize.Width, (int) ClientSize.Height);
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
			// Subscribe to observables on new trackers
			this.WhenAnyValue(w => w.ViewModel)
				.SubscribeWith(this.trackerSubscription)
				.DisposeWith(disposables);

			// Keep the boss window ViewModel in-sync with this one
			this.WhenAnyValue(w => w.ViewModel)
				.Subscribe(vm => this.bossesWindow?.ViewModel = vm)
				.DisposeWith(disposables);

			// Save window size to settings when it changes (throttled)
			this.WhenAnyValue(w => w.ClientSize)
				.DistinctUntilChanged()
				.Throttle(SaveSettingsThrottleTime)
				.ObserveOn(RxSchedulers.MainThreadScheduler)
				.InvokeCommand(SaveWindowSizeCommand)
				.DisposeWith(disposables);

			// Save window position to settings when it changes (throttled)
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

	protected override void OnUnloaded(RoutedEventArgs e)
	{
		base.OnUnloaded(e);
		this.serviceScope?.Dispose();
	}

	protected override void OnClosing(WindowClosingEventArgs e)
	{
		this.bossesWindow?.Closed -= OnBossWindowClosed;
		base.OnClosing(e);
	}

	[ReactiveCommand]
	private async Task EditTrackerSettingsAsync()
	{
		if (ViewModel is not { } tracker || TrackerManager is null)
			return;

		var dialog = new TrackerSettingsDialog(tracker) {
			Title = "Tracker Settings",
		};
		var result = await dialog.ShowDialog<bool>(this);

		if (!result)
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
	private async Task PopOutBossWindowAsync()
	{
		var isPoppedOut = this.bossesWindow is { IsVisible: true };

		if (isPoppedOut)
			return;

		// Shrink the window by the height of the bosses panel
		Height -= this.BossesPanel.Bounds.Height;

		// Actually pop the panel out (and save the change to the settings file)
		await ChangeBossWindowPoppedOutAsync(true);
	}

	private async Task ChangeBossWindowPoppedOutAsync(bool popOut)
	{
		if (ViewModel is not { } tracker)
			return;

		tracker.PopOutBossSection = popOut;

		if (TrackerManager is null)
			return;

		try
		{
			await TrackerManager.UpdateTrackerAsync(tracker, settings => {
				settings.PopOutBossSection = tracker.PopOutBossSection;
			});
		}
		catch
		{
			// Ignore all exceptions here
		}
	}

	[ReactiveCommand]
	private async Task SaveWindowSizeAsync(Size size)
	{
		if (ViewModel is not { } tracker)
			return;

		var systemSize = new System.Drawing.Size((int) size.Width, (int) size.Height);

		tracker.LastMainWindowSize = systemSize;

		if (TrackerManager is null)
			return;

		try
		{
			await TrackerManager.UpdateTrackerAsync(tracker, settings => {
				settings.LastMainWindowSize = tracker.LastMainWindowSize;
			});
		}
		catch
		{
			// Ignore all exceptions here
		}
	}

	[ReactiveCommand]
	private async Task SaveWindowPositionAsync(PixelPoint position)
	{
		if (ViewModel is not { } tracker)
			return;

		var systemPoint = new System.Drawing.Point(position.X, position.Y);

		tracker.LastMainWindowPosition = systemPoint;

		if (TrackerManager is null)
			return;

		try
		{
			await TrackerManager.UpdateTrackerAsync(tracker, settings => {
				settings.LastMainWindowPosition = tracker.LastMainWindowPosition;
			});
		}
		catch
		{
			// Ignore all exceptions here
		}
	}

	private void SubscribeTracker(TrackerViewModel tracker, CompositeDisposable disposables)
	{
		tracker.WhenAnyValue(t => t.PopOutBossSection)
			.Subscribe(UpdateBossWindowVisibility)
			.DisposeWith(disposables);
	}

	private void UpdateBossWindowVisibility(bool newVisibility)
	{
		var currentVisibility = this.bossesWindow is { IsVisible: true };

		if (newVisibility == currentVisibility)
			return;

		if (newVisibility)
		{
			this.bossesWindow = new BossesWindow(TrackerManager, ViewModel);
			this.bossesWindow.Closed += OnBossWindowClosed;
			this.bossesWindow.Show(this);
		}
		else
		{
			this.bossesWindow?.Closed -= OnBossWindowClosed;
			this.bossesWindow?.Close();
			this.bossesWindow = null;
		}
	}

	private async void OnBossWindowClosed(object? sender, EventArgs e)
	{
		try
		{
			await ChangeBossWindowPoppedOutAsync(false);

			// Wait for the boss panel to be measured so we know its height
			while (!this.BossesPanel.IsMeasureValid)
				await RxSchedulers.MainThreadScheduler.Yield();

			// Expand the window by the height of the bosses panel
			Height += this.BossesPanel.Bounds.Height;
		}
		catch
		{
			// Ignore all errors
		}
	}
}