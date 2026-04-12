using Avalonia.Controls;
using DreadRemoteConnector.Inventory;
using StandaloneDreadTracker.App.Utility;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.UI.Avalonia.DesignTime;

internal static class DesignTimeModels
{
	static DesignTimeModels()
	{
		if (!Design.IsDesignMode)
			return;

		Inventory = new DreadInventory {
			MetroidDna1 = true,
			MetroidDna5 = true,
		};
		RemoteTrackerViewModel = new TrackerViewModel {
			Name = "My Switch",
			TargetType = TrackerTargetType.Remote,
			TargetAddress = "192.168.0.42",
			MockInventory = Inventory,
		};
		LocalTrackerViewModel = new TrackerViewModel {
			Name = "Ryujinx",
			TargetType = TrackerTargetType.LocalEmulator,
			MockInventory = Inventory,
		};
		MainViewModel = new MainViewModel {
			TrackerTargets = { RemoteTrackerViewModel, LocalTrackerViewModel },
		};
	}

	public static DreadInventory?   Inventory              { get; }
	public static TrackerViewModel? RemoteTrackerViewModel { get; }
	public static TrackerViewModel? LocalTrackerViewModel  { get; }
	public static MainViewModel?    MainViewModel          { get; }
}