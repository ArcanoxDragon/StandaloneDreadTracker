using System.Windows.Input;
using Avalonia;
using ReactiveUI.Avalonia;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views;

public partial class TrackerTargetView : ReactiveUserControl<TrackerViewModel>
{
	public static readonly StyledProperty<ICommand?> OpenCommandProperty   = AvaloniaProperty.Register<TrackerTargetView, ICommand?>(nameof(OpenCommand));
	public static readonly StyledProperty<ICommand?> EditCommandProperty   = AvaloniaProperty.Register<TrackerTargetView, ICommand?>(nameof(EditCommand));
	public static readonly StyledProperty<ICommand?> DeleteCommandProperty = AvaloniaProperty.Register<TrackerTargetView, ICommand?>(nameof(DeleteCommand));

	public TrackerTargetView()
	{
		InitializeComponent();
	}

	public ICommand? OpenCommand
	{
		get => GetValue(OpenCommandProperty);
		set => SetValue(OpenCommandProperty, value);
	}

	public ICommand? EditCommand
	{
		get => GetValue(EditCommandProperty);
		set => SetValue(EditCommandProperty, value);
	}

	public ICommand? DeleteCommand
	{
		get => GetValue(DeleteCommandProperty);
		set => SetValue(DeleteCommandProperty, value);
	}
}