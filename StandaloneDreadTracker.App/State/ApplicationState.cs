using System.Text.Json.Serialization;
using Arcanox.AppCore.Settings;

namespace StandaloneDreadTracker.App.State;

public sealed class ApplicationState : ISettings<ApplicationState>
{
	[JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
	public Dictionary<string, TrackerSession> TrackerSessions { get; } = [];

	public TrackerSession GetOrCreateSession(string trackerId)
	{
		lock (TrackerSessions)
		{
			if (!TrackerSessions.TryGetValue(trackerId, out var session))
			{
				session = new TrackerSession();
				TrackerSessions[trackerId] = session;
			}

			return session;
		}
	}

	public void CopyTo(ApplicationState other)
	{
		other.TrackerSessions.Clear();

		foreach (var (id, session) in TrackerSessions)
			other.TrackerSessions.Add(id, session.Clone());
	}
}