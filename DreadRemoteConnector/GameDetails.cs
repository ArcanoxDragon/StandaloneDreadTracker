using System.Diagnostics.CodeAnalysis;

namespace DreadRemoteConnector;

public sealed class GameDetails : IParsable<GameDetails>
{
	public int    ApiVersion       { get; private set; } = -1;
	public int    BufferSize       { get; private set; } = -1;
	public Guid   WorldGuid        { get; private set; }
	public string GameVersion      { get; private set; } = "Unknown";
	public int    RequiredDnaCount { get; private set; }

	#region IParsable

	public static GameDetails Parse(string details, IFormatProvider? formatProvider = null)
	{
		var gameDetails = new GameDetails();

		gameDetails.ParseCore(details);

		return gameDetails;
	}

	public static bool TryParse([NotNullWhen(true)] string? details, [MaybeNullWhen(false)] out GameDetails result)
		=> TryParse(details, null, out result);

	public static bool TryParse([NotNullWhen(true)] string? details, IFormatProvider? provider, [MaybeNullWhen(false)] out GameDetails result)
	{
		if (string.IsNullOrEmpty(details))
		{
			result = null;
			return false;
		}

		try
		{
			result = Parse(details);
			return true;
		}
		catch
		{
			result = null;
			return false;
		}
	}

	#endregion

	private void ParseCore(string details)
	{
		var parts = details.Split(',');

		if (parts.Length != 5)
			throw new FormatException($"Could not parse game details. Expected 4 components, but got {parts.Length}.");

		try
		{
			ApiVersion = int.Parse(parts[0]);
			BufferSize = int.Parse(parts[1]);
			WorldGuid = Guid.Parse(parts[2]);
			GameVersion = parts[3];
			RequiredDnaCount = int.Parse(parts[4]);
		}
		catch (Exception ex)
		{
			throw new FormatException("Could not parse game details.", ex);
		}
	}
}