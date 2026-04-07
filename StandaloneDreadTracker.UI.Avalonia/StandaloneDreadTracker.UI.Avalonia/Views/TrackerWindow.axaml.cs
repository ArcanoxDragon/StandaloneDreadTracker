using ReactiveUI.Avalonia;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views;

public partial class TrackerWindow : ReactiveWindow<TrackerViewModel>
{
	public TrackerWindow()
		: this(null) { }

	public TrackerWindow(TrackerViewModel? viewModel)
	{
		ViewModel = viewModel;

		InitializeComponent();
	}
}