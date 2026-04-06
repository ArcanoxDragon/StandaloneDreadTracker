using ReactiveUI.Avalonia;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views;

public partial class TrackerTargetView : ReactiveUserControl<TrackerTargetViewModel>
{
    public TrackerTargetView()
    {
        InitializeComponent();
    }
}