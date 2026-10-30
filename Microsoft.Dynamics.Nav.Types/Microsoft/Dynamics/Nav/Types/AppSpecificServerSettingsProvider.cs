using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Types;

internal static class AppSpecificServerSettingsProvider
{
	public static ConcurrentDictionary<string, AppSpecificServerSettings> GetAppSpecificSettings(SettingsProvider provider)
	{
		ConcurrentDictionary<string, AppSpecificServerSettings> concurrentDictionary = new ConcurrentDictionary<string, AppSpecificServerSettings>();
		if (provider == null)
		{
			return concurrentDictionary;
		}
		string value = null;
		provider.TryGetConfigurationSettingValue("AppSpecificConfigurationSettings", out value);
		if (!string.IsNullOrEmpty(value))
		{
			foreach (AppConfigurationSetting item in JsonConvert.DeserializeObject<List<AppConfigurationSetting>>(value))
			{
				PopulateAppSpecificSettingsDictionary(concurrentDictionary, item.ApplicationConfigurationId, item.ServerSettings);
			}
		}
		return concurrentDictionary;
	}

	private static void PopulateAppSpecificSettingsDictionary(ConcurrentDictionary<string, AppSpecificServerSettings> appSpecificSettingsDict, string appConfigurationId, IDictionary<string, string> serverSettingsDict)
	{
		AppSpecificServerSettings appSpecificServerSettings = new AppSpecificServerSettings();
		foreach (KeyValuePair<string, string> item in serverSettingsDict)
		{
			string key = item.Key;
			string value = item.Value;
			ServerSettingsBase<AppSpecificServerSettings>.GetSettingDefinition(key).SetValue(appSpecificServerSettings, value);
		}
		appSpecificSettingsDict[appConfigurationId] = appSpecificServerSettings;
	}
}
