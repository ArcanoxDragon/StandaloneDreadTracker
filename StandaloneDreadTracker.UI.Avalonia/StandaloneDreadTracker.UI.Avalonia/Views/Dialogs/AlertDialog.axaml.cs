using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Dialogs;

public partial class AlertDialog : ReactiveWindow<ViewModelBase>
{
	#region Properties

	public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<AlertDialog, string>(
		nameof(Message),
		string.Empty,
		validate: value => value != null!);

	public static readonly StyledProperty<string> ButtonTextProperty = AvaloniaProperty.Register<AlertDialog, string>(
		nameof(ButtonText),
		"_Ok",
		validate: value => value != null!);

	#endregion

	public AlertDialog()
	{
		InitializeComponent();

		if (Design.IsDesignMode)
		{
			Message = "My dialog message";
			ButtonText = "My Button";
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
	/// Gets or sets the text shown on the dismiss button.
	/// </summary>
	public string ButtonText
	{
		get => GetValue(ButtonTextProperty);
		set => SetValue(ButtonTextProperty, value);
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public void OnButtonClicked() => Close();
}