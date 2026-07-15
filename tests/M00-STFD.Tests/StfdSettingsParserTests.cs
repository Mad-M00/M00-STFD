using M00STFD.Core;
using Xunit;

namespace M00STFD.Tests;

// The whole-file parser: the close-open-doors switch lives at the top
// level so either half of the mod can run alone. Same config promise:
// missing pieces default on, structural damage means complete defaults.
public class StfdSettingsParserTests
{
	private sealed class RecordingLog : IDoorLog
	{
		public int Warnings;

		public void Info(string message) { }

		public void Warning(string message) => Warnings++;
	}

	[Fact]
	public void Both_features_default_on_when_the_file_says_nothing()
	{
		var settings = StfdSettingsParser.Parse(
			"<STFD></STFD>", new RecordingLog());
		Assert.True(settings.CloseOpenDoorsEnabled);
		Assert.True(settings.Lockpicking.Enabled);
	}

	// The two rules are independent kill switches: a purist can keep the
	// close-doors fix and refuse the lockpicking power creep, or vice versa.
	[Fact]
	public void Each_feature_can_be_switched_off_alone()
	{
		const string xml = @"
<STFD>
  <CloseOpenDoors enabled=""false"" />
  <Lockpicking defaultTier=""iron"">
    <Tiers><Tier name=""iron"" seconds=""12"" breakChance=""0.6"" /></Tiers>
  </Lockpicking>
</STFD>";
		var settings = StfdSettingsParser.Parse(xml, new RecordingLog());
		Assert.False(settings.CloseOpenDoorsEnabled);
		Assert.True(settings.Lockpicking.Enabled);
	}

	[Fact]
	public void Malformed_xml_falls_back_to_complete_defaults()
	{
		var log = new RecordingLog();
		var settings = StfdSettingsParser.Parse("<STFD><Close", log);
		Assert.True(settings.CloseOpenDoorsEnabled);
		Assert.True(settings.Lockpicking.Enabled);
		Assert.True(log.Warnings > 0);
	}

	[Fact]
	public void Missing_file_content_uses_defaults()
	{
		var settings = StfdSettingsParser.Parse(null, new RecordingLog());
		Assert.True(settings.CloseOpenDoorsEnabled);
	}
}
