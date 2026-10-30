using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public class ConfigurationSettingsProvider : SettingsProvider
{
	private readonly object syncObject;

	private System.Configuration.Configuration configuration;

	public string SettingsFileName { get; }

	public ConfigurationSettingsProvider(string settingsFileName)
		: this(settingsFileName, new object())
	{
	}

	private ConfigurationSettingsProvider(string settingsFileName, object syncObject)
	{
		SettingsFileName = settingsFileName;
		this.syncObject = syncObject;
		Refresh();
	}

	public override bool TryGetConfigurationSettingValue(string settingName, out string value, HashSet<string> validValues = null, System.Configuration.Configuration config = null)
	{
		value = GetConfigurationSettingValue(settingName, out var exist, validValues);
		return exist;
	}

	protected string GetConfigurationSettingValue(string settingName, out bool exist, HashSet<string> validValues = null, System.Configuration.Configuration config = null)
	{
		try
		{
			lock (syncObject)
			{
				System.Configuration.Configuration configuration = config ?? this.configuration;
				if (configuration == null)
				{
					exist = false;
					return null;
				}
				KeyValueConfigurationElement keyValueConfigurationElement = configuration.AppSettings.Settings[settingName];
				if (keyValueConfigurationElement != null)
				{
					if (validValues != null && !validValues.Contains(keyValueConfigurationElement.Value))
					{
						throw new NavConfigurationException(Lang.ConfigurationInvalidValue, settingName);
					}
					exist = true;
					return keyValueConfigurationElement.Value;
				}
				exist = false;
				return null;
			}
		}
		catch (ConfigurationErrorsException ex)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationFileError, ex.Message), ex);
		}
	}

	public override void Refresh()
	{
		lock (syncObject)
		{
			configuration = (string.IsNullOrEmpty(SettingsFileName) ? GetDefaultConfiguration() : GetConfiguration(SettingsFileName));
		}
	}

	private static System.Configuration.Configuration GetDefaultConfiguration()
	{
		try
		{
			return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
		}
		catch (ConfigurationErrorsException ex)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationFileError, ex.Message), ex);
		}
	}

	protected virtual System.Configuration.Configuration GetConfiguration(string fileName)
	{
		ExeConfigurationFileMap exeConfigurationFileMap = new ExeConfigurationFileMap();
		exeConfigurationFileMap.ExeConfigFilename = fileName;
		if (!File.Exists(exeConfigurationFileMap.ExeConfigFilename))
		{
			throw new NavConfigurationException(Lang.ConfigurationFileRequired, exeConfigurationFileMap.ExeConfigFilename);
		}
		try
		{
			return ConfigurationManager.OpenMappedExeConfiguration(exeConfigurationFileMap, ConfigurationUserLevel.None);
		}
		catch (ConfigurationErrorsException ex)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationFileError, ex.Message), ex);
		}
	}
}
