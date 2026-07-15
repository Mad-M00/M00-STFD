using M00STFD.Core;
using Xunit;

namespace M00STFD.Tests;

// The difficulty config keys off these patterns; a matching bug silently
// re-tiers doors, so the anchoring rules are pinned explicitly.
public class WildcardPatternTests
{
	[Fact]
	public void Prefix_pattern_matches_the_run_after_the_star()
	{
		var pattern = new WildcardPattern("vault*");
		Assert.True(pattern.IsMatch("vaultDoor01"));
		Assert.True(pattern.IsMatch("vaultHatch01_Powered"));
		Assert.False(pattern.IsMatch("theVaultDoor"));
	}

	[Fact]
	public void Pattern_without_a_star_is_an_exact_match()
	{
		var pattern = new WildcardPattern("manholeHatch");
		Assert.True(pattern.IsMatch("manholeHatch"));
		Assert.False(pattern.IsMatch("manholeHatch01"));
	}

	[Fact]
	public void Contains_pattern_matches_anywhere()
	{
		var pattern = new WildcardPattern("*GarageDoor*");
		Assert.True(pattern.IsMatch("steelGarageDoor3x3Black"));
		Assert.True(pattern.IsMatch("ironGarageDoor_PoweredRed"));
		Assert.False(pattern.IsMatch("rollUpDoor3x3Black"));
	}

	// Config files are hand-edited; case must never matter.
	[Fact]
	public void Matching_ignores_case()
	{
		Assert.True(new WildcardPattern("JAILDOOR*").IsMatch("jailDoorDoubleGrey"));
	}

	// Regex metacharacters in block names are literal text, not syntax.
	[Fact]
	public void Special_characters_are_matched_literally()
	{
		Assert.True(new WildcardPattern("door.v(1)*").IsMatch("door.v(1)Fancy"));
		Assert.False(new WildcardPattern("door.v(1)*").IsMatch("doorXv(1)Fancy"));
	}

	[Fact]
	public void Null_candidate_never_matches()
	{
		Assert.False(new WildcardPattern("*").IsMatch(null));
	}
}
