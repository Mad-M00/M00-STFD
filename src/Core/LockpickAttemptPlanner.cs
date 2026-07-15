namespace M00STFD.Core;

// One pick attempt, planned up front the way vanilla plans safe-cracking:
// the timer resumes from where the last broken pick left off, and whether
// (and when) this attempt snaps is decided before the timer starts, from
// two random rolls the caller supplies. Injecting the rolls keeps the
// maths deterministic and testable.
public readonly struct LockpickAttemptPlan
{
	public LockpickAttemptPlan(float timeLeftAtStart, float breakAtElapsed)
	{
		TimeLeftAtStart = timeLeftAtStart;
		BreakAtElapsed = breakAtElapsed;
	}

	// Seconds still needed on the timer (full time minus earlier progress).
	public float TimeLeftAtStart { get; }

	// Elapsed-time mark (measured against the full pick time) at which the
	// pick snaps, or -1 when this attempt runs through to success. Matches
	// the game timer's AlternateTime semantics.
	public float BreakAtElapsed { get; }

	public bool WillBreak => BreakAtElapsed >= 0f;
}

public static class LockpickAttemptPlanner
{
	// The snap never happens in the first or last instants of the attempt;
	// same feel as vanilla's 0.05..0.95 clamp.
	private const float EarliestBreakFraction = 0.05f;
	private const float LatestBreakFraction = 0.95f;

	public static LockpickAttemptPlan Plan(
		float fullTimeSeconds,
		float completion,
		float breakChance,
		int breaksSoFar,
		int maxBreaksPerDoor,
		float breakRoll,
		float breakMomentRoll)
	{
		completion = Clamp(completion, 0f, 0.99f);
		float remaining = fullTimeSeconds * (1f - completion);
		// The break chance applies to the work still ahead, not to the whole
		// lock again: a fresh attempt risks the full chance, a retry at 90%
		// progress risks a tenth of it. Without this, every retry re-rolls
		// the full chance and a lock can snap 3-7 times in its last seconds
		// — statistically common, and it feels terrible. The mercy cap ends
		// even an unlucky streak: after maxBreaksPerDoor snaps on the same
		// door, the next attempt is guaranteed to run through.
		float effectiveChance = (breaksSoFar >= maxBreaksPerDoor)
			? 0f
			: breakChance * (1f - completion);
		if (breakRoll >= effectiveChance)
		{
			return new LockpickAttemptPlan(remaining, -1f);
		}
		float fraction = Clamp(breakMomentRoll, EarliestBreakFraction, LatestBreakFraction);
		float breakAtElapsed = (fullTimeSeconds - remaining) + fraction * remaining;
		return new LockpickAttemptPlan(remaining, breakAtElapsed);
	}

	private static float Clamp(float value, float min, float max)
	{
		if (value < min)
		{
			return min;
		}
		return value > max ? max : value;
	}
}
