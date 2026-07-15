using M00STFD.Core;
using Xunit;

namespace M00STFD.Tests;

// The policy has exactly one Yes and four boundaries. The Yes is the mod's
// reason to exist; each boundary is a promise about something the mod must
// never touch. If a change flips one of these, it changes what players can
// and cannot lock — that is a behaviour decision, not a refactor, and the
// test comment says which player-visible promise is at stake.
public class DoorUnlockPolicyTests
{
	private static DoorObservation Door(
		bool hasDoorFeature = true,
		bool hasLockFeature = true,
		bool isOpen = true,
		bool isLocked = true,
		bool isPlayerPlaced = false)
	{
		return new DoorObservation(hasDoorFeature, hasLockFeature, isOpen, isLocked, isPlayerPlaced);
	}

	// The headline behaviour: a POI door swung open by a switch, key or quest
	// trigger (or spawned open) with the locked flag left behind. Unlocking it
	// is what lets the player close it — and open it again afterwards.
	[Fact]
	public void A_world_door_standing_open_but_locked_is_unlocked()
	{
		Assert.True(DoorUnlockPolicy.ShouldUnlock(Door()));
	}

	// A locked CLOSED door is the game working as intended — it is what
	// lockpicks, keys and switches are for. This boundary is also what makes
	// the mod exploit-free at traders: their doors are locked only after
	// being force-closed for the night, so they never match.
	[Fact]
	public void Closed_doors_are_never_unlocked()
	{
		Assert.False(DoorUnlockPolicy.ShouldUnlock(Door(isOpen: false)));
	}

	// Base security. A player who opens their own locked door must not hand
	// every passer-by the ability to close it, camp it, or find it unlocked
	// later. Player-placed blocks have an owner; world-generated blocks do
	// not — that is the entire distinction this flag carries.
	[Fact]
	public void Player_placed_doors_are_never_touched()
	{
		Assert.False(DoorUnlockPolicy.ShouldUnlock(Door(isPlayerPlaced: true)));
	}

	// No work when there is nothing to do: unlocking an already-unlocked door
	// would mark the tile entity modified on every chunk load and trigger
	// pointless network syncs in multiplayer.
	[Fact]
	public void Doors_already_unlocked_are_left_alone()
	{
		Assert.False(DoorUnlockPolicy.ShouldUnlock(Door(isLocked: false)));
	}

	// Lockable composites that are not doors (writable storage, for one) run
	// through the same tile-entity hooks. "Open" has no meaning for them and
	// their locks are player-managed — never theirs to lose.
	[Fact]
	public void Lockable_blocks_that_are_not_doors_are_ignored()
	{
		Assert.False(DoorUnlockPolicy.ShouldUnlock(Door(hasDoorFeature: false)));
	}

	// Plain unlockable doors carry no lock module at all; there is nothing to
	// release and nothing the adapter could call SetLocked on.
	[Fact]
	public void Doors_without_a_lock_module_are_ignored()
	{
		Assert.False(DoorUnlockPolicy.ShouldUnlock(Door(hasLockFeature: false)));
	}
}
