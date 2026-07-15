namespace M00STFD.Core;

// Everything the mod does, as one configurable object: the close-open-doors
// rule can be switched off independently of lockpicking, so users can run
// either half of the mod alone. Loaded from STFDConfig.xml; a
// missing or broken file means complete defaults, never a half-applied mix.
public sealed class StfdSettings
{
	public StfdSettings(bool closeOpenDoorsEnabled, LockpickSettings lockpicking)
	{
		CloseOpenDoorsEnabled = closeOpenDoorsEnabled;
		Lockpicking = lockpicking;
	}

	// Rule 1: world doors standing open but locked become unlocked.
	public bool CloseOpenDoorsEnabled { get; }

	// Rule 2 and all its tuning (tiers, rules, noise, mercy cap).
	public LockpickSettings Lockpicking { get; }

	public static StfdSettings Default()
	{
		return new StfdSettings(closeOpenDoorsEnabled: true, LockpickSettings.Default());
	}
}
