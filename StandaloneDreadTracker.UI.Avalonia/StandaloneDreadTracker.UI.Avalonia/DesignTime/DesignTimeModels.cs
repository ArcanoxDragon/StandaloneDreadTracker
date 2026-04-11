using Avalonia.Controls;
using StandaloneDreadTracker.App.Utility;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.DesignTime;

internal static class DesignTimeModels
{
	static DesignTimeModels()
	{
		if (!Design.IsDesignMode)
			return;

		RemoteTrackerViewModel = new TrackerViewModel {
			Name = "My Switch",
			TargetType = TrackerTargetType.Remote,
			TargetAddress = "192.168.0.42",
		};
		LocalTrackerViewModel = new TrackerViewModel {
			Name = "Ryujinx",
			TargetType = TrackerTargetType.LocalEmulator,
		};
		MainViewModel = new MainViewModel {
			TrackerTargets = { RemoteTrackerViewModel, LocalTrackerViewModel },
		};
	}

	public static TrackerViewModel? RemoteTrackerViewModel { get; }
	public static TrackerViewModel? LocalTrackerViewModel  { get; }
	public static MainViewModel?    MainViewModel          { get; }
}