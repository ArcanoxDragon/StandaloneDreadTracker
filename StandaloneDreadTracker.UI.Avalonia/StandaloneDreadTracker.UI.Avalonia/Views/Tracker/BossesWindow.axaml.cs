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
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Tracker;

public partial class BossesWindow : ReactiveWindow<TrackerViewModel>
{
	private static readonly TimeSpan SaveSettingsThrottleTime = TimeSpan.FromSeconds(1.0);

	private readonly IServiceScope? serviceScope;

	public BossesWindow()
		: this(null, null) { }

	public BossesWindow(IServiceScope? serviceScope, TrackerViewModel? viewModel)
	{
		this.serviceScope = serviceScope;

		ViewModel = viewModel;

		if (ViewModel is { LastBossWindowPosition: { IsEmpty: false } position })
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
	private async Task SaveWindowPositionAsync(PixelPoint position)
	{
		if (ViewModel is not { } viewModel)
			return;

		var systemPoint = new System.Drawing.Point(position.X, position.Y);

		viewModel.LastBossWindowPosition = systemPoint;

		try
		{
			var settingsManager = Services.GetRequiredService<ISettingsManager>();

			await settingsManager.ModifyAsync(settings => {
				var tracker = settings.Trackers.Find(t => string.Equals(t.Name, viewModel.Name));

				tracker?.LastBossWindowPosition = systemPoint;
			});
		}
		catch
		{
			// Ignore all exceptions here
		}
	}
}