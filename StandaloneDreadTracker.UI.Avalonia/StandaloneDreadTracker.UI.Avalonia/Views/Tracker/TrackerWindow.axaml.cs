using System;
using System.Linq;
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
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.Utility;
using StandaloneDreadTracker.App.ViewModels;
using StandaloneDreadTracker.UI.Avalonia.Views.Dialogs;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Tracker;

public partial class TrackerWindow : ReactiveWindow<TrackerViewModel>
{
	private static readonly TimeSpan SaveSettingsThrottleTime = TimeSpan.FromSeconds(1.0);

	private readonly IServiceScope? serviceScope;

	public TrackerWindow()
		: this(null, null) { }

	public TrackerWindow(IServiceScope? serviceScope, TrackerViewModel? viewModel)
	{
		this.serviceScope = serviceScope;
		ViewModel = viewModel;

		if (viewModel is { LastWindowSize: { IsEmpty: false } size })
			ClientSize = new Size(size.Width, size.Height);
		else
			ClientSize = new Size(600, 400);

		if (viewModel is { LastWindowPosition: { IsEmpty: false } position })
		{
			var proposedRect = new PixelRect(position.X, position.Y, (int) ClientSize.Width, (int) ClientSize.Height);
			var anyScreensFit = Screens.All.Any(s => s.Bounds.Contains(proposedRect));

			if (anyScreensFit)
				Position = proposedRect.TopLeft;
		}

		// Allow the window to take focus so global key events can occur
		Focusable = true;

		InitializeComponent();

		this.WhenActivated(disposables => {
			this.WhenAnyValue(w => w.ClientSize)
				.DistinctUntilChanged()
				.Throttle(SaveSettingsThrottleTime)
				.ObserveOn(RxSchedulers.MainThreadScheduler)
				.InvokeCommand(SaveWindowSizeCommand)
				.DisposeWith(disposables);

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

	private IServiceProvider Services
		=> this.serviceScope?.ServiceProvider ?? throw new InvalidOperationException("Window was not initialized with a service provider");

	protected override void OnUnloaded(RoutedEventArgs e)
	{
		base.OnUnloaded(e);
		this.serviceScope?.Dispose();
	}

	[ReactiveCommand]
	private async Task EditTrackerSettingsAsync()
	{
		if (ViewModel is not { } tracker)
			return;

		var dialog = new TrackerSettingsDialog(tracker) {
			Title = "Tracker Settings",
		};
		var result = await dialog.ShowDialog<bool>(this);

		if (!result)
			return;

		try
		{
			var settingsManager = Services.GetRequiredService<ISettingsManager>();

			await settingsManager.ModifyAsync(settings => {
				var trackerSettings = settings.Trackers.Find(t => string.Equals(t.Name, tracker.Name));

				if (trackerSettings != null)
					tracker.CopySettingsTo(trackerSettings);
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
		if (ViewModel is not { } viewModel)
			return;

		var systemSize = new System.Drawing.Size((int) size.Width, (int) size.Height);

		viewModel.LastWindowSize = systemSize;

		try
		{
			var settingsManager = Services.GetRequiredService<ISettingsManager>();

			await settingsManager.ModifyAsync(settings => {
				var tracker = settings.Trackers.Find(t => string.Equals(t.Name, viewModel.Name));

				tracker?.LastWindowSize = systemSize;
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
		if (ViewModel is not { } viewModel)
			return;

		var systemPoint = new System.Drawing.Point(position.X, position.Y);

		viewModel.LastWindowPosition = systemPoint;

		try
		{
			var settingsManager = Services.GetRequiredService<ISettingsManager>();

			await settingsManager.ModifyAsync(settings => {
				var tracker = settings.Trackers.Find(t => string.Equals(t.Name, viewModel.Name));

				tracker?.LastWindowPosition = systemPoint;
			});
		}
		catch
		{
			// Ignore all exceptions here
		}
	}
}