using System;
using HarmonyLib;
using M00STFD.Game;

namespace M00STFD.Mod;

// Thin Harmony entry points; behaviour lives in the Game/Core layers behind
// StfdMod.Runtime. Together the three hooks cover every moment a
// door can be (or be found) locked open: when a trigger opens it, when it
// loads from disk or the network already open, and — as a catch-all — when
// the player interacts with it.
internal class HarmonyPatches
{
	private static ModRuntime Runtime => StfdMod.Runtime;

	// Switches, keys and quest events open doors through the trigger system,
	// which toggles the door without consulting the lock; the locked flag is
	// only cleared when the POI author ticked the trigger's Unlock checkbox,
	// and most did not. This postfix runs after every feature has processed
	// the trigger, so the door's new open state is final when we look at it.
	[HarmonyPatch(typeof(TileEntityComposite), nameof(TileEntityComposite.OnBlockTriggered))]
	private class TileEntityComposite_OnBlockTriggered
	{
		public static void Postfix(TileEntityComposite __instance)
		{
			try
			{
				Runtime.Doors.UnlockIfLockedOpen(__instance);
			}
			catch (Exception e)
			{
				LogException(e);
			}
		}
	}

	// Doors that arrive already open+locked without any trigger firing:
	// prefab-decorated POI doors and doors persisted before the mod was
	// installed. OnReadComplete fires once per tile entity after its data is
	// read from disk or the network.
	[HarmonyPatch(typeof(TileEntity), nameof(TileEntity.OnReadComplete))]
	private class TileEntity_OnReadComplete
	{
		public static void Postfix(TileEntity __instance)
		{
			try
			{
				if (__instance is TileEntityComposite composite)
				{
					Runtime.Doors.UnlockIfLockedOpen(composite);
				}
			}
			catch (Exception e)
			{
				LogException(e);
			}
		}
	}

	// The player pressed E on a door. Two of the mod's rules meet here:
	// a locked OPEN door is unlocked before vanilla's own locked check runs,
	// so the same press closes it; a locked CLOSED world door starts the
	// lockpick minigame instead of the locked buzzer (skip the original —
	// the timer UI owns the interaction now). Only the door module's
	// open/close commands are dispatched into this method.
	[HarmonyPatch(typeof(TEFeatureDoor), nameof(TEFeatureDoor.OnBlockActivated))]
	private class TEFeatureDoor_OnBlockActivated
	{
		public static bool Prefix(TEFeatureDoor __instance, EntityPlayerLocal _player, ref bool __result)
		{
			try
			{
				Runtime.Doors.UnlockIfLockedOpen(__instance.Parent);
				if (Runtime.Lockpicks.TryBeginPick(__instance, _player))
				{
					__result = true;
					return false;
				}
			}
			catch (Exception e)
			{
				LogException(e);
			}
			return true;
		}
	}

	// Drop session state (partial lockpick progress) when the player leaves
	// the world, so the static-rooted runtime never carries one world's
	// state into the next.
	[HarmonyPatch(typeof(GameManager), nameof(GameManager.SaveAndCleanupWorld))]
	private class GameManager_SaveAndCleanupWorld
	{
		public static void Postfix()
		{
			try
			{
				Runtime.HandleWorldUnloaded();
			}
			catch (Exception e)
			{
				LogException(e);
			}
		}
	}

	private static void LogException(Exception e)
	{
		Runtime.Log.Warning(e.Message);
		Runtime.Log.Warning(e.StackTrace);
	}
}
