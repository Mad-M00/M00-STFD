namespace M00STFD.Core;

// Scales a noise by how quiet the player is being. The factor combines
// the sneak baseline (crouched players are just quieter) with the game's
// own NoiseMultiplier passive effect, which the From The Shadows perk
// reduces while crouching — so "reduced based on sneaking skill" uses the
// exact perk the player levelled for it. Standing players pass factor 1.
public static class LockNoisePlanner
{
	public static NoiseProfile Scale(NoiseProfile noise, float quietnessFactor)
	{
		float factor = Clamp01(quietnessFactor);
		return new NoiseProfile(noise.Radius * factor, noise.AlertChance * factor);
	}

	public static bool ShouldAlert(float alertChance, float roll)
	{
		return roll < alertChance;
	}

	// Sanitises a noise that arrived over the network. The client computes
	// the sneak-scaled noise (it knows crouch and perk state), but the
	// server never trusts the numbers: radius is capped at the loudest
	// noise the server's own config can produce, chance at certainty.
	public static NoiseProfile ClampIncoming(NoiseProfile noise, float maxRadius)
	{
		float radius = noise.Radius;
		if (radius < 0f)
		{
			radius = 0f;
		}
		if (radius > maxRadius)
		{
			radius = maxRadius;
		}
		return new NoiseProfile(radius, Clamp01(noise.AlertChance));
	}

	private static float Clamp01(float value)
	{
		if (value < 0f)
		{
			return 0f;
		}
		return value > 1f ? 1f : value;
	}
}
