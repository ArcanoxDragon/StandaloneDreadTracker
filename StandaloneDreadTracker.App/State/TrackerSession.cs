using System.Text.Json.Serialization;
using StandaloneDreadTracker.App.Tracker;

namespace StandaloneDreadTracker.App.State;

public sealed class TrackerSession
{
	[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
	public ItemLocationHints ItemLocationHints { get; } = new();

	[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
	public BossDnaHints BossDnaHints { get; } = new();

	public TrackerSession Clone()
	{
		var newSession = new TrackerSession();

		ItemLocationHints.CopyTo(newSession.ItemLocationHints);
		BossDnaHints.CopyTo(newSession.BossDnaHints);

		return newSession;
	}
}