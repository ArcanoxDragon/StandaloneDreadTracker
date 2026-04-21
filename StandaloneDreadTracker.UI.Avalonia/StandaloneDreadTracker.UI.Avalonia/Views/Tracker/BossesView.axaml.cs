using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.Utility;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Tracker;

public partial class BossesView : UserControl
{
	public static readonly StyledProperty<int> BossIconSizeProperty = AvaloniaProperty.Register<BossesView, int>(nameof(BossIconSize), defaultValue: TrackerSettings.DefaultBossIconSize);

	public static readonly StyledProperty<TrackerOrientation> OrientationProperty = AvaloniaProperty.Register<BossesView, TrackerOrientation>(
		nameof(Orientation),
		defaultValue: TrackerOrientation.Horizontal,
		validate: o => o is TrackerOrientation.Horizontal or TrackerOrientation.Vertical);

	private const string HorizontalRowDefinitions    = "*,*";
	private const string HorizontalColumnDefinitions = "*,*,*,*,*,*";

	private const string VerticalRowDefinitions    = "*,*,*,*,*,*";
	private const string VerticalColumnDefinitions = "*,*";

	public BossesView()
	{
		InitializeComponent();
		UpdateGridLayout();

		this.WhenAnyValue(v => v.Orientation)
			.ObserveOn(RxSchedulers.MainThreadScheduler)
			.Subscribe(_ => UpdateGridLayout());
	}

	public int BossIconSize
	{
		get => GetValue(BossIconSizeProperty);
		set => SetValue(BossIconSizeProperty, value);
	}

	public TrackerOrientation Orientation
	{
		get => GetValue(OrientationProperty);
		set => SetValue(OrientationProperty, value);
	}

	private void UpdateGridLayout()
	{
		var isVertical = Orientation == TrackerOrientation.Vertical;

		this.BossesGrid.RowDefinitions = new RowDefinitions(isVertical ? VerticalRowDefinitions : HorizontalRowDefinitions);
		this.BossesGrid.ColumnDefinitions = new ColumnDefinitions(isVertical ? VerticalColumnDefinitions : HorizontalColumnDefinitions);
	}
}