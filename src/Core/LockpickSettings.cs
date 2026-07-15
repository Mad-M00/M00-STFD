using System;
using System.Collections.Generic;

namespace M00STFD.Core;

// One rule of the difficulty ladder: doors whose block name (or material
// id) matches the pattern belong to the named tier.
public sealed class LockpickRule
{
	public LockpickRule(WildcardPattern pattern, string tier)
	{
		Pattern = pattern;
		Tier = tier;
	}

	public WildcardPattern Pattern { get; }

	public string Tier { get; }
}

// Everything the lockpicking feature needs to know, resolved from
// STFDConfig.xml (or the built-in defaults when the file is
// missing or broken). Difficulty resolution order: first matching door
// name rule wins; otherwise the first matching material rule; otherwise
// the default tier. Rules higher in the config file beat rules below
// them, same promise as the other M00 mods.
public sealed class LockpickSettings
{
	private readonly Dictionary<string, LockpickProfile> tiers;
	private readonly List<LockpickRule> doorRules;
	private readonly List<LockpickRule> materialRules;

	public LockpickSettings(
		bool enabled,
		string lockPickItemName,
		string defaultTier,
		Dictionary<string, LockpickProfile> tiers,
		List<LockpickRule> doorRules,
		List<LockpickRule> materialRules,
		NoiseProfile? unlockNoise = null,
		NoiseProfile? breakNoise = null,
		float sneakNoiseFactor = DefaultSneakNoiseFactor,
		int maxBreaksPerDoor = DefaultMaxBreaksPerDoor,
		float investigateSeconds = DefaultInvestigateSeconds)
	{
		if (tiers == null || tiers.Count == 0)
		{
			throw new ArgumentException("at least one tier is required", nameof(tiers));
		}
		if (!tiers.ContainsKey(defaultTier))
		{
			throw new ArgumentException("default tier '" + defaultTier + "' is not defined", nameof(defaultTier));
		}
		Enabled = enabled;
		LockPickItemName = lockPickItemName;
		DefaultTier = defaultTier;
		UnlockNoise = unlockNoise ?? DefaultUnlockNoise;
		BreakNoise = breakNoise ?? DefaultBreakNoise;
		SneakNoiseFactor = sneakNoiseFactor;
		MaxBreaksPerDoor = maxBreaksPerDoor;
		InvestigateSeconds = investigateSeconds;
		this.tiers = tiers;
		this.doorRules = doorRules ?? new List<LockpickRule>();
		this.materialRules = materialRules ?? new List<LockpickRule>();
	}

	// The unlock click is quiet — it can wake what is directly behind the
	// door. A snapping pick rings out — it can pull the room and nearby.
	public static readonly NoiseProfile DefaultUnlockNoise = new NoiseProfile(radius: 3f, alertChance: 0.5f);
	public static readonly NoiseProfile DefaultBreakNoise = new NoiseProfile(radius: 8f, alertChance: 0.8f);
	public const float DefaultSneakNoiseFactor = 0.6f;

	// The mercy cap: after this many snaps on one door the next attempt is
	// guaranteed. Vanilla V3 does the same for safes (capped at 14).
	public const int DefaultMaxBreaksPerDoor = 4;

	// How long alerted zombies investigate the door, in seconds.
	public const float DefaultInvestigateSeconds = 20f;

	public bool Enabled { get; }

	public string LockPickItemName { get; }

	public string DefaultTier { get; }

	public NoiseProfile UnlockNoise { get; }

	public NoiseProfile BreakNoise { get; }

	// Base quietness while crouched, before the From The Shadows perk
	// shrinks it further via the game's NoiseMultiplier passive effect.
	public float SneakNoiseFactor { get; }

	public int MaxBreaksPerDoor { get; }

	public float InvestigateSeconds { get; }

	public bool HasTier(string tier)
	{
		return tier != null && tiers.ContainsKey(tier);
	}

	// For display (console status); resolution goes through ResolveProfile.
	public IEnumerable<KeyValuePair<string, LockpickProfile>> Tiers => tiers;

	public string ResolveTier(string blockName, string materialId)
	{
		foreach (LockpickRule rule in doorRules)
		{
			if (rule.Pattern.IsMatch(blockName))
			{
				return rule.Tier;
			}
		}
		foreach (LockpickRule rule in materialRules)
		{
			if (rule.Pattern.IsMatch(materialId))
			{
				return rule.Tier;
			}
		}
		return DefaultTier;
	}

	public LockpickProfile ResolveProfile(string blockName, string materialId)
	{
		return tiers[ResolveTier(blockName, materialId)];
	}

	// The shipped baseline: four tiers spanning "old wooden house door" to
	// "bank vault", calibrated against vanilla safes (gun safe 15s,
	// municipal vault 20s, break chance 0.75 before perks).
	public static LockpickSettings Default()
	{
		var tiers = new Dictionary<string, LockpickProfile>
		{
			["wood"] = new LockpickProfile(10f, 0.75f),
			["iron"] = new LockpickProfile(15f, 0.75f),
			["steel"] = new LockpickProfile(20f, 0.75f),
			["vault"] = new LockpickProfile(30f, 0.75f),
		};
		var doorRules = new List<LockpickRule>
		{
			new LockpickRule(new WildcardPattern("vault*"), "vault"),
			new LockpickRule(new WildcardPattern("jailDoor*"), "steel"),
			new LockpickRule(new WildcardPattern("steelGarageDoor*"), "steel"),
			new LockpickRule(new WildcardPattern("shuttersSteel*"), "steel"),
			new LockpickRule(new WildcardPattern("cellarDoorDoubleSteel"), "steel"),
			new LockpickRule(new WildcardPattern("commercialBulletproof*"), "steel"),
			new LockpickRule(new WildcardPattern("elevator*"), "steel"),
			new LockpickRule(new WildcardPattern("iron*"), "iron"),
			new LockpickRule(new WildcardPattern("shuttersIron*"), "iron"),
			new LockpickRule(new WildcardPattern("cellarDoorDoubleIron"), "iron"),
			new LockpickRule(new WildcardPattern("manholeHatch"), "iron"),
			new LockpickRule(new WildcardPattern("rollUp*"), "iron"),
			new LockpickRule(new WildcardPattern("chainlink*"), "iron"),
			new LockpickRule(new WildcardPattern("metalReinforcedWoodDrawBridge*"), "iron"),
		};
		var materialRules = new List<LockpickRule>
		{
			new LockpickRule(new WildcardPattern("Mwood*"), "wood"),
			new LockpickRule(new WildcardPattern("Mglass*"), "wood"),
			new LockpickRule(new WildcardPattern("Miron*"), "iron"),
			new LockpickRule(new WildcardPattern("Mmetal*"), "iron"),
			new LockpickRule(new WildcardPattern("Msteel*"), "steel"),
		};
		return new LockpickSettings(
			enabled: true,
			lockPickItemName: "resourceLockPick",
			defaultTier: "iron",
			tiers,
			doorRules,
			materialRules);
	}
}
