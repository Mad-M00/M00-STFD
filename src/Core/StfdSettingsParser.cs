using System;
using System.Xml;

namespace M00STFD.Core;

// Parses the whole STFDConfig.xml: the <CloseOpenDoors> feature
// switch here, the <Lockpicking> section via LockpickSettingsParser.
// Same promise as everywhere else: structural damage falls back to the
// complete defaults with a warning.
public static class StfdSettingsParser
{
	public static StfdSettings Parse(string xml, IDoorLog log)
	{
		if (string.IsNullOrEmpty(xml))
		{
			return StfdSettings.Default();
		}
		bool closeOpenDoors = true;
		try
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlElement closeOpen = doc.DocumentElement?["CloseOpenDoors"];
			if (closeOpen != null && bool.TryParse(closeOpen.GetAttribute("enabled"), out bool enabled))
			{
				closeOpenDoors = enabled;
			}
		}
		catch (Exception e)
		{
			log.Warning("malformed STFDConfig.xml - using complete defaults (" + e.Message + ")");
			return StfdSettings.Default();
		}
		return new StfdSettings(closeOpenDoors, LockpickSettingsParser.Parse(xml, log));
	}
}
