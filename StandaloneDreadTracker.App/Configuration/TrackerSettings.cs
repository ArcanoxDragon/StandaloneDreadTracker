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

	public string? Id                { get; set; } = Guid.NewGuid().ToString("D");
	public string? Name              { get; set; }
	public string? IpAddress         { get; set; }
	public bool    ShowItemGroups    { get; set; } = true;
	public int     ItemIconSize      { get; set; } = DefaultItemIconSize;
	public int     BossIconSize      { get; set; } = DefaultBossIconSize;
	public bool    PopOutBossSection { get; set; }

	[JsonConverter(typeof(JsonStringEnumConverter<TrackerOrientation>))]
	public TrackerOrientation BossesOrientation { get; set; } = TrackerOrientation.Horizontal;

	public Size  LastMainWindowSize     { get; set; }
	public Point LastMainWindowPosition { get; set; }
	public Point LastBossWindowPosition { get; set; }

	public TrackerSettings Clone(bool keepId = false)
	{
		var cloned = new TrackerSettings {
			TargetType = TargetType,
			Name = Name,
			IpAddress = IpAddress,
			ShowItemGroups = ShowItemGroups,
			ItemIconSize = ItemIconSize,
			BossIconSize = BossIconSize,
			BossesOrientation = BossesOrientation,
			PopOutBossSection = PopOutBossSection,
			LastMainWindowSize = LastMainWindowSize,
			LastMainWindowPosition = LastMainWindowPosition,
			LastBossWindowPosition = LastBossWindowPosition,
		};

		if (keepId)
			cloned.Id = Id;

		return cloned;
	}

	public void OnSerializing()
	{
		if (TargetType != TrackerTargetType.Remote)
			// Save some bytes if a target is changed from Remote to LocalEmulator
			IpAddress = null;
	}
}