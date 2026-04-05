using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace StandaloneDreadTracker.UI.Avalonia.Converters;

internal class ChooseConverter : AvaloniaObject, IValueConverter
{
	public static readonly DirectProperty<ChooseConverter, object?> ValueWhenFalseProperty = AvaloniaProperty.RegisterDirect<ChooseConverter, object?>(
		nameof(ValueWhenFalse),
		static c => c.ValueWhenFalse,
		static (c, value) => c.ValueWhenFalse = value
	);

	public static readonly DirectProperty<ChooseConverter, object?> ValueWhenTrueProperty = AvaloniaProperty.RegisterDirect<ChooseConverter, object?>(
		nameof(ValueWhenTrue),
		static c => c.ValueWhenTrue,
		static (c, value) => c.ValueWhenTrue = value
	);

	public object? ValueWhenFalse { get; set; }
	public object? ValueWhenTrue  { get; set; }

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is true ? ValueWhenTrue : ValueWhenFalse;

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> Equals(value, ValueWhenTrue);
}