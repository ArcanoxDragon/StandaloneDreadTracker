using JetBrains.Annotations;

namespace DreadRemoteConnector.Inventory;

[PublicAPI]
public class DreadBosses : DreadBossContainer<bool>
{
	public static IReadOnlyList<string> BossOrder { get; } = [
		"WhiteEmmi",
		"GreenEmmi",
		"YellowEmmi",
		"BlueEmmi",
		"PurpleEmmi",
		"OrangeEmmi",
		"Corpius",
		"Kraid",
		"Drogyga",
		"ExperimentZ57",
		"Golzuna",
		"Escue",
	];
}