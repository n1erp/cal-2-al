using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public abstract class ServerSettingsBase<T>
{
	private static object syncLock = new object();

	private static Dictionary<string, ServerSettingDefinition> settingDefinitions;

	private static Dictionary<string, Func<T, object>> settingGetters;

	public static Dictionary<string, ServerSettingDefinition> SettingDefinitions
	{
		get
		{
			if (settingDefinitions == null)
			{
				CreateServerSettingDefinitions();
			}
			return settingDefinitions;
		}
	}

	private static void CreateServerSettingDefinitions()
	{
		lock (syncLock)
		{
			if (settingDefinitions != null)
			{
				return;
			}
			settingDefinitions = new Dictionary<string, ServerSettingDefinition>(StringComparer.OrdinalIgnoreCase);
			settingGetters = new Dictionary<string, Func<T, object>>(StringComparer.OrdinalIgnoreCase);
			PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in properties)
			{
				ConfigSettingAttribute configSettingAttribute = (ConfigSettingAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(ConfigSettingAttribute));
				if (configSettingAttribute != null)
				{
					settingDefinitions.Add(configSettingAttribute.SettingName, new ServerSettingDefinition(propertyInfo.Name, configSettingAttribute, propertyInfo));
					settingGetters.Add(configSettingAttribute.SettingName, propertyInfo.BuildUntypedGetter<T>());
				}
			}
		}
	}

	public static ServerSettingDefinition GetSettingDefinition(string settingName)
	{
		if (!SettingDefinitions.TryGetValue(settingName, out var value))
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToResolveSettingsName, settingName, ProductNames.ServerServiceDisplayName));
		}
		return value;
	}

	public static bool SettingExists(string settingName)
	{
		return SettingDefinitions.ContainsKey(settingName);
	}

	public static object ReadSettingValue(T instance, string settingName)
	{
		if (settingGetters == null)
		{
			CreateServerSettingDefinitions();
		}
		return settingGetters[settingName](instance);
	}

	internal static void ApplyDefaultSettings(System.Configuration.Configuration configuration)
	{
		foreach (ServerSettingDefinition value in SettingDefinitions.Values)
		{
			if (configuration.AppSettings.Settings[value.Definition.SettingName] == null)
			{
				configuration.AppSettings.Settings.Add(value.Definition.SettingName, value.Definition.DefaultValue.ToString());
			}
		}
	}
}
