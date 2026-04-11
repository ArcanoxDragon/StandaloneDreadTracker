using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace StandaloneDreadTracker.UI.Avalonia.Converters;

internal class BooleanToOpacityConverter : IValueConverter
{
	public double FalseOpacity { get; set; }
	public double TrueOpacity  { get; set; } = 1.0;

	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is true ? TrueOpacity : FalseOpacity;

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is double opacity && opacity >= TrueOpacity;
}