using M00STFD.Core;
using Xunit;

namespace M00STFD.Tests;

// The config promise: a broken edit can never half-apply. Structural
// damage falls back to the complete defaults; a single bad rule is
// skipped while the rest of the file still works.
public class LockpickSettingsParserTests
{
	private sealed class RecordingLog : IDoorLog
	{
		public int Warnings;

		public void Info(string message) { }

		public void Warning(string message) => Warnings++;
	}

	private const string ValidXml = @"
<STFD>
  <Lockpicking enabled=""true"" item=""resourceLockPick"" defaultTier=""iron"">
    <Tiers>
      <Tier name=""wood"" seconds=""8"" breakChance=""0.5"" />
      <Tier name=""iron"" seconds=""12"" breakChance=""0.6"" />
      <Tier name=""vault"" seconds=""25"" breakChance=""0.8"" />
    </Tiers>
    <DoorRules>
      <Door match=""vault*"" tier=""vault"" />
    </DoorRules>
    <MaterialRules>
      <Material match=""Mwood*"" tier=""wood"" />
    </MaterialRules>
  </Lockpicking>
</STFD>";

	[Fact]
	public void A_valid_config_applies_its_own_numbers()
	{
		var settings = LockpickSettingsParser.Parse(ValidXml, new RecordingLog());
		Assert.True(settings.Enabled);
		Assert.Equal(25f, settings.ResolveProfile("vaultDoor01", "Msteel").Seconds);
		Assert.Equal(8f, settings.ResolveProfile("someDoor", "Mwood_regular").Seconds);
		Assert.Equal(12f, settings.ResolveProfile("someDoor", "Munknown").Seconds);
	}

	[Fact]
	public void Malformed_xml_falls_back_to_complete_defaults()
	{
		var log = new RecordingLog();
		var settings = LockpickSettingsParser.Parse("<STFD><Lockpicking", log);
		Assert.Equal(LockpickSettings.Default().DefaultTier, settings.DefaultTier);
		Assert.Equal(
			LockpickSettings.Default().ResolveProfile("vaultDoor01", "Msteel").Seconds,
			settings.ResolveProfile("vaultDoor01", "Msteel").Seconds);
		Assert.True(log.Warnings > 0);
	}

	[Fact]
	public void Missing_or_empty_config_uses_defaults_silently()
	{
		var settings = LockpickSettingsParser.Parse(null, new RecordingLog());
		Assert.True(settings.Enabled);
		Assert.Equal("resourceLockPick", settings.LockPickItemName);
	}

	// One bad rule must not take the whole file down with it.
	[Fact]
	public void A_rule_naming_an_unknown_tier_is_skipped_with_a_warning()
	{
		const string xml = @"
<STFD>
  <Lockpicking defaultTier=""iron"">
    <Tiers>
      <Tier name=""iron"" seconds=""12"" breakChance=""0.6"" />
    </Tiers>
    <DoorRules>
      <Door match=""vault*"" tier=""doesNotExist"" />
      <Door match=""jailDoor*"" tier=""iron"" />
    </DoorRules>
  </Lockpicking>
</STFD>";
		var log = new RecordingLog();
		var settings = LockpickSettingsParser.Parse(xml, log);
		Assert.Equal(1, log.Warnings);
		Assert.Equal("iron", settings.ResolveTier("jailDoorGrey", "whatever"));
		Assert.Equal("iron", settings.ResolveTier("vaultDoor01", "whatever"));
	}

	[Fact]
	public void A_default_tier_that_is_not_defined_falls_back_to_defaults()
	{
		const string xml = @"
<STFD>
  <Lockpicking defaultTier=""adamantium"">
    <Tiers>
      <Tier name=""iron"" seconds=""12"" breakChance=""0.6"" />
    </Tiers>
  </Lockpicking>
</STFD>";
		var log = new RecordingLog();
		var settings = LockpickSettingsParser.Parse(xml, log);
		Assert.Equal(LockpickSettings.Default().DefaultTier, settings.DefaultTier);
		Assert.True(log.Warnings > 0);
	}

	[Fact]
	public void Noise_section_overrides_ship_defaults_and_absence_keeps_them()
	{
		const string xml = @"
<STFD>
  <Lockpicking defaultTier=""iron"">
    <Tiers><Tier name=""iron"" seconds=""12"" breakChance=""0.6"" /></Tiers>
    <Noise sneakFactor=""0.4"" investigateSeconds=""45"">
      <Unlock radius=""2"" chance=""0.25"" />
      <Break radius=""12"" chance=""1"" />
    </Noise>
  </Lockpicking>
</STFD>";
		var settings = LockpickSettingsParser.Parse(xml, new RecordingLog());
		Assert.Equal(0.4f, settings.SneakNoiseFactor);
		Assert.Equal(2f, settings.UnlockNoise.Radius);
		Assert.Equal(12f, settings.BreakNoise.Radius);
		Assert.Equal(45f, settings.InvestigateSeconds);

		var withoutNoise = LockpickSettingsParser.Parse(ValidXml, new RecordingLog());
		Assert.Equal(LockpickSettings.DefaultUnlockNoise.Radius, withoutNoise.UnlockNoise.Radius);
		Assert.Equal(LockpickSettings.DefaultSneakNoiseFactor, withoutNoise.SneakNoiseFactor);
		Assert.Equal(LockpickSettings.DefaultInvestigateSeconds, withoutNoise.InvestigateSeconds);
	}

	[Fact]
	public void MaxBreaks_is_parsed_and_defaults_when_absent()
	{
		const string xml = @"
<STFD>
  <Lockpicking defaultTier=""iron"" maxBreaks=""2"">
    <Tiers><Tier name=""iron"" seconds=""12"" breakChance=""0.6"" /></Tiers>
  </Lockpicking>
</STFD>";
		Assert.Equal(2, LockpickSettingsParser.Parse(xml, new RecordingLog()).MaxBreaksPerDoor);
		Assert.Equal(LockpickSettings.DefaultMaxBreaksPerDoor,
			LockpickSettingsParser.Parse(ValidXml, new RecordingLog()).MaxBreaksPerDoor);
	}

	// The kill switch players can flip without uninstalling the mod.
	[Fact]
	public void Enabled_false_is_respected()
	{
		const string xml = @"
<STFD>
  <Lockpicking enabled=""false"" defaultTier=""iron"">
    <Tiers><Tier name=""iron"" seconds=""12"" breakChance=""0.6"" /></Tiers>
  </Lockpicking>
</STFD>";
		Assert.False(LockpickSettingsParser.Parse(xml, new RecordingLog()).Enabled);
	}
}
