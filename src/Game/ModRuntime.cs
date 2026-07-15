using M00STFD.Core;

namespace M00STFD.Game;

// Composition root. Built once in InitMod; Harmony patches reach the object
// graph through StfdMod.Runtime (patches are static entry points, so
// one static bridge is unavoidable — everything behind it is instance-based).
// Services receive settings through accessor functions rather than
// instances, so "stfd reload" swaps the configuration by replacing one
// object here and every service sees it immediately.
internal sealed class ModRuntime
{
	private readonly string configPath;

	private ModRuntime(string modPath)
	{
		Log = new UnityDoorLog();
		configPath = modPath + "/STFDConfig.xml";
		Config = ConfigurationLoader.LoadSettings(configPath, Log);
		Doors = new DoorUnlockService(Log, () => Config);
		Noise = new DoorNoiseService(Log, () => Config.Lockpicking);
		Lockpicks = new DoorLockpickService(Log, () => Config.Lockpicking, Noise);
	}

	public static ModRuntime Create(string modPath) => new(modPath);

	public IDoorLog Log { get; }

	public StfdSettings Config { get; private set; }

	public DoorUnlockService Doors { get; }

	public DoorNoiseService Noise { get; }

	public DoorLockpickService Lockpicks { get; }

	public void ReloadConfiguration()
	{
		Config = ConfigurationLoader.LoadSettings(configPath, Log);
	}

	// Called when the player leaves the world so session state (partial
	// lockpick progress) never leaks into the next world.
	public void HandleWorldUnloaded()
	{
		Lockpicks.ClearWorldState();
	}
}

internal static class StfdMod
{
	public static ModRuntime Runtime;
}
