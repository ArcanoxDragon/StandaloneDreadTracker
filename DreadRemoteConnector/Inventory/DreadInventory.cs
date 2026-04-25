using JetBrains.Annotations;

namespace DreadRemoteConnector.Inventory;

[PublicAPI]
public class DreadInventory : DreadItemContainer<bool>
{
	#region Health/Ammo

	public int MaxMissileCapacity
	{
		get;
		set => SetField(ref field, value);
	} = 15;

	public int MaxEnergyCapacity
	{
		get;
		set => SetField(ref field, value);
	} = 99;

	public int MaxPowerBombCapacity
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Randomizer Upgrades

	public int FlashShiftUpgrades
	{
		get;
		set
		{
			if (SetField(ref field, value))
				RaisePropertyChanged(nameof(FlashShiftChainCount));
		}
	} = 2;

	public int SpeedBoosterUpgrades
	{
		get;
		set
		{
			if (SetField(ref field, value))
				RaisePropertyChanged(nameof(SpeedBoosterChargeTime));
		}
	}

	public int FlashShiftChainCount => 1 + FlashShiftUpgrades;

	public double SpeedBoosterChargeTime
	{
		get
		{
			const double DefaultChargeTime = 1.5;
			const double MinChargeTime = 0.5;
			const double UpgradeIncrement = 0.25;

			return Math.Max(MinChargeTime, DefaultChargeTime - ( SpeedBoosterUpgrades * UpgradeIncrement ));
		}
	}

	#endregion

	#region Metroid DNA

	public bool AllMetroidDnaCollected => AllMetroidDna.All(b => b);

	/// <summary>
	/// Gets the total number of DNA that has been collected out of the total number required.
	/// If the number of required DNA is less than 12, this only considers the "earlier" DNA,
	/// and ignores any of the DNA items beyond the number required (since the extras will always
	/// be in the collected state).
	/// </summary>
	public int CollectedDnaCount => AllMetroidDna.Take(RequiredDnaCount).Count(b => b);

	public int RequiredDnaCount
	{
		get;
		set
		{
			SetField(ref field, value);
			RaisePropertyChanged(nameof(CollectedDnaCount));
		}
	}

	protected override void NotifyWhenDnaChanged(int dnaNumber)
	{
		base.NotifyWhenDnaChanged(dnaNumber);

		RaisePropertyChanged(nameof(CollectedDnaCount));
		RaisePropertyChanged(nameof(AllMetroidDnaCollected));
	}

	#endregion

	internal void UpdateItemQuantity(string itemName, int quantity)
	{
		switch (itemName)
		{
			#region Health/Ammo

			case DreadItems.MaxMissileCapacity:
				MaxMissileCapacity = quantity;
				return;
			case DreadItems.MaxEnergyCapacity:
				MaxEnergyCapacity = quantity;
				return;
			case DreadItems.MaxPowerBombCapacity:
				MaxPowerBombCapacity = quantity;
				return;

			#endregion

			#region Beam Upgrades

			case DreadItems.WideBeam:
				WideBeam = quantity > 0;
				return;
			case DreadItems.PlasmaBeam:
				PlasmaBeam = quantity > 0;
				return;
			case DreadItems.WaveBeam:
				WaveBeam = quantity > 0;
				return;

			#endregion

			#region Charge Beam Upgrades

			case DreadItems.ChargeBeam:
				ChargeBeam = quantity > 0;
				return;
			case DreadItems.DiffusionBeam:
				DiffusionBeam = quantity > 0;
				return;

			#endregion

			#region Morph Ball Upgrades

			case DreadItems.MorphBall:
				MorphBall = quantity > 0;
				return;
			case DreadItems.Bomb:
				Bomb = quantity > 0;
				return;
			case DreadItems.CrossBomb:
				CrossBomb = quantity > 0;
				return;
			case DreadItems.PowerBomb:
				PowerBomb = quantity > 0;
				return;

			#endregion

			#region Missile Upgrades

			case DreadItems.SuperMissile:
				SuperMissile = quantity > 0;
				return;
			case DreadItems.IceMissile:
				IceMissile = quantity > 0;
				return;
			case DreadItems.StormMissile:
				StormMissile = quantity > 0;
				return;

			#endregion

			#region Jump Upgrades

			case DreadItems.SpinBoost:
				SpinBoost = quantity > 0;
				return;
			case DreadItems.SpaceJump:
				SpaceJump = quantity > 0;
				return;

			#endregion

			#region Suit Upgrades

			case DreadItems.VariaSuit:
				VariaSuit = quantity > 0;
				return;
			case DreadItems.GravitySuit:
				GravitySuit = quantity > 0;
				return;

			#endregion

			#region Misc. Upgrades

			case DreadItems.SpiderMagnet:
				SpiderMagnet = quantity > 0;
				return;
			case DreadItems.GrappleBeam:
				GrappleBeam = quantity > 0;
				return;
			case DreadItems.SpeedBooster:
				SpeedBooster = quantity > 0;
				return;
			case DreadItems.ScrewAttack:
				ScrewAttack = quantity > 0;
				return;

			#endregion

			#region Aeion Upgrades

			case DreadItems.PhantomCloak:
				PhantomCloak = quantity > 0;
				return;
			case DreadItems.FlashShift:
				FlashShift = quantity > 0;
				return;
			case DreadItems.PulseRadar:
				PulseRadar = quantity > 0;
				return;

			#endregion

			#region Randomizer Upgrades

			case DreadItems.FlashShiftUpgrade:
				FlashShiftUpgrades = quantity;
				return;
			case DreadItems.SpeedBoosterUpgrade:
				SpeedBoosterUpgrades = quantity;
				return;

			#endregion
		}

		#region Metroid DNA

		if (itemName.StartsWith(DreadItems.MetroidDnaPrefix))
		{
			if (!int.TryParse(itemName[DreadItems.MetroidDnaPrefix.Length..], out var dnaNumber))
				return;

			if (dnaNumber is < 1 or > DreadItems.MaxMetroidDnaCount)
				return;

			ref var dnaSlot = ref GetDnaRef(dnaNumber);
			var prevDnaCollected = dnaSlot;
			var newDnaCollected = quantity > 0;

			if (prevDnaCollected != newDnaCollected)
			{
				dnaSlot = newDnaCollected;
				NotifyWhenDnaChanged(dnaNumber);
			}
		}

		#endregion
	}
}