using Avalonia.Data.Converters;
using ReactiveUI.Avalonia;
using StandaloneDreadTracker.App.Utility;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Dialogs;

public partial class TrackerSettingsDialog : ReactiveWindow<TrackerSettingsViewModel>
{
	public static FuncValueConverter<TrackerTargetType, string> TargetTypeDisplayTextConverter { get; } = new(GetTargetTypeDisplayText);

	/// <summary>
	/// Used to "revert" a tracker upon cancelling.
	/// </summary>
	private readonly TrackerViewModel trackerSnapshot = new();

	private readonly bool didSnapshot;

	public TrackerSettingsDialog()
		: this(null) { }

	public TrackerSettingsDialog(TrackerViewModel? tracker)
	{
		ViewModel = new TrackerSettingsViewModel();

		if (tracker != null)
		{
			// The ViewModel will edit the tracker "live", so in order to support cancellation,
			// we need to snapshot the "before" state of the tracker right now.
			tracker.CopySettingsTo(this.trackerSnapshot);
			this.didSnapshot = true;
			ViewModel.Tracker = tracker;
		}

		InitializeComponent();
	}

	public void OnPositiveButtonClicked() => Close(true);

	public void OnNegativeButtonClicked()
	{
		if (this.didSnapshot && ViewModel is { Tracker: { } editingTracker })
			// Restore the snapshot when cancelling
			this.trackerSnapshot.CopySettingsTo(editingTracker);

		Close(false);
	}

	private static string GetTargetTypeDisplayText(TrackerTargetType type)
		=> type == TrackerTargetType.Unknown ? "Select a type..." : type.DisplayText;
}