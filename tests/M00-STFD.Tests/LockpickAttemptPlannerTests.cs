using M00STFD.Core;
using Xunit;

namespace M00STFD.Tests;

// The attempt maths mirrors vanilla safe-cracking: progress from a broken
// pick carries into the next attempt, and the snap moment is decided up
// front against the full pick time (the game timer's AlternateTime is an
// elapsed-time mark). Two rules keep retries fair — playtest-driven, both:
// the break chance applies to the work still AHEAD (a lock at 95% must
// not keep the full snap odds), and a mercy cap guarantees the attempt
// after enough snaps on the same door.
public class LockpickAttemptPlannerTests
{
	private static LockpickAttemptPlan Plan(
		float fullTime = 20f,
		float completion = 0f,
		float breakChance = 0f,
		int breaksSoFar = 0,
		int maxBreaks = 4,
		float breakRoll = 0.9f,
		float breakMomentRoll = 0.5f)
	{
		return LockpickAttemptPlanner.Plan(fullTime, completion, breakChance,
			breaksSoFar, maxBreaks, breakRoll, breakMomentRoll);
	}

	[Fact]
	public void A_fresh_attempt_runs_the_full_time()
	{
		var plan = Plan();
		Assert.Equal(20f, plan.TimeLeftAtStart);
		Assert.False(plan.WillBreak);
	}

	// The whole point of tracking completion: a snapped pick must not send
	// the player back to zero.
	[Fact]
	public void Progress_from_a_broken_pick_shortens_the_next_attempt()
	{
		Assert.Equal(5f, Plan(completion: 0.75f).TimeLeftAtStart, precision: 3);
	}

	[Fact]
	public void A_fresh_attempt_risks_the_full_break_chance()
	{
		Assert.True(Plan(breakChance: 0.75f, breakRoll: 0.5f).WillBreak);
		Assert.False(Plan(breakChance: 0.75f, breakRoll: 0.8f).WillBreak);
	}

	// The playtest complaint this fixes: retries near the end re-rolled the
	// full 0.75 and locks snapped 3-7 times in their last seconds. The
	// chance now covers only the remaining work — at 90% progress a 0.75
	// base chance is really 0.075.
	[Fact]
	public void Retry_break_chance_shrinks_with_progress()
	{
		Assert.False(Plan(completion: 0.9f, breakChance: 0.75f, breakRoll: 0.5f).WillBreak);
		Assert.True(Plan(completion: 0.9f, breakChance: 0.75f, breakRoll: 0.05f).WillBreak);
	}

	// The mercy cap: once a door has eaten maxBreaks picks, the next
	// attempt runs through no matter the roll — an unlucky streak ends.
	[Fact]
	public void The_mercy_cap_guarantees_the_attempt_after_enough_snaps()
	{
		Assert.False(Plan(breakChance: 1f, breaksSoFar: 4, maxBreaks: 4, breakRoll: 0f).WillBreak);
		Assert.True(Plan(breakChance: 1f, breaksSoFar: 3, maxBreaks: 4, breakRoll: 0f).WillBreak);
	}

	// AlternateTime is measured in elapsed-time against the FULL pick time,
	// so a resumed attempt must schedule its snap after the already-earned
	// progress, inside the remaining window only.
	[Fact]
	public void The_snap_moment_lands_inside_the_remaining_window()
	{
		var plan = Plan(completion: 0.5f, breakChance: 1f, breakRoll: 0f, breakMomentRoll: 0.5f);
		Assert.Equal(10f, plan.TimeLeftAtStart, precision: 3);
		Assert.Equal(15f, plan.BreakAtElapsed, precision: 3); // 10 elapsed + 0.5 * 10 remaining
	}

	// No instant snaps at the very start, no heartbreak in the last frames.
	[Fact]
	public void The_snap_moment_is_clamped_away_from_the_edges()
	{
		Assert.Equal(1f, Plan(breakChance: 1f, breakRoll: 0f, breakMomentRoll: 0f).BreakAtElapsed, precision: 3);
		Assert.Equal(19f, Plan(breakChance: 1f, breakRoll: 0f, breakMomentRoll: 1f).BreakAtElapsed, precision: 3);
	}
}
