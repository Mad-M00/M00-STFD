namespace M00STFD.Core;

// One noise a lockpicking action makes: how far it carries and how likely
// each zombie inside that range is to come looking.
public readonly struct NoiseProfile
{
	public NoiseProfile(float radius, float alertChance)
	{
		Radius = radius;
		AlertChance = alertChance;
	}

	public float Radius { get; }

	public float AlertChance { get; }

	public bool IsSilent => Radius <= 0f || AlertChance <= 0f;
}
