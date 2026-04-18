using System.Buffers;
using System.Runtime.CompilerServices;
using DreadRemoteConnector.Observability;

namespace StandaloneDreadTracker.App.Tracker;

public class BossDnaHints : NotifyPropertyChangedObject
{
	private static readonly string[] BossOrder = [
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

	private readonly int[] dnaHints = new int[BossOrder.Length];

	public int WhiteEmmi     { get => GetDnaHint(); set => SetDnaHint(value); }
	public int GreenEmmi     { get => GetDnaHint(); set => SetDnaHint(value); }
	public int YellowEmmi    { get => GetDnaHint(); set => SetDnaHint(value); }
	public int BlueEmmi      { get => GetDnaHint(); set => SetDnaHint(value); }
	public int PurpleEmmi    { get => GetDnaHint(); set => SetDnaHint(value); }
	public int OrangeEmmi    { get => GetDnaHint(); set => SetDnaHint(value); }
	public int Corpius       { get => GetDnaHint(); set => SetDnaHint(value); }
	public int Kraid         { get => GetDnaHint(); set => SetDnaHint(value); }
	public int Drogyga       { get => GetDnaHint(); set => SetDnaHint(value); }
	public int ExperimentZ57 { get => GetDnaHint(); set => SetDnaHint(value); }
	public int Golzuna       { get => GetDnaHint(); set => SetDnaHint(value); }
	public int Escue         { get => GetDnaHint(); set => SetDnaHint(value); }

	private ref int MaxSlot => ref this.dnaHints[^1];

	public void AssignHint(string bossName, int dnaNumber)
	{
		if (!SetDnaHint(dnaNumber, bossName))
			throw new ArgumentException($"Unknown boss: {bossName}", nameof(bossName));
	}

	public void ToggleDnaHint(string bossName)
	{
		ref var slot = ref GetHintRef(bossName);

		if (Unsafe.IsNullRef(ref slot))
			return;

		if (slot > 0)
		{
			// Just clear the hint
			SetField(ref slot, 0, bossName);
			return;
		}

		var usedDnaNumbers = ArrayPool<bool>.Shared.Rent(this.dnaHints.Length);

		try
		{
			Array.Clear(usedDnaNumbers);

			// Find the first unused DNA number to assign.
			// First, we will flag all used numbers, and then find the
			// first "false" value in the array and use that index.
			for (var i = 0; i < this.dnaHints.Length; i++)
			{
				ref var thisSlot = ref this.dnaHints[i];
				var thisDnaNumber = thisSlot;

				if (thisDnaNumber <= 0 || thisDnaNumber > this.dnaHints.Length)
					// Don't mark out-of-bounds numbers
					continue;
				if (Unsafe.AreSame(ref slot, ref thisSlot))
					// Skip the slot being assigned
					continue;

				usedDnaNumbers[thisDnaNumber - 1] = true;
			}

			// Find the first "false" value, which is the index of the first unused DNA number
			for (var i = 0; i < this.dnaHints.Length; i++)
			{
				if (!usedDnaNumbers[i])
				{
					SetField(ref slot, i + 1, bossName);
					return;
				}
			}

			// If we didn't find a "false" value (which should be impossible, but just in case),
			// we will just clear the value again.
			SetField(ref slot, 0, bossName);
		}
		finally
		{
			ArrayPool<bool>.Shared.Return(usedDnaNumbers);
		}
	}

	private int GetDnaHint([CallerMemberName] string? bossName = null)
	{
		ref var slot = ref GetHintRef(bossName);

		if (Unsafe.IsNullRef(ref slot))
			return 0;

		return slot;
	}

	private bool SetDnaHint(int value, [CallerMemberName] string? bossName = null)
	{
		ref var slot = ref GetHintRef(bossName);

		if (Unsafe.IsNullRef(ref slot))
			return false;

		SetField(ref slot, value, bossName);
		return true;
	}

	private ref int GetHintRef(string? bossName)
	{
		var index = BossOrder.IndexOf(bossName);

		if (index < 0)
			return ref Unsafe.NullRef<int>();

		return ref this.dnaHints[index];
	}
}