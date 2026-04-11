namespace StandaloneDreadTracker.App.Utility;

public enum TrackerTargetType
{
	Unknown,
	Remote,
	LocalEmulator,
}

public static class TrackerTargetTypeExtensions
{
	extension(TrackerTargetType type)
	{
		public string DisplayText => type switch {
			TrackerTargetType.LocalEmulator => "Local Emulator",
			TrackerTargetType.Remote        => "Physical Console (or other PC)",

			_ => "Unknown",
		};
	}
}