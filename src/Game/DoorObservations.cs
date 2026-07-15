using M00STFD.Core;

namespace M00STFD.Game;

// The one place a composite tile entity is read into the game-free
// snapshot both policies decide on.
internal static class DoorObservations
{
	public static DoorObservation From(
		TileEntityComposite tileEntity,
		out TEFeatureDoor door,
		out TEFeatureLockable doorLock)
	{
		door = tileEntity.GetFeature<TEFeatureDoor>();
		doorLock = tileEntity.GetFeature<TEFeatureLockable>();
		return new DoorObservation(
			hasDoorFeature: door != null,
			hasLockFeature: doorLock != null,
			isOpen: door != null && door.IsOpen(),
			isLocked: doorLock != null && doorLock.IsLocked(),
			isPlayerPlaced: tileEntity.PlayerPlaced);
	}
}
