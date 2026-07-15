namespace M00STFD.Core;

// The whole mod is this one rule. Vanilla leaves a door's locked flag behind
// when a switch, key or quest trigger swings it open (the trigger system
// toggles doors without consulting the lock), so the door stands wide open,
// shows a Close prompt, and answers it with the locked buzzer. A door that is
// already open has let everyone through — its lock protects nothing — so an
// open, locked, world-owned door is unlocked. Every other combination is left
// exactly as vanilla had it.
public static class DoorUnlockPolicy
{
	public static bool ShouldUnlock(DoorObservation door)
	{
		return door.HasDoorFeature
			&& door.HasLockFeature
			&& door.IsOpen
			&& door.IsLocked
			&& !door.IsPlayerPlaced;
	}
}
