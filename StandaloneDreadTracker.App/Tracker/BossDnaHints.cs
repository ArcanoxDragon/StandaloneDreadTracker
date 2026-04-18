using System.Buffers;
using System.Runtime.CompilerServices;
using DreadRemoteConnector.Inventory;

namespace StandaloneDreadTracker.App.Tracker;

public class BossDnaHints : DreadBossContainer<int>
{
	public void ToggleDnaHint(string bossName)
	{
		ref var slot = ref GetSlot(bossName);

		if (Unsafe.IsNullRef(ref slot))
			return;

		if (slot > 0)
		{
			// Just clear the hint
			SetField(ref slot, 0, bossName);
			return;
		}

		var usedDnaNumbers = ArrayPool<bool>.Shared.Rent(DreadBosses.BossOrder.Count);

		try
		{
			Array.Clear(usedDnaNumbers);

			// Find the first unused DNA number to assign.
			// First, we will flag all used numbers, and then find the
			// first "false" value in the array and use that index.
			for (var i = 0; i < DreadBosses.BossOrder.Count; i++)
			{
				ref var thisSlot = ref GetSlot(i);
				var thisDnaNumber = thisSlot;

				if (thisDnaNumber <= 0 || thisDnaNumber > DreadBosses.BossOrder.Count)
					// Don't mark out-of-bounds numbers
					continue;
				if (Unsafe.AreSame(ref slot, ref thisSlot))
					// Skip the slot being assigned
					continue;

				usedDnaNumbers[thisDnaNumber - 1] = true;
			}

			// Find the first "false" value, which is the index of the first unused DNA number
			for (var i = 0; i < DreadBosses.BossOrder.Count; i++)
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
}