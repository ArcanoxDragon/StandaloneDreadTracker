using Avalonia;
using Avalonia.Controls;

namespace StandaloneDreadTracker.UI.Avalonia.Utilities;

public static class WindowHelpers
{
	static WindowHelpers()
	{
		TopLevel.ActualTransparencyLevelProperty.Changed.AddClassHandler<TopLevel>(OnActualTransparencyLevelChanged);
	}

	public static readonly AttachedProperty<bool> IsUsingMicaProperty        = AvaloniaProperty.RegisterAttached<TopLevel, bool>("IsUsingMica", typeof(WindowHelpers));
	public static readonly AttachedProperty<bool> IsUsingAcrylicBlurProperty = AvaloniaProperty.RegisterAttached<TopLevel, bool>("IsUsingAcrylicBlur", typeof(WindowHelpers));

	public static bool GetIsUsingMica(TopLevel target)
		=> target.GetValue(IsUsingMicaProperty);

	public static bool GetIsUsingAcrylicBlur(TopLevel target)
		=> target.GetValue(IsUsingAcrylicBlurProperty);

	private static void OnActualTransparencyLevelChanged(TopLevel target, AvaloniaPropertyChangedEventArgs args)
	{
		target.SetValue(IsUsingMicaProperty, target.ActualTransparencyLevel == WindowTransparencyLevel.Mica);
		target.SetValue(IsUsingAcrylicBlurProperty, target.ActualTransparencyLevel == WindowTransparencyLevel.AcrylicBlur);
	}
}