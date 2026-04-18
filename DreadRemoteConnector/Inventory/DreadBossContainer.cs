using System.Runtime.CompilerServices;
using DreadRemoteConnector.Observability;

namespace DreadRemoteConnector.Inventory;

public class BossValueChangedEventArgs(int bossIndex, string bossName) : EventArgs
{
	public int    BossIndex { get; } = bossIndex;
	public string BossName  { get; } = bossName;
}

public abstract class DreadBossContainer<T> : NotifyPropertyChangedObject
{
	private readonly T[] storage = new T[DreadBosses.BossOrder.Count];

	public event EventHandler<BossValueChangedEventArgs>? ValueChanged;

	#region EMMIs

	public T WhiteEmmi  { get => GetValue(); set => SetValue(value); }
	public T GreenEmmi  { get => GetValue(); set => SetValue(value); }
	public T YellowEmmi { get => GetValue(); set => SetValue(value); }
	public T BlueEmmi   { get => GetValue(); set => SetValue(value); }
	public T PurpleEmmi { get => GetValue(); set => SetValue(value); }
	public T OrangeEmmi { get => GetValue(); set => SetValue(value); }

	#endregion

	#region Other Bosses

	public T Corpius       { get => GetValue(); set => SetValue(value); }
	public T Kraid         { get => GetValue(); set => SetValue(value); }
	public T Drogyga       { get => GetValue(); set => SetValue(value); }
	public T ExperimentZ57 { get => GetValue(); set => SetValue(value); }
	public T Golzuna       { get => GetValue(); set => SetValue(value); }
	public T Escue         { get => GetValue(); set => SetValue(value); }

	#endregion

	public T this[int bossIndex]
	{
		get => GetSlot(bossIndex);
		set
		{
			ref var slot = ref GetSlot(bossIndex);
			var bossName = DreadBosses.BossOrder[bossIndex];

			SetValueCore(ref slot, value, bossName);
		}
	}

	public T this[string bossName]
	{
		get => GetSlot(bossName);
		set
		{
			ref var slot = ref GetSlot(bossName);

			SetValueCore(ref slot, value, bossName);
		}
	}

	protected ref T GetSlot(int bossIndex)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(bossIndex, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bossIndex, this.storage.Length);
		return ref this.storage[bossIndex];
	}

	protected ref T GetSlot(string bossName)
	{
		ref var slot = ref GetSlotUnsafe(bossName);

		if (Unsafe.IsNullRef(ref slot))
			throw new ArgumentException($"Unknown boss: {bossName}", nameof(bossName));

		return ref slot;
	}

	protected ref T GetSlotUnsafe(string? bossName)
	{
		for (var i = 0; i < DreadBosses.BossOrder.Count; i++)
		{
			if (DreadBosses.BossOrder[i] == bossName)
				return ref this.storage[i];
		}

		return ref Unsafe.NullRef<T>();
	}

	protected T GetValue([CallerMemberName] string? bossName = null)
	{
		ref var slot = ref GetSlotUnsafe(bossName);

		if (Unsafe.IsNullRef(ref slot))
			throw new ArgumentException($"Unknown boss: {bossName}", nameof(bossName));

		return slot;
	}

	protected bool SetValue(T value, [CallerMemberName] string? bossName = null)
	{
		ref var slot = ref GetSlotUnsafe(bossName);

		if (Unsafe.IsNullRef(ref slot))
			throw new ArgumentException($"Unknown boss: {bossName}", nameof(bossName));

		return SetValueCore(ref slot, value, bossName!);
	}

	protected bool SetValueCore(ref T slot, T value, string bossName)
	{
		if (!SetField(ref slot, value, bossName))
			return false;

		if (ValueChanged is not { } valueChanged)
			return true;

		ref var firstSlot = ref this.storage[0];
		var byteDifference = Unsafe.ByteOffset(ref firstSlot, ref slot);
		var bossIndex = (int) (byteDifference / Unsafe.SizeOf<T>());
		var eventArgs = new BossValueChangedEventArgs(bossIndex, bossName);

		valueChanged(this, eventArgs);
		return true;
	}
}