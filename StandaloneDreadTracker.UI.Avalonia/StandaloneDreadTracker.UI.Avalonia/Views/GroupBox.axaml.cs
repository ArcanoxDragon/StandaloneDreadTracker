using Avalonia;
using Avalonia.Controls;

namespace StandaloneDreadTracker.UI.Avalonia.Views;

public partial class GroupBox : ContentControl
{
	public static readonly StyledProperty<object?>   HeaderProperty  = AvaloniaProperty.Register<GroupBox, object?>(nameof(Header));

	public GroupBox()
	{
		if (Design.IsDesignMode)
			SetValue(HeaderProperty, "Header Text");

		InitializeComponent();
	}

	public object? Header
	{
		get => GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}
}