using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Dialogs;

public partial class ConfirmDialog : ReactiveWindow<ViewModelBase>
{
	#region Properties

	public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<ConfirmDialog, string>(
		nameof(Message),
		string.Empty,
		validate: value => value != null!);

	public static readonly StyledProperty<string> PositiveTextProperty = AvaloniaProperty.Register<ConfirmDialog, string>(
		nameof(PositiveText),
		"_Yes",
		validate: value => value != null!);

	public static readonly StyledProperty<string> NegativeTextProperty = AvaloniaProperty.Register<ConfirmDialog, string>(
		nameof(NegativeText),
		"_No",
		validate: value => value != null!);

	public static readonly StyledProperty<bool> PositiveButtonAccentProperty = AvaloniaProperty.Register<ConfirmDialog, bool>(
		nameof(PositiveButtonAccent),
		true);

	#endregion

	public ConfirmDialog()
	{
		InitializeComponent();

		if (Design.IsDesignMode)
		{
			Message = "My dialog message";
			PositiveText = "Positive Button";
			NegativeText = "Negative Button";
		}
	}

	/// <summary>
	/// Gets or sets the message that is shown in the body of the dialog.
	/// </summary>
	public string Message
	{
		get => GetValue(MessageProperty);
		set => SetValue(MessageProperty, value);
	}

	/// <summary>
	/// Gets or sets the text shown on the "positive" or "confirm" button.
	/// </summary>
	public string PositiveText
	{
		get => GetValue(PositiveTextProperty);
		set => SetValue(PositiveTextProperty, value);
	}

	/// <summary>
	/// Gets or sets the text shown on the "negative" or "cancel" button.
	/// </summary>
	public string NegativeText
	{
		get => GetValue(NegativeTextProperty);
		set => SetValue(NegativeTextProperty, value);
	}

	/// <summary>
	/// Gets or sets whether or not the "positive" button uses the accent color.
	/// </summary>
	public bool PositiveButtonAccent
	{
		get => GetValue(PositiveButtonAccentProperty);
		set => SetValue(PositiveButtonAccentProperty, value);
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public void OnPositiveButtonClicked() => Close(true);
	public void OnNegativeButtonClicked() => Close(false);
}