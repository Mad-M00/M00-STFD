namespace M00STFD.Core;

// A game-free snapshot of everything the unlock rule needs to know about one
// block. The Game layer fills it in from the composite tile entity's feature
// modules; tests build it by hand.
public readonly struct DoorObservation
{
	public DoorObservation(
		bool hasDoorFeature,
		bool hasLockFeature,
		bool isOpen,
		bool isLocked,
		bool isPlayerPlaced)
	{
		HasDoorFeature = hasDoorFeature;
		HasLockFeature = hasLockFeature;
		IsOpen = isOpen;
		IsLocked = isLocked;
		IsPlayerPlaced = isPlayerPlaced;
	}

	// The block is an openable door (door, hatch, garage door, drawbridge).
	public bool HasDoorFeature { get; }

	// The block carries a lock module at all.
	public bool HasLockFeature { get; }

	// The door currently stands open.
	public bool IsOpen { get; }

	// The lock module currently reports locked.
	public bool IsLocked { get; }

	// A player placed (and therefore owns) this block; world-generated POI
	// blocks have no owner.
	public bool IsPlayerPlaced { get; }
}
