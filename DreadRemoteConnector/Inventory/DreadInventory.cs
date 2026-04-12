using DreadRemoteConnector.Observability;
using JetBrains.Annotations;

namespace DreadRemoteConnector.Inventory;

[PublicAPI]
public partial class DreadInventory : NotifyPropertyChangedObject
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

	#region Beam Upgrades

	public bool WideBeam
	{
		get;
		set => SetField(ref field, value);
	}

	public bool PlasmaBeam
	{
		get;
		set => SetField(ref field, value);
	}

	public bool WaveBeam
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Charge Beam Upgrades

	public bool ChargeBeam
	{
		get;
		set => SetField(ref field, value);
	}

	public bool DiffusionBeam
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Morph Ball Upgrades

	public bool MorphBall
	{
		get;
		set => SetField(ref field, value);
	}

	public bool Bomb
	{
		get;
		set => SetField(ref field, value);
	}

	public bool CrossBomb
	{
		get;
		set => SetField(ref field, value);
	}

	public bool PowerBomb
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Missile Upgrades

	public bool SuperMissile
	{
		get;
		set => SetField(ref field, value);
	}

	public bool IceMissile
	{
		get;
		set => SetField(ref field, value);
	}

	public bool StormMissile
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Jump Upgrades

	public bool SpinBoost
	{
		get;
		set => SetField(ref field, value);
	}

	public bool SpaceJump
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Suit Upgrades

	public bool VariaSuit
	{
		get;
		set => SetField(ref field, value);
	}

	public bool GravitySuit
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Misc. Upgrades

	public bool SpiderMagnet
	{
		get;
		set => SetField(ref field, value);
	}

	public bool GrappleBeam
	{
		get;
		set => SetField(ref field, value);
	}

	public bool SpeedBooster
	{
		get;
		set => SetField(ref field, value);
	}

	public bool ScrewAttack
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Aeion Upgrades

	public bool PhantomCloak
	{
		get;
		set => SetField(ref field, value);
	}

	public bool FlashShift
	{
		get;
		set => SetField(ref field, value);
	}

	public bool PulseRadar
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

	private readonly bool[] metroidDna = new bool[Items.MaxMetroidDnaCount];

	public IReadOnlyList<bool> AllMetroidDna => this.metroidDna;

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

	public bool MetroidDna1
	{
		get => this.metroidDna[0];
		set
		{
			if (SetField(ref this.metroidDna[0], value))
				RaisePropertyChanged(nameof(AllMetroidDnaCollected));
		}
	}

	public bool MetroidDna2
	{
		get => this.metroidDna[1];
		set
		{
			if (SetField(ref this.metroidDna[1], value))
				NotifyWhenAnyDnaChanged();
		}
	}

	public bool MetroidDna3
	{
		get => this.metroidDna[2];
		set
		{
			if (SetField(ref this.metroidDna[2], value))
				NotifyWhenAnyDnaChanged();
		}
	}

	public bool MetroidDna4
	{
		get => this.metroidDna[3];
		set
		{
			if (SetField(ref this.metroidDna[3], value))
				NotifyWhenAnyDnaChanged();
		}
	}

	public bool MetroidDna5
	{
		get => this.metroidDna[4];
		set
		{
			if (SetField(ref this.metroidDna[4], value))
				NotifyWhenAnyDnaChanged();
		}
	}

	public bool MetroidDna6
	{
		get => this.metroidDna[5];
		set
		{
			if (SetField(ref this.metroidDna[5], value))
				NotifyWhenAnyDnaChanged();
		}
	}

	public bool MetroidDna7
	{
		get => this.metroidDna[6];
		set
		{
			if (SetField(ref this.metroidDna[6], value))
				NotifyWhenAnyDnaChanged();
		}
	}

	public bool MetroidDna8
	{
		get => this.metroidDna[7];
		set
		{
			if (SetField(ref this.metroidDna[7], value))
				NotifyWhenAnyDnaChanged();
		}
	}

	public bool MetroidDna9
	{
		get => this.metroidDna[8];
		set
		{
			if (SetField(ref this.metroidDna[8], value))
				NotifyWhenAnyDnaChanged();
		}
	}

	public bool MetroidDna10
	{
		get => this.metroidDna[9];
		set
		{
			if (SetField(ref this.metroidDna[9], value))
				NotifyWhenAnyDnaChanged();
		}
	}

	public bool MetroidDna11
	{
		get => this.metroidDna[10];
		set
		{
			if (SetField(ref this.metroidDna[10], value))
				NotifyWhenAnyDnaChanged();
		}
	}

	public bool MetroidDna12
	{
		get => this.metroidDna[11];
		set
		{
			if (SetField(ref this.metroidDna[11], value))
				NotifyWhenAnyDnaChanged();
		}
	}

	private void NotifyWhenAnyDnaChanged()
	{
		RaisePropertyChanged(nameof(CollectedDnaCount));
		RaisePropertyChanged(nameof(AllMetroidDnaCollected));
	}

	#endregion

	internal void UpdateItemQuantity(string itemName, int quantity)
	{
		switch (itemName)
		{
			#region Health/Ammo

			case Items.MaxMissileCapacity:
				MaxMissileCapacity = quantity;
				return;
			case Items.MaxEnergyCapacity:
				MaxEnergyCapacity = quantity;
				return;
			case Items.MaxPowerBombCapacity:
				MaxPowerBombCapacity = quantity;
				return;

			#endregion

			#region Beam Upgrades

			case Items.WideBeam:
				WideBeam = quantity > 0;
				return;
			case Items.PlasmaBeam:
				PlasmaBeam = quantity > 0;
				return;
			case Items.WaveBeam:
				WaveBeam = quantity > 0;
				return;

			#endregion

			#region Charge Beam Upgrades

			case Items.ChargeBeam:
				ChargeBeam = quantity > 0;
				return;
			case Items.DiffusionBeam:
				DiffusionBeam = quantity > 0;
				return;

			#endregion

			#region Morph Ball Upgrades

			case Items.MorphBall:
				MorphBall = quantity > 0;
				return;
			case Items.Bomb:
				Bomb = quantity > 0;
				return;
			case Items.CrossBomb:
				CrossBomb = quantity > 0;
				return;
			case Items.PowerBomb:
				PowerBomb = quantity > 0;
				return;

			#endregion

			#region Missile Upgrades

			case Items.SuperMissile:
				SuperMissile = quantity > 0;
				return;
			case Items.IceMissile:
				IceMissile = quantity > 0;
				return;
			case Items.StormMissile:
				StormMissile = quantity > 0;
				return;

			#endregion

			#region Jump Upgrades

			case Items.SpinBoost:
				SpinBoost = quantity > 0;
				return;
			case Items.SpaceJump:
				SpaceJump = quantity > 0;
				return;

			#endregion

			#region Suit Upgrades

			case Items.VariaSuit:
				VariaSuit = quantity > 0;
				return;
			case Items.GravitySuit:
				GravitySuit = quantity > 0;
				return;

			#endregion

			#region Misc. Upgrades

			case Items.SpiderMagnet:
				SpiderMagnet = quantity > 0;
				return;
			case Items.GrappleBeam:
				GrappleBeam = quantity > 0;
				return;
			case Items.SpeedBooster:
				SpeedBooster = quantity > 0;
				return;
			case Items.ScrewAttack:
				ScrewAttack = quantity > 0;
				return;

			#endregion

			#region Aeion Upgrades

			case Items.PhantomCloak:
				PhantomCloak = quantity > 0;
				return;
			case Items.FlashShift:
				FlashShift = quantity > 0;
				return;
			case Items.PulseRadar:
				PulseRadar = quantity > 0;
				return;

			#endregion

			#region Randomizer Upgrades

			case Items.FlashShiftUpgrade:
				FlashShiftUpgrades = quantity;
				return;
			case Items.SpeedBoosterUpgrade:
				SpeedBoosterUpgrades = quantity;
				return;

			#endregion
		}

		#region Metroid DNA

		if (itemName.StartsWith(Items.MetroidDnaPrefix))
		{
			if (!int.TryParse(itemName[Items.MetroidDnaPrefix.Length..], out var dnaNumber))
				return;

			if (dnaNumber is < 1 or > Items.MaxMetroidDnaCount)
				return;

			ref var dnaField = ref this.metroidDna[dnaNumber - 1];
			var hasDna = quantity > 0;

			if (dnaField != hasDna)
			{
				dnaField = hasDna;

				string propertyNamePrefix = nameof(MetroidDna1)[..^1];

				RaisePropertyChanged($"{propertyNamePrefix}{dnaNumber}");
				NotifyWhenAnyDnaChanged();
			}
		}

		#endregion
	}
}