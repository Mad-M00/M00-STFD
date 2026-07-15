using System;
using System.Collections.Generic;
using M00STFD.Core;
using UnityEngine;

namespace M00STFD.Game;

// Turns a lockpicking noise into consequences: every zombie within the
// (sneak-scaled) radius rolls against the alert chance; the ones that
// hear it wake up and come investigate the door. Quietness combines the
// crouch baseline with the game's NoiseMultiplier passive effect, so the
// From The Shadows perk reduces lock noise exactly as it reduces footsteps.
//
// Zombie AI is simulated by the authority, so the alert must run there:
// in single-player and on a hosting client this process IS the authority
// and alerts locally; a client on a dedicated server sends the pre-scaled
// noise to the server (NetPackageStfdLockNoise), which sanitises and
// executes it.
internal sealed class DoorNoiseService
{
	private readonly IDoorLog log;
	private readonly Func<LockpickSettings> settingsAccessor;

	private LockpickSettings Settings => settingsAccessor();

	public DoorNoiseService(IDoorLog log, Func<LockpickSettings> settings)
	{
		this.log = log;
		settingsAccessor = settings;
	}

	// The loudest noise this side's config can produce; the server uses it
	// to cap whatever a client claims.
	public float MaxConfiguredRadius =>
		Math.Max(Settings.UnlockNoise.Radius, Settings.BreakNoise.Radius);

	public void EmitNoise(Vector3i doorPos, NoiseProfile noise, EntityPlayerLocal player)
	{
		NoiseProfile effective = LockNoisePlanner.Scale(noise, QuietnessOf(player));
		if (effective.IsSilent)
		{
			return;
		}
		Vector3 center = doorPos.ToVector3() + Vector3.one * 0.5f;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AlertZombies(GameManager.Instance.World, center, effective);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
			NetPackageManager.GetPackage<NetPackageStfdLockNoise>()
				.Setup(center, effective, player.entityId));
	}

	// Runs on the authority (directly, or from the net package).
	public void AlertZombies(World world, Vector3 center, NoiseProfile noise)
	{
		if (world == null || noise.IsSilent)
		{
			return;
		}
		var bounds = new Bounds(center, Vector3.one * (noise.Radius * 2f));
		List<Entity> nearby = world.GetEntitiesInBounds(typeof(EntityEnemy), bounds, new List<Entity>());
		GameRandom random = world.GetGameRandom();
		int alerted = 0;
		foreach (Entity entity in nearby)
		{
			if (!(entity is EntityAlive enemy) || enemy.IsDead())
			{
				continue;
			}
			if (!LockNoisePlanner.ShouldAlert(noise.AlertChance, random.RandomRange(0f, 1f)))
			{
				continue;
			}
			enemy.ConditionalTriggerSleeperWakeUp();
			// InvestigateSeconds is config; the game API wants ticks (20/s).
			enemy.SetInvestigatePosition(center, (int)(Settings.InvestigateSeconds * 20f), isAlert: true);
			alerted++;
		}
		if (alerted > 0)
		{
			log.Info("lock noise at " + center + " alerted " + alerted + " zombie(s)");
		}
	}

	private float QuietnessOf(EntityPlayerLocal player)
	{
		if (!player.IsCrouching)
		{
			return 1f;
		}
		// NoiseMultiplier starts at 1 and is reduced by From The Shadows
		// (its effect is itself gated on crouching), stacked on the plain
		// crouch baseline from config.
		float perkMultiplier = EffectManager.GetValue(PassiveEffects.NoiseMultiplier,
			player.inventory.holdingItemItemValue, 1f, player);
		return Settings.SneakNoiseFactor * perkMultiplier;
	}
}
