using System.Drawing;
using System.Text.Json.Serialization;
using StandaloneDreadTracker.App.Utility;

namespace StandaloneDreadTracker.App.Configuration;

public sealed class TrackerSettings : IJsonOnSerializing
{
	public const int DefaultItemIconSize = 56;
	public const int DefaultBossIconSize = 80;

	[JsonConverter(typeof(JsonStringEnumConverter<TrackerTargetType>))]
	public TrackerTargetType TargetType { get; set; }

	public string? Name               { get; set; }
	public string? IpAddress          { get; set; }
	public bool    ShowItemGroups     { get; set; } = true;
	public int     ItemIconSize       { get; set; } = DefaultItemIconSize;
	public int     BossIconSize       { get; set; } = DefaultBossIconSize;
	public Size    LastWindowSize     { get; set; }
	public Point   LastWindowPosition { get; set; }

	public TrackerSettings Clone()
		=> new() {
			TargetType = TargetType,
			Name = Name,
			IpAddress = IpAddress,
			ShowItemGroups = ShowItemGroups,
			ItemIconSize = ItemIconSize,
			BossIconSize = BossIconSize,
			LastWindowSize = LastWindowSize,
			LastWindowPosition = LastWindowPosition,
		};

	public void OnSerializing()
	{
		if (TargetType != TrackerTargetType.Remote)
			// Save some bytes if a target is changed from Remote to LocalEmulator
			IpAddress = null;
	}
}