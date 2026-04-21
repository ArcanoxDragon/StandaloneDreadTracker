using System.Text.Json.Serialization;

namespace StandaloneDreadTracker.App.Configuration;

public sealed class ApplicationSettings
{
	[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
	public List<TrackerSettings> Trackers { get; } = [];

	public void CopyTo(ApplicationSettings other)
	{
		other.Trackers.Clear();

		foreach (var tracker in Trackers)
			other.Trackers.Add(tracker.Clone(keepId: true));
	}
}