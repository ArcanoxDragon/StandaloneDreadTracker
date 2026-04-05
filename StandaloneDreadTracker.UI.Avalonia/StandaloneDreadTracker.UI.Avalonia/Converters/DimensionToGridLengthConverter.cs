using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace StandaloneDreadTracker.UI.Avalonia.Converters;

internal class DimensionToGridLengthConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is null || targetType != typeof(GridLength))
			return null;

		try
		{
			var dimension = System.Convert.ToDouble(value);

			return new GridLength(dimension, GridUnitType.Pixel);
		}
		catch
		{
			return null;
		}
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not GridLength { GridUnitType: GridUnitType.Pixel } gridLength)
			return null;

		return System.Convert.ChangeType(gridLength.Value, typeof(double));
	}
}