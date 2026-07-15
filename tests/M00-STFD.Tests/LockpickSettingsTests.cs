using M00STFD.Core;
using Xunit;

namespace M00STFD.Tests;

// Difficulty resolution is a cascade: door-name rules first (top of the
// file wins), then material rules, then the default tier. Reordering the
// cascade re-tiers doors across the whole game, so each step is pinned.
public class LockpickSettingsTests
{
	private static readonly LockpickSettings Defaults = LockpickSettings.Default();

	[Fact]
	public void Vault_doors_resolve_by_name_before_their_steel_material()
	{
		// vaultDoor01's material is Msteel; the name rule must win so vaults
		// stay harder than ordinary steel doors.
		Assert.Equal("vault", Defaults.ResolveTier("vaultDoor01", "Msteel"));
	}

	[Fact]
	public void Unmatched_names_fall_back_to_the_material()
	{
		Assert.Equal("wood", Defaults.ResolveTier("exteriorHouseDoorOak", "Mwood_regular"));
		Assert.Equal("steel", Defaults.ResolveTier("someModdedDoor", "Msteel"));
	}

	[Fact]
	public void Unknown_name_and_material_use_the_default_tier()
	{
		Assert.Equal(Defaults.DefaultTier, Defaults.ResolveTier("mysteryDoor", "Mmystery"));
	}

	// The four shipped tiers must stay ordered easiest-to-hardest; a config
	// regression here changes game balance everywhere at once.
	[Fact]
	public void Default_tiers_escalate_wood_iron_steel_vault()
	{
		float wood = Defaults.ResolveProfile("oldWoodDoor", "Mwood_regular").Seconds;
		float iron = Defaults.ResolveProfile("ironDoorGrey", "Mmetal").Seconds;
		float steel = Defaults.ResolveProfile("jailDoorGrey", "Msteel").Seconds;
		float vault = Defaults.ResolveProfile("vaultDoor01", "Msteel").Seconds;
		Assert.True(wood < iron && iron < steel && steel < vault);
	}

	[Fact]
	public void Representative_door_families_land_on_their_intended_tiers()
	{
		Assert.Equal("steel", Defaults.ResolveTier("jailDoorDoubleGrey", "Mmetal"));
		Assert.Equal("steel", Defaults.ResolveTier("commercialBulletproofGlassDoor", "MglassBulletproof"));
		Assert.Equal("iron", Defaults.ResolveTier("rollUpDoor5x4Grey", "Mmetal_medium"));
		Assert.Equal("iron", Defaults.ResolveTier("ironHatchBlack", "Mmetal"));
		Assert.Equal("iron", Defaults.ResolveTier("chainlinkGateDoubleWide", "Mmetal"));
		Assert.Equal("wood", Defaults.ResolveTier("woodHatch", "Mwood_regular"));
	}
}
