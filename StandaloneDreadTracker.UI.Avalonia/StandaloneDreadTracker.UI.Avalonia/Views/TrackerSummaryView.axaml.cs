using System.Windows.Input;
using Avalonia;
using ReactiveUI.Avalonia;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views;

public partial class TrackerSummaryView : ReactiveUserControl<TrackerViewModel>
{
	public static readonly StyledProperty<ICommand?> LoadSessionCommandProperty = AvaloniaProperty.Register<TrackerSummaryView, ICommand?>(nameof(LoadSessionCommand));
	public static readonly StyledProperty<ICommand?> NewSessionCommandProperty  = AvaloniaProperty.Register<TrackerSummaryView, ICommand?>(nameof(NewSessionCommand));
	public static readonly StyledProperty<ICommand?> EditCommandProperty        = AvaloniaProperty.Register<TrackerSummaryView, ICommand?>(nameof(EditCommand));
	public static readonly StyledProperty<ICommand?> DeleteCommandProperty      = AvaloniaProperty.Register<TrackerSummaryView, ICommand?>(nameof(DeleteCommand));

	public TrackerSummaryView()
	{
		InitializeComponent();
	}

	public ICommand? LoadSessionCommand
	{
		get => GetValue(LoadSessionCommandProperty);
		set => SetValue(LoadSessionCommandProperty, value);
	}

	public ICommand? NewSessionCommand
	{
		get => GetValue(NewSessionCommandProperty);
		set => SetValue(NewSessionCommandProperty, value);
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