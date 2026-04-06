using System.Text;
using System.Text.Json;
using DreadRemoteConnector.Extensions;
using DreadRemoteConnector.Serialization;
using JetBrains.Annotations;

namespace DreadRemoteConnector.Packets.Receiving;

[PublicAPI]
public sealed class GameStateReceivePacket : LengthPrefixedReceivePacket, IPublicReceivePacket
{
	private const string GameStateMenu    = "MAINMENU";
	private const string GameStateLoading = "LOADING";
	private const string GameStateInGame  = "INGAME";

	public GameState GameState    { get; private set; }
	public string    ScenarioName { get; private set; } = "Not Connected";

	protected override void ReadData(BinaryReader reader, int dataLength, Encoding encoding)
	{
		var json = reader.ReadStringFast(dataLength, encoding);
		var update = JsonSerializer.Deserialize(json, DreadConnectorJsonSerializerContext.Default.GameStateUpdate);

		if (update is null)
			return;

		GameState = update.Mode switch {
			GameStateMenu    => GameState.TitleScreen,
			GameStateLoading => GameState.Loading,
			GameStateInGame  => GameState.InGame,
			_                => GameState.Unknown,
		};
		ScenarioName = GetScenarioName(update.Scenario);
	}

	private static string GetScenarioName(string? scenarioId)
		=> scenarioId switch {
			"none"            => "None",
			"s010_cave"       => "Artaria",
			"s020_magma"      => "Cataris",
			"s030_baselab"    => "Dairon",
			"s040_aqua"       => "Burenia",
			"s050_forest"     => "Ghavoran",
			"s060_quarantine" => "Elun",
			"s070_basesanc"   => "Ferenia",
			"s080_shipyard"   => "Hanubia",
			"s090_skybase"    => "Itorash",

			_ => "Unknown",
		};

	internal sealed record GameStateUpdate(string? Mode, string? Scenario);
}