using M00STFD.Core;
using Xunit;

namespace M00STFD.Tests;

// Lockpicking is the complement of the unlock rule: open locked doors are
// unlocked outright (DoorUnlockPolicy), closed locked doors must be earned
// with lockpicks. These tests pin that a door is offered to exactly one of
// the two paths, and that the base-security boundary holds for both.
public class DoorLockpickPolicyTests
{
	private static DoorObservation Door(
		bool hasDoorFeature = true,
		bool hasLockFeature = true,
		bool isOpen = false,
		bool isLocked = true,
		bool isPlayerPlaced = false)
	{
		return new DoorObservation(hasDoorFeature, hasLockFeature, isOpen, isLocked, isPlayerPlaced);
	}

	[Fact]
	public void A_closed_locked_world_door_can_be_picked()
	{
		Assert.True(DoorLockpickPolicy.ShouldOfferPicking(Door()));
	}

	// An open locked door belongs to the unlock rule, not the pick minigame
	// — offering the timer there would demand lockpicks for a door the mod
	// already promises to open for free.
	[Fact]
	public void Open_doors_are_never_picked_the_unlock_rule_owns_them()
	{
		var door = Door(isOpen: true);
		Assert.False(DoorLockpickPolicy.ShouldOfferPicking(door));
		Assert.True(DoorUnlockPolicy.ShouldUnlock(door));
	}

	// Base security, again: a stranger must not be able to pick a player's
	// locked base door. Same flag, same promise as the unlock rule.
	[Fact]
	public void Player_placed_doors_are_never_picked()
	{
		Assert.False(DoorLockpickPolicy.ShouldOfferPicking(Door(isPlayerPlaced: true)));
	}

	[Fact]
	public void Unlocked_doors_are_not_picked_vanilla_opens_them()
	{
		Assert.False(DoorLockpickPolicy.ShouldOfferPicking(Door(isLocked: false)));
	}

	[Fact]
	public void Blocks_without_door_or_lock_modules_are_ignored()
	{
		Assert.False(DoorLockpickPolicy.ShouldOfferPicking(Door(hasDoorFeature: false)));
		Assert.False(DoorLockpickPolicy.ShouldOfferPicking(Door(hasLockFeature: false)));
	}
}
