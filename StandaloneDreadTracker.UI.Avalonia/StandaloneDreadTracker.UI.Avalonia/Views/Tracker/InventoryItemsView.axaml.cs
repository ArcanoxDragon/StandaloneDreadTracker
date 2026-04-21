using Avalonia;
using Avalonia.Controls;
using StandaloneDreadTracker.App.Configuration;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Tracker;

internal partial class InventoryItemsView : UserControl
{
	public static readonly StyledProperty<int>  ItemIconSizeProperty         = AvaloniaProperty.Register<InventoryItemsView, int>(nameof(ItemIconSize), defaultValue: TrackerSettings.DefaultItemIconSize);
	public static readonly StyledProperty<bool> ShowGroupBackgroundsProperty = AvaloniaProperty.Register<InventoryItemsView, bool>(nameof(ShowGroupBackgrounds), defaultValue: true);

	public InventoryItemsView()
	{
		InitializeComponent();
	}

	public int ItemIconSize
	{
		get => GetValue(ItemIconSizeProperty);
		set => SetValue(ItemIconSizeProperty, value);
	}

	public bool ShowGroupBackgrounds
	{
		get => GetValue(ShowGroupBackgroundsProperty);
		set => SetValue(ShowGroupBackgroundsProperty, value);
	}
}