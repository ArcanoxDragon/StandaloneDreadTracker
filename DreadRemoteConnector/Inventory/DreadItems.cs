namespace DreadRemoteConnector.Inventory;

public static class DreadItems
{
	#region Item Constants

	#region Health/Ammo

	public const string MaxMissileCapacity   = "ITEM_WEAPON_MISSILE_MAX";
	public const string MaxEnergyCapacity    = "ITEM_MAX_LIFE";
	public const string MaxPowerBombCapacity = "ITEM_WEAPON_POWER_BOMB_MAX";

	#endregion

	#region Beam Upgrades

	public const string WideBeam   = "ITEM_WEAPON_WIDE_BEAM";
	public const string PlasmaBeam = "ITEM_WEAPON_PLASMA_BEAM";
	public const string WaveBeam   = "ITEM_WEAPON_WAVE_BEAM";

	#endregion

	#region Charge Beam Upgrades

	public const string ChargeBeam    = "ITEM_WEAPON_CHARGE_BEAM";
	public const string DiffusionBeam = "ITEM_WEAPON_DIFFUSION_BEAM";

	#endregion

	#region Morph Ball Upgrades

	public const string MorphBall = "ITEM_MORPH_BALL";
	public const string Bomb      = "ITEM_WEAPON_BOMB";
	public const string CrossBomb = "ITEM_WEAPON_LINE_BOMB";
	public const string PowerBomb = "ITEM_WEAPON_POWER_BOMB";

	#endregion

	#region Missile Upgrades

	public const string SuperMissile = "ITEM_WEAPON_SUPER_MISSILE";
	public const string IceMissile   = "ITEM_WEAPON_ICE_MISSILE";
	public const string StormMissile = "ITEM_MULTILOCKON";

	#endregion

	#region Jump Upgrades

	public const string SpinBoost = "ITEM_DOUBLE_JUMP";
	public const string SpaceJump = "ITEM_SPACE_JUMP";

	#endregion

	#region Suit Upgrades

	public const string VariaSuit   = "ITEM_VARIA_SUIT";
	public const string GravitySuit = "ITEM_GRAVITY_SUIT";

	#endregion

	#region Misc. Upgrades

	public const string SpiderMagnet = "ITEM_MAGNET_GLOVE";
	public const string GrappleBeam  = "ITEM_WEAPON_GRAPPLE_BEAM";
	public const string SpeedBooster = "ITEM_SPEED_BOOSTER";
	public const string ScrewAttack  = "ITEM_SCREW_ATTACK";

	#endregion

	#region Aeion Upgrades

	public const string PhantomCloak = "ITEM_OPTIC_CAMOUFLAGE";
	public const string FlashShift   = "ITEM_GHOST_AURA";
	public const string PulseRadar   = "ITEM_SONAR";

	#endregion

	#region Randomizer Upgrades

	public const string FlashShiftUpgrade   = "ITEM_UPGRADE_FLASH_SHIFT_CHAIN";
	public const string SpeedBoosterUpgrade = "ITEM_UPGRADE_SPEED_BOOST_CHARGE";

	#endregion

	#region Metroid DNA

	public const string MetroidDnaPrefix   = "ITEM_RANDO_ARTIFACT_";
	public const int    MaxMetroidDnaCount = 12;

	public static IReadOnlyList<string> AllMetroidDnaItems { get; } = GetMetroidDnaItems(MaxMetroidDnaCount).ToList();

	public static string GetMetroidDnaItem(int number)
	{
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(number, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(number, MaxMetroidDnaCount);
		return MetroidDnaPrefix + number;
	}

	public static IEnumerable<string> GetMetroidDnaItems(int count)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(count, MaxMetroidDnaCount);

		for (var i = 1; i <= count; i++)
			yield return GetMetroidDnaItem(i);
	}

	#endregion

	#endregion

	#region Items of Interest

	public static IReadOnlyList<string> DefaultItemsOfInterest { get; } = [
		MaxMissileCapacity,
		MaxEnergyCapacity,
		MaxPowerBombCapacity,
		WideBeam,
		PlasmaBeam,
		WaveBeam,
		ChargeBeam,
		DiffusionBeam,
		MorphBall,
		Bomb,
		CrossBomb,
		PowerBomb,
		SuperMissile,
		IceMissile,
		StormMissile,
		SpinBoost,
		SpaceJump,
		VariaSuit,
		GravitySuit,
		SpiderMagnet,
		GrappleBeam,
		SpeedBooster,
		ScrewAttack,
		PhantomCloak,
		FlashShift,
		PulseRadar,
		FlashShiftUpgrade,
		SpeedBoosterUpgrade,
		..AllMetroidDnaItems,
	];

	#endregion
}