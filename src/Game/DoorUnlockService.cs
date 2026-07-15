using System;
using M00STFD.Core;

namespace M00STFD.Game;

// Adapter for the open-door rule: reads one composite tile entity into a
// DoorObservation, asks DoorUnlockPolicy for the verdict, and applies it.
// No decision is made here — if a condition ever needs to change, it
// changes in the policy where a test can pin it.
internal sealed class DoorUnlockService
{
	private readonly IDoorLog log;
	private readonly Func<StfdSettings> settings;

	public DoorUnlockService(IDoorLog log, Func<StfdSettings> settings)
	{
		this.log = log;
		this.settings = settings;
	}

	public void UnlockIfLockedOpen(TileEntityComposite tileEntity)
	{
		if (tileEntity == null || !settings().CloseOpenDoorsEnabled)
		{
			return;
		}
		DoorObservation observation = DoorObservations.From(tileEntity, out _, out TEFeatureLockable doorLock);
		if (!DoorUnlockPolicy.ShouldUnlock(observation))
		{
			return;
		}
		// SetLocked marks the tile entity modified, which also syncs the new
		// state to connected clients in multiplayer.
		doorLock.SetLocked(_isLocked: false);
		log.Info("unlocked open door '" + tileEntity.TeData.Block.GetBlockName() +
			"' at " + tileEntity.ToWorldPos());
	}
}
