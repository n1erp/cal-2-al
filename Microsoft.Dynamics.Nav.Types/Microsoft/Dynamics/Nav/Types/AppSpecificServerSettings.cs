using System;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Dynamics.Nav.Types;

public class AppSpecificServerSettings : ServerSettingsBase<AppSpecificServerSettings>
{
	public const string DefaultLanguageSettingName = "DefaultLanguage";

	public const string SupportedLanguagesSettingName = "SupportedLanguages";

	[ConfigSetting("AppProtocolName", "ms-dynamicsnav", ConfigSettingAttribute.ConfigCategory.WebServices, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public virtual SimpleConfigSettingNoMaxValue<string> AppProtocolName { get; set; }

	[ConfigSetting("AppIdUri", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<Uri> AppIdUri { get; set; }

	[ConfigSetting("AzureActiveDirectoryClientApplicationId", "00000000-0000-0000-0000-000000000000", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Guid> AzureActiveDirectoryClientApplicationId { get; set; }

	[ConfigSetting("DefaultLanguage", "en-US", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public LanguageSetting DefaultLanguage { get; set; }

	[ConfigSetting("SupportedLanguages", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SupportedServerLanguageSettings SupportedLanguages { get; set; }

	[ConfigSetting("FixedTenantWebServiceEndpointBaseUri", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<Uri> FixedTenantWebServiceEndpointBaseUri { get; set; }

	[ConfigSetting("FixedWebClientEndpointBaseUri", "", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<Uri> FixedWebClientEndpointBaseUri { get; set; }

	public static IXPathNavigable GetApplicationSpecificSettings(string appConfigPath, string applicationId, bool applyDefaults)
	{
		if (string.IsNullOrEmpty(appConfigPath))
		{
			throw new ArgumentNullException("appConfigPath");
		}
		if (string.IsNullOrEmpty(applicationId))
		{
			throw new ArgumentNullException("applicationId");
		}
		string text = Path.GetDirectoryName(appConfigPath) + "\\CustomSettings." + applicationId + ".config";
		if (!File.Exists(text))
		{
			return null;
		}
		string tempFileName = Path.GetTempFileName();
		try
		{
			System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
			{
				ExeConfigFilename = text
			}, ConfigurationUserLevel.None);
			if (applyDefaults)
			{
				ServerSettingsBase<AppSpecificServerSettings>.ApplyDefaultSettings(configuration);
			}
			XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
			configuration.AppSettings.File = null;
			configuration.SaveAs(tempFileName);
			using XmlReader reader = XmlHelper.SafeXmlReader(tempFileName);
			xmlDocument.Load(reader);
			return xmlDocument;
		}
		finally
		{
			if (File.Exists(tempFileName))
			{
				File.Delete(tempFileName);
			}
		}
	}
}
