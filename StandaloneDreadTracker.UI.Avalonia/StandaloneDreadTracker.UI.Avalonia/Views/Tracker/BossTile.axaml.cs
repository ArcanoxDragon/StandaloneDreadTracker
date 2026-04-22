using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Media;
using ReactiveUI;
using StandaloneDreadTracker.App.Configuration;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Tracker;

public partial class BossTile : UserControl, IActivatableView
{
	public const int DefaultIconSize = TrackerSettings.DefaultBossIconSize;

	public static readonly StyledProperty<bool>      IsDefeatedProperty                 = AvaloniaProperty.Register<BossTile, bool>(nameof(IsDefeated), defaultBindingMode: BindingMode.TwoWay);
	public static readonly StyledProperty<IBrush?>   RegionBorderProperty               = AvaloniaProperty.Register<BossTile, IBrush?>(nameof(RegionBorder), Brushes.Transparent);
	public static readonly StyledProperty<IImage?>   IconProperty                       = AvaloniaProperty.Register<BossTile, IImage?>(nameof(Icon));
	public static readonly StyledProperty<int>       IconSizeProperty                   = AvaloniaProperty.Register<BossTile, int>(nameof(IconSize), defaultValue: DefaultIconSize);
	public static readonly StyledProperty<bool>      ShowDnaHintProperty                = AvaloniaProperty.Register<BossTile, bool>(nameof(ShowDnaHint), defaultValue: true);
	public static readonly StyledProperty<ICommand?> RightClickCommandProperty          = AvaloniaProperty.Register<BossTile, ICommand?>(nameof(RightClickCommand));
	public static readonly StyledProperty<object?>   RightClickCommandParameterProperty = AvaloniaProperty.Register<BossTile, object?>(nameof(RightClickCommandParameter));

	public static readonly StyledProperty<int> DnaHintProperty = AvaloniaProperty.Register<BossTile, int>(
		nameof(DnaHint),
		defaultBindingMode: BindingMode.TwoWay,
		validate: i => i is >= 0 and <= 12);

	public static readonly FuncValueConverter<int, bool> HasDnaHintConverter = new(i => i > 0);

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "The referenced types/properties are strongly referenced elsewhere")]
	public BossTile()
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

	public bool IsDefeated
	{
		get => GetValue(IsDefeatedProperty);
		set => SetValue(IsDefeatedProperty, value);
	}

	public IBrush? RegionBorder
	{
		get => GetValue(RegionBorderProperty);
		set => SetValue(RegionBorderProperty, value);
	}

	public int DnaHint
	{
		get => GetValue(DnaHintProperty);
		set => SetValue(DnaHintProperty, value);
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

	public bool ShowDnaHint
	{
		get => GetValue(ShowDnaHintProperty);
		set => SetValue(ShowDnaHintProperty, value);
	}

	public ICommand? RightClickCommand
	{
		get => GetValue(RightClickCommandProperty);
		set => SetValue(RightClickCommandProperty, value);
	}

	public object? RightClickCommandParameter
	{
		get => GetValue(RightClickCommandParameterProperty);
		set => SetValue(RightClickCommandParameterProperty, value);
	}

	protected void OnGlobalKeyDown(object? sender, KeyEventArgs e)
	{
		if (!IsPointerOver)
			return;

		if (e.KeySymbol is [var character and (>= '0' and <= '9' or '-' or '=')])
		{
			// Setting DNA number
			DnaHint = character switch {
				>= '1' and <= '9' => 1 + ( character - '1' ),
				'0'               => 10,
				'-'               => 11,
				'='               => 12,
				_                 => 0, // Shouldn't be possible
			};
		}
		else if (e.PhysicalKey is PhysicalKey.Delete or PhysicalKey.Backspace)
		{
			// Clearing DNA number
			DnaHint = 0;
		}
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		base.OnPointerReleased(e);

		// Left-clicking to toggle only works when there is no DNA,
		// as bosses with DNA are synchronized to the DNA items.
		if (e.InitialPressMouseButton == MouseButton.Left && DnaHint == 0)
			IsDefeated = !IsDefeated;
		else if (e.InitialPressMouseButton == MouseButton.Right)
			RightClickCommand?.Execute(RightClickCommandParameter);
	}

	protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
	{
		base.OnPointerWheelChanged(e);

		const int MaxValueExclusive = 13; // 0-12, inclusive

		if (e.Delta.Y <= -1) // Scroll wheel down
		{
			DnaHint = ( DnaHint + 1 ) % MaxValueExclusive;
		}
		else if (e.Delta.Y >= 1) // Scroll wheel up
		{
			// Modulus operator won't work right if it goes negative
			DnaHint = DnaHint switch {
				0 => MaxValueExclusive - 1,
				_ => DnaHint - 1,
			};
		}
	}
}