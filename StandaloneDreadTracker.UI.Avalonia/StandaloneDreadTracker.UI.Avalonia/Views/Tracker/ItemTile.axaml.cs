using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Media;
using ReactiveUI;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Tracker;

public partial class ItemTile : UserControl, IActivatableView
{
	public const int DefaultIconSize = TrackerSettings.DefaultItemIconSize;

	public static readonly StyledProperty<bool>    IsCollectedProperty         = AvaloniaProperty.Register<ItemTile, bool>(nameof(IsCollected));
	public static readonly StyledProperty<bool>    IsFirstInGroupProperty      = AvaloniaProperty.Register<ItemTile, bool>(nameof(IsFirstInGroup));
	public static readonly StyledProperty<bool>    IsLastInGroupProperty       = AvaloniaProperty.Register<ItemTile, bool>(nameof(IsLastInGroup));
	public static readonly StyledProperty<IBrush?> GroupBackgroundProperty     = AvaloniaProperty.Register<ItemTile, IBrush?>(nameof(GroupBackground), Brushes.Transparent);
	public static readonly StyledProperty<bool>    ShowGroupBackgroundProperty = AvaloniaProperty.Register<ItemTile, bool>(nameof(ShowGroupBackground), defaultValue: true);
	public static readonly StyledProperty<IImage?> IconProperty                = AvaloniaProperty.Register<ItemTile, IImage?>(nameof(Icon));
	public static readonly StyledProperty<int>     IconSizeProperty            = AvaloniaProperty.Register<ItemTile, int>(nameof(IconSize), defaultValue: DefaultIconSize);
	public static readonly StyledProperty<string?> AuxTextProperty             = AvaloniaProperty.Register<ItemTile, string?>(nameof(AuxText));
	public static readonly StyledProperty<bool>    ShowLocationHintProperty    = AvaloniaProperty.Register<ItemTile, bool>(nameof(ShowLocationHint), defaultValue: true);

	public static readonly StyledProperty<char> LocationHintProperty = AvaloniaProperty.Register<ItemTile, char>(
		nameof(LocationHint),
		'?',
		defaultBindingMode: BindingMode.TwoWay,
		validate: c => c is >= 'A' and <= 'H' or 'S' or '?');

	public static readonly FuncValueConverter<int, CornerRadius> IconCornerRadiusConverter = new(iconSize => new CornerRadius(iconSize / 2.0));

	public ItemTile()
	{
		InitializeComponent();

		this.WhenActivated(disposables => {
			var window = TopLevel.GetTopLevel(this);

			if (window != null)
			{
				window.AddHandler(KeyDownEvent, OnGlobalKeyDown, handledEventsToo: true);
				Disposable.Create(() => window.RemoveHandler(KeyDownEvent, OnGlobalKeyDown)).DisposeWith(disposables);
			}
		});
	}

	public bool IsCollected
	{
		get => GetValue(IsCollectedProperty);
		set => SetValue(IsCollectedProperty, value);
	}

	public char LocationHint
	{
		get => GetValue(LocationHintProperty);
		set => SetValue(LocationHintProperty, value);
	}

	public bool IsFirstInGroup
	{
		get => GetValue(IsFirstInGroupProperty);
		set => SetValue(IsFirstInGroupProperty, value);
	}

	public bool IsLastInGroup
	{
		get => GetValue(IsLastInGroupProperty);
		set => SetValue(IsLastInGroupProperty, value);
	}

	public IBrush? GroupBackground
	{
		get => GetValue(GroupBackgroundProperty);
		set => SetValue(GroupBackgroundProperty, value);
	}

	public bool ShowGroupBackground
	{
		get => GetValue(ShowGroupBackgroundProperty);
		set => SetValue(ShowGroupBackgroundProperty, value);
	}

	public IImage? Icon
	{
		get => GetValue(IconProperty);
		set => SetValue(IconProperty, value);
	}

	public int IconSize
	{
		get => GetValue(IconSizeProperty);
		set => SetValue(IconSizeProperty, value);
	}

	public string? AuxText
	{
		get => GetValue(AuxTextProperty);
		set => SetValue(AuxTextProperty, value);
	}

	public bool ShowLocationHint
	{
		get => GetValue(ShowLocationHintProperty);
		set => SetValue(ShowLocationHintProperty, value);
	}

	protected void OnGlobalKeyDown(object? sender, KeyEventArgs e)
	{
		if (!IsPointerOver)
			return;

		if (e.KeySymbol is [var letter] && char.ToUpper(letter) is >= 'A' and <= 'H' or 'S' or '?')
		{
			// Normal region letter (or "S" for starting)
			LocationHint = char.ToUpper(letter);
		}
		else if (e.PhysicalKey is PhysicalKey.Delete or PhysicalKey.Backspace)
		{
			// Clear the letter
			LocationHint = '?';
		}
	}

	protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
	{
		base.OnPointerWheelChanged(e);

		if (e.Delta.Y <= -1) // Scroll wheel down
		{
			LocationHint = LocationHint switch {
				>= 'A' and < 'H' => (char) ( LocationHint + 1 ),

				'?' => 'A',
				'H' => 'S',
				_   => '?',
			};
		}
		else if (e.Delta.Y >= 1) // Scroll wheel up
		{
			LocationHint = LocationHint switch {
				> 'A' and <= 'H' => (char) ( LocationHint - 1 ),

				'?' => 'S',
				'S' => 'H',
				_   => '?',
			};
		}
	}
}