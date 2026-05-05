using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace StandaloneDreadTracker.UI.Avalonia.Converters;

public sealed class HexToColorConverter : AvaloniaObject, IValueConverter
{
	public static readonly DirectProperty<HexToColorConverter, Color> DefaultColorProperty = AvaloniaProperty.RegisterDirect<HexToColorConverter, Color>(
		nameof(DefaultColor),
		static c => c.DefaultColor,
		static (c, value) => c.DefaultColor = value
	);

	public Color DefaultColor { get; set; } = Colors.Transparent;

	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not string colorString)
			return DefaultColor;

		return Color.TryParse(colorString, out var color) ? color : DefaultColor;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not Color color)
			return string.Empty;

		var colorValue = color.ToUInt32();

		if (color.A == byte.MaxValue)
		{
			colorValue &= 0xFFFFFF;
			return $"#{colorValue:x6}";
		}

		return $"#{colorValue:x8}";
	}
}