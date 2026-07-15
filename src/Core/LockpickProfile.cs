namespace M00STFD.Core;

// How hard one tier of door is to pick: how long a full pick takes and how
// likely each attempt is to snap the lockpick. Perk and item effects are
// applied on top of these base values by the Game layer, exactly as they
// are for vanilla safes.
public readonly struct LockpickProfile
{
	public LockpickProfile(float seconds, float breakChance)
	{
		Seconds = seconds;
		BreakChance = breakChance;
	}

	public float Seconds { get; }

	public float BreakChance { get; }
}
