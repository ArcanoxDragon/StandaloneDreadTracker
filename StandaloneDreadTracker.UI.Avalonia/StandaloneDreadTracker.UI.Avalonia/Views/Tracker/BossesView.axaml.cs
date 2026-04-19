using Avalonia;
using Avalonia.Controls;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Tracker;

public partial class BossesView : UserControl
{
	public static readonly StyledProperty<int> BossIconSizeProperty = AvaloniaProperty.Register<BossesView, int>(nameof(BossIconSize), defaultValue: TrackerSettings.DefaultBossIconSize);

	public BossesView()
	{
		InitializeComponent();
	}

	public int BossIconSize
	{
		get => GetValue(BossIconSizeProperty);
		set => SetValue(BossIconSizeProperty, value);
	}
}