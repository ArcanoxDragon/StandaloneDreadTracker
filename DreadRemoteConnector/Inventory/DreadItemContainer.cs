using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using DreadRemoteConnector.Observability;

namespace DreadRemoteConnector.Inventory;

public class DnaValueChangedEventArgs(int dnaNumber) : EventArgs
{
	public int DnaNumber { get; } = dnaNumber;
}

public abstract class DreadItemContainer<T> : NotifyPropertyChangedObject
{
	#region Beam Upgrades

	public T? WideBeam
	{
		get;
		set => SetField(ref field, value);
	}

	public T? PlasmaBeam
	{
		get;
		set => SetField(ref field, value);
	}

	public T? WaveBeam
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Charge Beam Upgrades

	public T? ChargeBeam
	{
		get;
		set => SetField(ref field, value);
	}

	public T? DiffusionBeam
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Morph Ball Upgrades

	public T? MorphBall
	{
		get;
		set => SetField(ref field, value);
	}

	public T? Bomb
	{
		get;
		set => SetField(ref field, value);
	}

	public T? CrossBomb
	{
		get;
		set => SetField(ref field, value);
	}

	public T? PowerBomb
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Missile Upgrades

	public T? SuperMissile
	{
		get;
		set => SetField(ref field, value);
	}

	public T? IceMissile
	{
		get;
		set => SetField(ref field, value);
	}

	public T? StormMissile
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Jump Upgrades

	public T? SpinBoost
	{
		get;
		set => SetField(ref field, value);
	}

	public T? SpaceJump
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Suit Upgrades

	public T? VariaSuit
	{
		get;
		set => SetField(ref field, value);
	}

	public T? GravitySuit
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Misc. Upgrades

	public T? SpiderMagnet
	{
		get;
		set => SetField(ref field, value);
	}

	public T? GrappleBeam
	{
		get;
		set => SetField(ref field, value);
	}

	public T? SpeedBooster
	{
		get;
		set => SetField(ref field, value);
	}

	public T? ScrewAttack
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Aeion Upgrades

	public T? PhantomCloak
	{
		get;
		set => SetField(ref field, value);
	}

	public T? FlashShift
	{
		get;
		set => SetField(ref field, value);
	}

	public T? PulseRadar
	{
		get;
		set => SetField(ref field, value);
	}

	#endregion

	#region Metroid DNA

	private static readonly string MetroidDnaPropertyNamePrefix = nameof(MetroidDna1)[..^1];

	private readonly T?[] metroidDna = new T?[DreadItems.MaxMetroidDnaCount];

	public event EventHandler<DnaValueChangedEventArgs>? DnaStateChanged;

	[JsonIgnore]
	public IReadOnlyList<T?> AllMetroidDna => this.metroidDna;

	public T? MetroidDna1  { get => GetDna(); set => SetDna(value); }
	public T? MetroidDna2  { get => GetDna(); set => SetDna(value); }
	public T? MetroidDna3  { get => GetDna(); set => SetDna(value); }
	public T? MetroidDna4  { get => GetDna(); set => SetDna(value); }
	public T? MetroidDna5  { get => GetDna(); set => SetDna(value); }
	public T? MetroidDna6  { get => GetDna(); set => SetDna(value); }
	public T? MetroidDna7  { get => GetDna(); set => SetDna(value); }
	public T? MetroidDna8  { get => GetDna(); set => SetDna(value); }
	public T? MetroidDna9  { get => GetDna(); set => SetDna(value); }
	public T? MetroidDna10 { get => GetDna(); set => SetDna(value); }
	public T? MetroidDna11 { get => GetDna(); set => SetDna(value); }
	public T? MetroidDna12 { get => GetDna(); set => SetDna(value); }

	protected virtual void NotifyWhenDnaChanged(int dnaNumber)
	{
		string dnaPropertyName = MetroidDnaPropertyNamePrefix + dnaNumber;

		RaisePropertyChanged(dnaPropertyName);
		DnaStateChanged?.Invoke(this, new DnaValueChangedEventArgs(dnaNumber));
	}

	protected ref T? GetDnaRef(int dnaNumber)
		=> ref this.metroidDna[dnaNumber - 1];

	private T? GetDna([CallerMemberName] string? propertyName = null)
	{
		ArgumentNullException.ThrowIfNull(propertyName);
		return GetDnaRef(propertyName, out _);
	}

	private void SetDna(T? value, [CallerMemberName] string? propertyName = null)
	{
		ArgumentNullException.ThrowIfNull(propertyName);

		ref var dnaRef = ref GetDnaRef(propertyName, out var dnaNumber);

		if (Equals(dnaRef, value))
			// Value not actually changing
			return;

		dnaRef = value;
		NotifyWhenDnaChanged(dnaNumber);
	}

	private ref T? GetDnaRef(string propertyName, out int dnaNumber)
	{
		if (!int.TryParse(propertyName[MetroidDnaPropertyNamePrefix.Length..], out dnaNumber))
			throw new ArgumentException($"Invalid DNA property name: {propertyName}", nameof(propertyName));

		return ref GetDnaRef(dnaNumber);
	}

	#endregion

	public void Clear() => Fill(default(T));

	public void Fill(T? value)
	{
		WideBeam = value;
		PlasmaBeam = value;
		WaveBeam = value;
		ChargeBeam = value;
		DiffusionBeam = value;
		MorphBall = value;
		Bomb = value;
		CrossBomb = value;
		PowerBomb = value;
		SuperMissile = value;
		IceMissile = value;
		StormMissile = value;
		SpinBoost = value;
		SpaceJump = value;
		VariaSuit = value;
		GravitySuit = value;
		SpiderMagnet = value;
		GrappleBeam = value;
		SpeedBooster = value;
		ScrewAttack = value;
		PhantomCloak = value;
		FlashShift = value;
		PulseRadar = value;
		this.metroidDna.AsSpan().Fill(value);

		for (var i = 0; i < this.metroidDna.Length; i++)
			NotifyWhenDnaChanged(i + 1);
	}

	public void Fill(Func<T?> valueFactory)
	{
		WideBeam = valueFactory();
		PlasmaBeam = valueFactory();
		WaveBeam = valueFactory();
		ChargeBeam = valueFactory();
		DiffusionBeam = valueFactory();
		MorphBall = valueFactory();
		Bomb = valueFactory();
		CrossBomb = valueFactory();
		PowerBomb = valueFactory();
		SuperMissile = valueFactory();
		IceMissile = valueFactory();
		StormMissile = valueFactory();
		SpinBoost = valueFactory();
		SpaceJump = valueFactory();
		VariaSuit = valueFactory();
		GravitySuit = valueFactory();
		SpiderMagnet = valueFactory();
		GrappleBeam = valueFactory();
		SpeedBooster = valueFactory();
		ScrewAttack = valueFactory();
		PhantomCloak = valueFactory();
		FlashShift = valueFactory();
		PulseRadar = valueFactory();

		for (var i = 0; i < this.metroidDna.Length; i++)
		{
			this.metroidDna[i] = valueFactory();
			NotifyWhenDnaChanged(i + 1);
		}
	}

	public void CopyTo(DreadItemContainer<T> other)
	{
		other.WideBeam = WideBeam;
		other.PlasmaBeam = PlasmaBeam;
		other.WaveBeam = WaveBeam;
		other.ChargeBeam = ChargeBeam;
		other.DiffusionBeam = DiffusionBeam;
		other.MorphBall = MorphBall;
		other.Bomb = Bomb;
		other.CrossBomb = CrossBomb;
		other.PowerBomb = PowerBomb;
		other.SuperMissile = SuperMissile;
		other.IceMissile = IceMissile;
		other.StormMissile = StormMissile;
		other.SpinBoost = SpinBoost;
		other.SpaceJump = SpaceJump;
		other.VariaSuit = VariaSuit;
		other.GravitySuit = GravitySuit;
		other.SpiderMagnet = SpiderMagnet;
		other.GrappleBeam = GrappleBeam;
		other.SpeedBooster = SpeedBooster;
		other.ScrewAttack = ScrewAttack;
		other.PhantomCloak = PhantomCloak;
		other.FlashShift = FlashShift;
		other.PulseRadar = PulseRadar;

		for (var i = 0; i < this.metroidDna.Length; i++)
		{
			other.metroidDna[i] = this.metroidDna[i];
			other.NotifyWhenDnaChanged(i + 1);
		}
	}
}