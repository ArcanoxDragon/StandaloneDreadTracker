using System;
using Avalonia.Data.Converters;
using ReactiveUI.Avalonia;
using StandaloneDreadTracker.App.Utility;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.Views.Dialogs;

public partial class EditTrackerDialog : ReactiveWindow<EditTrackerViewModel>
{
	public static FuncValueConverter<TrackerTargetType, string> TargetTypeDisplayTextConverter { get; } = new(GetTargetTypeDisplayText);

	public EditTrackerDialog()
		: this(null) { }

	public EditTrackerDialog(TrackerViewModel? existingTracker)
	{
		ViewModel = new EditTrackerViewModel();

		// If an existing tracker is being edited, we COPY the settings to the view model's tracker.
		// We don't want to be changing the "live" tracker instance while the dialog is open, or
		// the user wouldn't be able to truly cancel.
		existingTracker?.CopySettingsTo(ViewModel.Tracker);

		InitializeComponent();
	}

	/// <summary>
	/// Copies the settings from the dialog's view model to the <paramref name="other"/> view model.
	/// </summary>
	public void ApplySettingsTo(TrackerViewModel other)
	{
		if (ViewModel is not { } vm)
			return;

		other.Name = vm.Tracker.Name;
		other.TargetType = vm.Tracker.TargetType;

		// Clear out "Address" if type is not remote, even if there is an address in the (hidden) text box
		other.TargetAddress = vm.Tracker.TargetType switch {
			TrackerTargetType.Remote => vm.Tracker.TargetAddress,
			_                        => null,
		};
	}

	public void OnPositiveButtonClicked() => Close(true);
	public void OnNegativeButtonClicked() => Close(false);

	protected override void OnOpened(EventArgs e)
	{
		base.OnOpened(e);
		this.NameTextBox.Focus();
		this.NameTextBox.SelectAll();
	}

	private static string GetTargetTypeDisplayText(TrackerTargetType type)
		=> type == TrackerTargetType.Unknown ? "Select a type..." : type.DisplayText;
}