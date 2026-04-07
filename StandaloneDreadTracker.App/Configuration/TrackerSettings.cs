using System.Text.Json.Serialization;
using StandaloneDreadTracker.App.Utility;

namespace StandaloneDreadTracker.App.Configuration;

public sealed class TrackerSettings : IJsonOnSerializing
{
	[JsonConverter(typeof(JsonStringEnumConverter<TrackerTargetType>))]
	public TrackerTargetType TargetType { get; set; }

	public string? Name      { get; set; }
	public string? IpAddress { get; set; }

	public TrackerSettings Clone()
		=> new() {
			TargetType = TargetType,
			Name = Name,
			IpAddress = IpAddress,
		};

	public void OnSerializing()
	{
		if (TargetType != TrackerTargetType.Remote)
			// Save some bytes if a target is changed from Remote to LocalEmulator
			IpAddress = null;
	}
}