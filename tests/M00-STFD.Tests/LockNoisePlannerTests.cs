using M00STFD.Core;
using Xunit;

namespace M00STFD.Tests;

// Lockpicking noise is the risk that balances the reward: the unlock
// click can wake what's directly behind the door, a snapping pick can
// pull the room. Sneaking scales both down. These pin the scaling and
// the shipped loudness ordering — quiet click, loud snap.
public class LockNoisePlannerTests
{
	[Fact]
	public void Standing_players_make_the_full_noise()
	{
		var scaled = LockNoisePlanner.Scale(new NoiseProfile(8f, 0.8f), quietnessFactor: 1f);
		Assert.Equal(8f, scaled.Radius);
		Assert.Equal(0.8f, scaled.AlertChance);
	}

	// "If sneaking, the noise is reduced" — both reach and chance shrink,
	// so sneaking helps even when a zombie is inside the smaller radius.
	[Fact]
	public void Sneaking_shrinks_both_radius_and_chance()
	{
		var scaled = LockNoisePlanner.Scale(new NoiseProfile(8f, 0.8f), quietnessFactor: 0.5f);
		Assert.Equal(4f, scaled.Radius);
		Assert.Equal(0.4f, scaled.AlertChance);
	}

	// Perk maths must never amplify: a broken effect value above 1 (or a
	// negative one) is clamped rather than trusted.
	[Fact]
	public void The_quietness_factor_is_clamped_to_sane_bounds()
	{
		Assert.Equal(8f, LockNoisePlanner.Scale(new NoiseProfile(8f, 0.8f), 1.5f).Radius);
		Assert.True(LockNoisePlanner.Scale(new NoiseProfile(8f, 0.8f), -1f).IsSilent);
	}

	[Fact]
	public void The_alert_roll_decides_against_the_chance()
	{
		Assert.True(LockNoisePlanner.ShouldAlert(0.5f, roll: 0.4f));
		Assert.False(LockNoisePlanner.ShouldAlert(0.5f, roll: 0.6f));
	}

	// The server never trusts noise numbers a client sends: radius is
	// capped at the loudest noise the server's own config can produce,
	// chance at certainty. A hacked client can at worst make the noise
	// the mod could already make.
	[Fact]
	public void Incoming_network_noise_is_clamped_to_server_limits()
	{
		var clamped = LockNoisePlanner.ClampIncoming(new NoiseProfile(500f, 7f), maxRadius: 8f);
		Assert.Equal(8f, clamped.Radius);
		Assert.Equal(1f, clamped.AlertChance);
		Assert.True(LockNoisePlanner.ClampIncoming(new NoiseProfile(-3f, 0.5f), 8f).IsSilent);
	}

	// The shipped balance: a snapping pick is louder and more alerting
	// than the unlock click. Swapping them inverts the risk model.
	[Fact]
	public void A_breaking_pick_is_louder_than_the_unlock_click()
	{
		Assert.True(LockpickSettings.DefaultBreakNoise.Radius > LockpickSettings.DefaultUnlockNoise.Radius);
		Assert.True(LockpickSettings.DefaultBreakNoise.AlertChance > LockpickSettings.DefaultUnlockNoise.AlertChance);
	}
}
