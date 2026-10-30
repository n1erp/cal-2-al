using System;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Types;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ConfigSettingAttribute : Attribute
{
	public enum ConfigCategory
	{
		General,
		Database,
		ClientServices,
		WebServices,
		ODataServices,
		NASService,
		Management,
		AzureKeyVault,
		AzureActiveDirectory,
		TaskScheduler,
		Reports,
		FeatureSwitches,
		Extensions,
		Development,
		Compatibility,
		HybridDeploy
	}

	private const string DescriptionPropertyName = "ConfigurationSetting{0}Description";

	private const string DisplayNamePropertyName = "ConfigurationSetting{0}DisplayName";

	public string SettingName { get; private set; }

	public object DefaultValue { get; private set; }

	public ConfigSettings.EmitSetting EmitSetting { get; private set; }

	public string Description => Localization.GetNonLocalizableString(string.Format(CultureInfo.CurrentCulture, "ConfigurationSetting{0}Description", SettingName)) ?? string.Empty;

	public ConfigCategory Category { get; private set; }

	public string DisplayName => Localization.GetNonLocalizableString(string.Format(CultureInfo.CurrentCulture, "ConfigurationSetting{0}DisplayName", SettingName)) ?? SettingName;

	public bool InternalUseOnly { get; set; }

	public bool DynamicallyUpdatable { get; set; }

	public ConfigSettingAttribute(string settingName, object defaultValue, ConfigCategory category, ConfigSettings.EmitSetting emitSetting)
	{
		SettingName = settingName;
		DefaultValue = defaultValue;
		Category = category;
		EmitSetting = emitSetting;
	}

	internal string GetFormattedDescription(object target, PropertyInfo pi)
	{
		object value = pi.GetValue(target);
		string arg = ((value != null) ? value.ToString() : string.Empty);
		if (EmitSetting == ConfigSettings.EmitSetting.Restricted)
		{
			arg = ((value is IRestrictedConfigurationProperty restrictedConfigurationProperty) ? restrictedConfigurationProperty.RestrictedString() : "<redacted>");
		}
		return string.Format(CultureInfo.CurrentCulture, "{0} = {1}{2}", SettingName ?? string.Empty, arg, Environment.NewLine);
	}
}
