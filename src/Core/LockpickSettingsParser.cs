using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace M00STFD.Core;

// Parses STFDConfig.xml into LockpickSettings. The promise, same
// as the other M00 mods: a broken config edit can never half-apply. Any
// structural problem falls back to the complete built-in defaults with a
// warning; individually bad rules are skipped with a warning while the
// rest of the file still applies.
public static class LockpickSettingsParser
{
	public static LockpickSettings Parse(string xml, IDoorLog log)
	{
		if (string.IsNullOrEmpty(xml))
		{
			return LockpickSettings.Default();
		}
		try
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlElement root = doc.DocumentElement;
			XmlElement lockpicking = root?["Lockpicking"];
			if (lockpicking == null)
			{
				log.Warning("config has no <Lockpicking> element - using defaults");
				return LockpickSettings.Default();
			}

			bool enabled = ParseBool(lockpicking.GetAttribute("enabled"), defaultValue: true);
			string item = Attribute(lockpicking, "item", "resourceLockPick");
			string defaultTier = Attribute(lockpicking, "defaultTier", "iron");
			int maxBreaks = ParseInt(lockpicking.GetAttribute("maxBreaks"),
				LockpickSettings.DefaultMaxBreaksPerDoor);

			var tiers = new Dictionary<string, LockpickProfile>();
			foreach (XmlElement tier in Elements(lockpicking["Tiers"], "Tier"))
			{
				string name = tier.GetAttribute("name");
				if (string.IsNullOrEmpty(name))
				{
					log.Warning("config: <Tier> without a name skipped");
					continue;
				}
				tiers[name] = new LockpickProfile(
					ParseFloat(tier.GetAttribute("seconds"), 15f),
					ParseFloat(tier.GetAttribute("breakChance"), 0.75f));
			}
			if (tiers.Count == 0 || !tiers.ContainsKey(defaultTier))
			{
				log.Warning("config: no usable tiers (or defaultTier '" + defaultTier +
					"' undefined) - using defaults");
				return LockpickSettings.Default();
			}

			List<LockpickRule> doorRules = ReadRules(lockpicking["DoorRules"], "Door", "match", tiers, log);
			List<LockpickRule> materialRules = ReadRules(lockpicking["MaterialRules"], "Material", "match", tiers, log);

			XmlElement noise = lockpicking["Noise"];
			float sneakFactor = LockpickSettings.DefaultSneakNoiseFactor;
			float investigateSeconds = LockpickSettings.DefaultInvestigateSeconds;
			NoiseProfile unlockNoise = LockpickSettings.DefaultUnlockNoise;
			NoiseProfile breakNoise = LockpickSettings.DefaultBreakNoise;
			if (noise != null)
			{
				sneakFactor = ParseFloat(noise.GetAttribute("sneakFactor"), sneakFactor);
				investigateSeconds = ParseFloat(noise.GetAttribute("investigateSeconds"), investigateSeconds);
				unlockNoise = ReadNoise(noise["Unlock"], unlockNoise);
				breakNoise = ReadNoise(noise["Break"], breakNoise);
			}

			return new LockpickSettings(enabled, item, defaultTier, tiers, doorRules, materialRules,
				unlockNoise, breakNoise, sneakFactor, maxBreaks, investigateSeconds);
		}
		catch (Exception e)
		{
			log.Warning("malformed STFDConfig.xml - using complete defaults (" + e.Message + ")");
			return LockpickSettings.Default();
		}
	}

	private static NoiseProfile ReadNoise(XmlElement element, NoiseProfile defaults)
	{
		if (element == null)
		{
			return defaults;
		}
		return new NoiseProfile(
			ParseFloat(element.GetAttribute("radius"), defaults.Radius),
			ParseFloat(element.GetAttribute("chance"), defaults.AlertChance));
	}

	private static List<LockpickRule> ReadRules(
		XmlElement parent, string elementName, string matchAttribute,
		Dictionary<string, LockpickProfile> tiers, IDoorLog log)
	{
		var rules = new List<LockpickRule>();
		foreach (XmlElement rule in Elements(parent, elementName))
		{
			string match = rule.GetAttribute(matchAttribute);
			string tier = rule.GetAttribute("tier");
			if (string.IsNullOrEmpty(match) || string.IsNullOrEmpty(tier))
			{
				log.Warning("config: <" + elementName + "> needs match and tier attributes - skipped");
				continue;
			}
			if (!tiers.ContainsKey(tier))
			{
				log.Warning("config: <" + elementName + " match=\"" + match + "\"> names unknown tier '" +
					tier + "' - skipped");
				continue;
			}
			rules.Add(new LockpickRule(new WildcardPattern(match), tier));
		}
		return rules;
	}

	private static IEnumerable<XmlElement> Elements(XmlElement parent, string name)
	{
		if (parent == null)
		{
			yield break;
		}
		foreach (XmlNode node in parent.ChildNodes)
		{
			if (node is XmlElement element && element.Name == name)
			{
				yield return element;
			}
		}
	}

	private static string Attribute(XmlElement element, string name, string defaultValue)
	{
		string value = element.GetAttribute(name);
		return string.IsNullOrEmpty(value) ? defaultValue : value;
	}

	private static bool ParseBool(string text, bool defaultValue)
	{
		return bool.TryParse(text, out bool value) ? value : defaultValue;
	}

	private static float ParseFloat(string text, float defaultValue)
	{
		return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)
			? value
			: defaultValue;
	}

	private static int ParseInt(string text, int defaultValue)
	{
		return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
			? value
			: defaultValue;
	}
}
