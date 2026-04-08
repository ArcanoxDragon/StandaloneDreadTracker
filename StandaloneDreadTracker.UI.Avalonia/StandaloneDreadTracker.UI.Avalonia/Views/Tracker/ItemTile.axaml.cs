using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using ReactiveUI;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Tracker;

public partial class ItemTile : UserControl, IActivatableView
{
	public static readonly StyledProperty<bool>    IsCollectedProperty     = AvaloniaProperty.Register<ItemTile, bool>(nameof(IsCollected));
	public static readonly StyledProperty<bool>    IsFirstInGroupProperty  = AvaloniaProperty.Register<ItemTile, bool>(nameof(IsFirstInGroup));
	public static readonly StyledProperty<bool>    IsLastInGroupProperty   = AvaloniaProperty.Register<ItemTile, bool>(nameof(IsLastInGroup));
	public static readonly StyledProperty<IBrush?> GroupBackgroundProperty = AvaloniaProperty.Register<ItemTile, IBrush?>(nameof(GroupBackground), Brushes.Transparent);

	public static readonly StyledProperty<char> LocationHintProperty = AvaloniaProperty.Register<ItemTile, char>(
		nameof(LocationHint),
		'?',
		defaultBindingMode: BindingMode.TwoWay,
		validate: c => c is >= 'A' and <= 'H' or 'S' or '?');

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