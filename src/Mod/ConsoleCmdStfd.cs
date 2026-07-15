using System.Collections.Generic;
using M00STFD.Core;
using M00STFD.Game;

namespace M00STFD.Mod;

// The "stfd" console entry point: stfd | stfd reload | stfd tier <blockName>.
// Exists so users can tune STFDConfig.xml and see the result
// without restarting the game: edit, "stfd reload", "stfd tier vaultDoor01".
public class ConsoleCmdStfd : ConsoleCmdAbstract
{
	private static ModRuntime Runtime => StfdMod.Runtime;

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SdtdConsole console = SingletonMonoBehaviour<SdtdConsole>.Instance;
		if (Runtime == null)
		{
			console.Output("[STFD] mod not initialised.");
			return;
		}
		string subcommand = _params.Count == 0 ? "status" : _params[0].ToLowerInvariant();
		switch (subcommand)
		{
			case "status":
				PrintStatus(console);
				break;
			case "reload":
				Runtime.ReloadConfiguration();
				console.Output("[STFD] configuration reloaded.");
				PrintStatus(console);
				break;
			case "tier":
				PrintTier(console, _params);
				break;
			default:
				console.Output("[STFD] Unknown subcommand '" + subcommand + "'.");
				console.Output("Usage: stfd | stfd reload | stfd tier <blockName>");
				break;
		}
	}

	private static void PrintStatus(SdtdConsole console)
	{
		StfdSettings config = Runtime.Config;
		LockpickSettings picks = config.Lockpicking;
		console.Output("[STFD] close open doors: " + (config.CloseOpenDoorsEnabled ? "on" : "off") +
			" | lockpicking: " + (picks.Enabled ? "on" : "off") +
			" (item " + picks.LockPickItemName + ", mercy cap " + picks.MaxBreaksPerDoor + " breaks)");
		foreach (KeyValuePair<string, LockpickProfile> tier in picks.Tiers)
		{
			console.Output("  tier " + tier.Key + ": " + tier.Value.Seconds + "s base, " +
				(int)(tier.Value.BreakChance * 100f) + "% base break chance" +
				(tier.Key == picks.DefaultTier ? " (default)" : ""));
		}
		console.Output("  noise: unlock " + Describe(picks.UnlockNoise) +
			" | break " + Describe(picks.BreakNoise) +
			" | sneak factor " + picks.SneakNoiseFactor +
			" | investigate " + picks.InvestigateSeconds + "s");
		console.Output("  edit STFDConfig.xml in the mod folder, then 'stfd reload'.");
	}

	private static string Describe(NoiseProfile noise)
	{
		return noise.Radius + "m/" + (int)(noise.AlertChance * 100f) + "%";
	}

	private static void PrintTier(SdtdConsole console, List<string> _params)
	{
		if (_params.Count < 2)
		{
			console.Output("[STFD] usage: stfd tier <blockName>  (e.g. stfd tier vaultDoor01)");
			return;
		}
		string blockName = _params[1];
		Block block = Block.GetBlockByName(blockName);
		string materialId = block?.blockMaterial?.id;
		if (block == null)
		{
			console.Output("[STFD] block '" + blockName + "' not found (load into a world first?) - " +
				"resolving by name rules only.");
		}
		LockpickSettings picks = Runtime.Config.Lockpicking;
		string tier = picks.ResolveTier(blockName, materialId);
		LockpickProfile profile = picks.ResolveProfile(blockName, materialId);
		console.Output("[STFD] " + blockName + (materialId != null ? " (" + materialId + ")" : "") +
			" -> tier '" + tier + "': " + profile.Seconds + "s base, " +
			(int)(profile.BreakChance * 100f) + "% base break chance.");
	}

	public override string[] getCommands()
	{
		return new string[2] { "stfd", "shutthefriggingdoor" };
	}

	public override string getDescription()
	{
		return "STFD: stfd | stfd reload | stfd tier <blockName>";
	}
}
