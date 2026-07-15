namespace M00STFD.Core;

// The second rule of the mod: a CLOSED locked world door can be lockpicked
// open, the way vanilla lets you pick a safe or a police car. This is the
// exact complement of DoorUnlockPolicy — a door is either standing open
// (unlock it outright) or shut (earn it with lockpicks); the same
// boundaries protect player bases, and closed doors are only ever opened
// through the pick minigame, never silently.
public static class DoorLockpickPolicy
{
	public static bool ShouldOfferPicking(DoorObservation door)
	{
		return door.HasDoorFeature
			&& door.HasLockFeature
			&& !door.IsOpen
			&& door.IsLocked
			&& !door.IsPlayerPlaced;
	}
}
