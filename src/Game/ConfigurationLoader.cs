using System;
using System.IO;
using M00STFD.Core;

namespace M00STFD.Game;

internal static class ConfigurationLoader
{
	public static StfdSettings LoadSettings(string configPath, IDoorLog log)
	{
		try
		{
			if (!File.Exists(configPath))
			{
				log.Info("no STFDConfig.xml found - using built-in defaults");
				return StfdSettings.Default();
			}
			return StfdSettingsParser.Parse(File.ReadAllText(configPath), log);
		}
		catch (Exception e)
		{
			log.Warning("could not read " + configPath + " - using built-in defaults (" +
				e.Message + ")");
			return StfdSettings.Default();
		}
	}
}
