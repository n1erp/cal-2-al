using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Microsoft.Dynamics.Nav.Types;

public class AppSpecificConfigurationSettingsProvider : ConfigurationSettingsProvider
{
	private string configFilePath;

	internal string ConfigFileName
	{
		get
		{
			if (configFilePath == null)
			{
				string text = (string.IsNullOrEmpty(ServerUserSettings.SettingsFileName) ? null : Path.GetDirectoryName(ServerUserSettings.SettingsFileName));
				string text2 = "CustomSettings." + SettingsFileName + ".config";
				configFilePath = ((text == null) ? text2 : Path.Combine(text, text2));
			}
			return configFilePath;
		}
	}

	public AppSpecificConfigurationSettingsProvider(string applicationId)
		: base(applicationId)
	{
	}

	public override bool TryGetConfigurationSettingValue(string settingName, out string value, HashSet<string> validValues = null, System.Configuration.Configuration config = null)
	{
		bool exist = false;
		value = GetConfigurationSettingValue(settingName, out exist, validValues, GetConfiguration(null));
		return exist;
	}

	public bool ConfigFileExists()
	{
		return File.Exists(ConfigFileName);
	}

	protected override System.Configuration.Configuration GetConfiguration(string fileName)
	{
		if (!ConfigFileExists())
		{
			return null;
		}
		return ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
		{
			ExeConfigFilename = ConfigFileName
		}, ConfigurationUserLevel.None);
	}
}
