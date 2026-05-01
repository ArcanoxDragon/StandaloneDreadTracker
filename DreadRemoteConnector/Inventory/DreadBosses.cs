using DreadRemoteConnector.Extensions;
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

	internal void UpdateBossState(string resourceId, int quantity)
	{
		const string BossResourceIdPrefix = "BOSS_DEAD_";

		if (!resourceId.StartsWith(BossResourceIdPrefix, StringComparison.OrdinalIgnoreCase))
			return;

		// Turn resource ID such as "BOSS_DEAD_EXPERIMENT_Z57" into "EXPERIMENT_Z57" and then "EXPERIMENTZ57"
		var bossId = resourceId[BossResourceIdPrefix.Length..];
		var bossName = bossId.Replace("_", "");
		var bossIndex = BossOrder.IndexOf(bossName, StringComparer.OrdinalIgnoreCase);

		if (bossIndex < 0)
			return;

		this[bossIndex] = quantity > 0;
	}
}