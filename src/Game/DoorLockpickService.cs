using System;
using System.Collections.Generic;
using Audio;
using M00STFD.Core;
using Platform;
using UnityEngine;

namespace M00STFD.Game;

// Adapter for the lockpick rule: when the player activates a closed locked
// world door, run the same hold-to-pick minigame vanilla uses for safes
// and police cars (XUiC_Timer + break chance + Lock Picking perk effects).
// Which doors qualify is DoorLockpickPolicy's decision; how hard they are
// is LockpickSettings'; whether an attempt snaps is LockpickAttemptPlanner's.
// This class only wires those decisions to the game.
internal sealed class DoorLockpickService
{
	private readonly IDoorLog log;
	private readonly Func<LockpickSettings> settingsAccessor;
	private readonly DoorNoiseService noise;

	private LockpickSettings Settings => settingsAccessor();

	private struct PickState
	{
		public float Completion;
		public int Breaks;
	}

	// Progress and snap count a broken pick leaves behind, per door.
	// Session-scoped on purpose: vanilla safes forget partial progress on
	// relog too.
	private readonly Dictionary<Vector3i, PickState> progressByDoor = new Dictionary<Vector3i, PickState>();

	public DoorLockpickService(IDoorLog log, Func<LockpickSettings> settings, DoorNoiseService noise)
	{
		this.log = log;
		settingsAccessor = settings;
		this.noise = noise;
	}

	// Returns true when this interaction was handled (timer opened, or the
	// "no lockpicks" hint shown) so the caller suppresses vanilla's locked
	// buzzer. Returns false when the door is not ours to handle.
	public bool TryBeginPick(TEFeatureDoor door, EntityPlayerLocal player)
	{
		if (door == null || player == null || !Settings.Enabled || GameManager.Instance.IsEditMode())
		{
			return false;
		}
		TileEntityComposite tileEntity = door.Parent;
		DoorObservation observation = DoorObservations.From(tileEntity, out _, out TEFeatureLockable doorLock);
		if (!DoorLockpickPolicy.ShouldOfferPicking(observation))
		{
			return false;
		}
		// An allowed user (keypad code holders on POI doors) opens the door
		// through vanilla; there is no locked interaction to take over.
		if (doorLock.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return false;
		}
		// Trader gates lock for the night; picking one would bypass the
		// trader schedule entirely. Leave the interaction to vanilla, which
		// shows the "come back at opening time" message.
		if (GameManager.Instance.World.GetTraderAreaAt(tileEntity.ToWorldPos()) != null)
		{
			return false;
		}
		LocalPlayerUI ui = player.PlayerUI;
		if (ui == null)
		{
			return false;
		}
		ItemValue lockpick = ItemClass.GetItem(Settings.LockPickItemName);
		if (lockpick == null || lockpick.IsEmpty())
		{
			log.Warning("lockpick item '" + Settings.LockPickItemName + "' does not exist - lockpicking disabled");
			return false;
		}
		Vector3i doorPos = tileEntity.ToWorldPos();
		if (ui.xui.PlayerInventory.GetItemCount(lockpick) == 0)
		{
			ui.xui.CollectedItemList.AddItemStack(new ItemStack(lockpick, 0), _bAddOnlyIfNotExisting: true);
			GameManager.ShowTooltip(player, Localization.Get("ttLockpickMissing"));
			return true;
		}

		Block block = tileEntity.TeData.Block;
		LockpickProfile profile = Settings.ResolveProfile(block.GetBlockName(), block.blockMaterial?.id);
		float fullTime = EffectManager.GetValue(PassiveEffects.LockPickTime,
			player.inventory.holdingItemItemValue, profile.Seconds, player);
		float breakChance = EffectManager.GetValue(PassiveEffects.LockPickBreakChance,
			player.inventory.holdingItemItemValue, profile.BreakChance, player);
		progressByDoor.TryGetValue(doorPos, out PickState state);
		LockpickAttemptPlan plan = LockpickAttemptPlanner.Plan(fullTime, state.Completion, breakChance,
			state.Breaks, Settings.MaxBreaksPerDoor,
			player.rand.RandomRange(0f, 1f), player.rand.RandomRange(0f, 1f));

		var timerData = new TimerEventData
		{
			Data = player,
			AlternateTime = plan.BreakAtElapsed,
		};
		timerData.FullTimeFinishEvent += _ => HandlePickSucceeded(doorLock, block, doorPos, player);
		timerData.AlternateEvent += timer => HandlePickBroken(timer, lockpick, doorPos, player);
		timerData.CloseEvent += _ => PlayAt(doorPos, "Misc/locked");

		player.AimingGun = false;
		XUiC_Timer.OpenTimer(ui.xui, fullTime, timerData, plan.TimeLeftAtStart);
		PlayAt(doorPos, "Misc/unlocking");
		return true;
	}

	public void ClearWorldState()
	{
		progressByDoor.Clear();
	}

	private void HandlePickSucceeded(TEFeatureLockable doorLock, Block block, Vector3i doorPos, EntityPlayerLocal player)
	{
		progressByDoor.Remove(doorPos);
		doorLock.SetLocked(_isLocked: false);
		PlayAt(doorPos, "Misc/unlocking");
		// The click of the lock giving way is quiet, but whatever is right
		// behind the door might hear it.
		noise.EmitNoise(doorPos, Settings.UnlockNoise, player);
		log.Info("lockpicked door '" + block.GetBlockName() + "' at " + doorPos);
	}

	private void HandlePickBroken(TimerEventData timer, ItemValue lockpick, Vector3i doorPos, EntityPlayerLocal player)
	{
		var oneLockpick = new ItemStack(lockpick, 1);
		LocalPlayerUI ui = player.PlayerUI;
		ui.xui.PlayerInventory.RemoveItem(oneLockpick);
		ui.xui.CollectedItemList.RemoveItemStack(oneLockpick);
		GameManager.ShowTooltip(player, Localization.Get("ttLockpickBroken"));
		PlayAt(doorPos, "Misc/locked");
		// A snapping pick rings out — the room and anything nearby may come
		// to see what that was.
		noise.EmitNoise(doorPos, Settings.BreakNoise, player);
		progressByDoor.TryGetValue(doorPos, out PickState state);
		state.Completion = Mathf.Max(state.Completion, timer.Completion);
		state.Breaks++;
		progressByDoor[doorPos] = state;
	}

	private static void PlayAt(Vector3i blockPos, string sound)
	{
		Manager.BroadcastPlayByLocalPlayer(blockPos.ToVector3() + Vector3.one * 0.5f, sound);
	}
}
