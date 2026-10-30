using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Configuration;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class ServerUserSettings : ServerSettingsBase<ServerUserSettings>
{
	public const string ServerNameSettingName = "DatabaseServer";

	public const string DatabaseInstanceNameSettingName = "DatabaseInstance";

	public const string ServicesLanguageSettingsName = "ServicesLanguage";

	public const string DefaultLanguageSettingName = "DefaultLanguage";

	public const string SupportedLanguagesSettingName = "SupportedLanguages";

	public const string FederationMetadataLocationSettingsName = "ClientServicesFederationMetadataLocation";

	public const string CommonTenantName = "common";

	private const string ProductNamesPrefix = "ProductNames_";

	public const int DefaultSqlBulkImportBatchSize = 448;

	public const string FeatureSwitchesEnvironmentVariable = "NAV_INTERNAL_FEATURESWITCHOVERRIDES";

	public const string ServiceFabricServiceClientType = "ServiceFabricService";

	public const string WindowsServiceClientType = "WindowsService";

	public const int DefaultReportSettingsLookupPageId = 1560;

	private TimeInterval operationTimeout;

	private TimeInterval deltaServiceClientTimeout;

	private TimeInterval odataServicesOperationTimeout;

	private static ServerUserSettings instance;

	private static readonly object SingletonLock = new object();

	private string licensingServiceClientType;

	private readonly SettingsProvider settingsProvider;

	private readonly ConcurrentDictionary<string, AppSpecificServerSettings> appSpecificSettings;

	private readonly ConcurrentDictionary<string, Action<string, string>> dynamicSettingsUpdaters = new ConcurrentDictionary<string, Action<string, string>>(StringComparer.OrdinalIgnoreCase);

	private SimpleConfigSettingNoMaxValue<string> appProtocolName;

	private SimpleConfigSetting<Uri> azureActiveDirectoryClientApplicationAppIdUri;

	private SimpleConfigSetting<Guid> azureActiveDirctoryClientApplicationId;

	private LanguageSetting defaultLanguage;

	private SupportedServerLanguageSettings supportedServerLanguages;

	private SimpleConfigSetting<Uri> fixedTenantWebServiceBaseUri;

	private SimpleConfigSetting<Uri> fixedWebClientEndpointBaseUri;

	public static Func<string> GetCurrentAppId { get; set; }

	public static ServerUserSettings Instance
	{
		get
		{
			if (instance == null)
			{
				CreateInstance(string.Empty);
			}
			return instance;
		}
	}

	public static string SettingsFileName { get; private set; }

	public static string CommandLineServerInstance { get; set; }

	[ConfigSetting("ServerInstance", "DynamicsNAV90", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public InstanceNameSetting ServerInstance { get; set; }

	[ConfigSetting("NetworkProtocol", NetProtocolType.Default, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public EnumConfigSetting<NetProtocolType> NetworkProtocol { get; set; }

	[ConfigSetting("ClientServicesProtectionLevel", ProtectionLevel.EncryptAndSign, ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public EnumConfigSetting<ProtectionLevel> ClientServicesProtectionLevel { get; set; }

	[ConfigSetting("DatabaseServer", "", ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public DatabaseServer DatabaseServer { get; set; }

	[ConfigSetting("DatabaseInstance", "", ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public DatabaseInstance DatabaseInstance { get; set; }

	[ConfigSetting("EnableDeadlockMonitoring", false, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableDeadlockMonitoring { get; set; }

	[ConfigSetting("EnableSqlInformationDebugger", true, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableSqlInformationDebugger { get; set; }

	[ConfigSetting("AmountOfSqlStatementsInDebugger", 10, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public PositiveIntegerIncludingMaxValue AmountOfSqlStatementsInDebugger { get; set; }

	[ConfigSetting("EnableLongRunningSqlStatementsInDebugger", true, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableLongRunningSqlStatementsInDebugger { get; set; }

	[ConfigSetting("LongRunningSqlStatementsInDebuggerThreshold", 500, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public PositiveIntegerIncludingMaxValue LongRunningSqlStatementsInDebuggerThreshold { get; set; }

	[ConfigSetting("AllowSessionCallSuspendWhenWriteTransactionStarted", true, ConfigSettingAttribute.ConfigCategory.Compatibility, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> AllowSessionCallSuspendWhenWriteTransactionStarted { get; set; }

	[ConfigSetting("TrackSessionsInMemory", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> TrackSessionsInMemory { get; set; }

	[ConfigSetting("ReplaceReportExecutionTimeWithClientTime", true, ConfigSettingAttribute.ConfigCategory.Compatibility, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ReplaceReportExecutionTimeWithClientTime { get; set; }

	[ConfigSetting("UseFindMinusWhenPopulatingPage", true, ConfigSettingAttribute.ConfigCategory.Compatibility, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> UseFindMinusWhenPopulatingPage { get; set; }

	[ConfigSetting("EnableEventSourceExceptionLogging", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableEventSourceExceptionLogging { get; set; }

	[ConfigSetting("ServiceBusConnectionString", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Restricted, InternalUseOnly = true)]
	public ServiceBusConnectionString ServiceBusConnectionString { get; set; }

	[ConfigSetting("ServiceBusBatchFlushInterval", 200, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public PositiveIntegerIncludingMaxValue ServiceBusBatchFlushInterval { get; set; }

	[ConfigSetting("ForceSyncFromUpgradeCodeunit", false, ConfigSettingAttribute.ConfigCategory.Compatibility, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> ForceSyncFromUpgradeCodeunit { get; set; }

	[ConfigSetting("SqlConnectionKeepAliveInterval", "00:04:00", ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public TimeInterval SqlConnectionKeepAliveInterval { get; set; }

	public string ServerName { get; private set; }

	[ConfigSetting("DatabaseName", "", ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public DatabaseName DatabaseName { get; set; }

	[ConfigSetting("DatabaseUserName", "", ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<string> DatabaseUserName { get; set; }

	[ConfigSetting("ProtectedDatabasePassword", "", ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Denied, InternalUseOnly = true)]
	public ProtectedDatabasePassword ProtectedDatabasePassword { get; set; }

	[ConfigSetting("ClientServicesOperationTimeout", "MaxValue", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public TimeInterval ClientServicesOperationTimeout
	{
		get
		{
			return operationTimeout;
		}
		set
		{
			operationTimeout = ((value.Value == TimeSpan.Zero) ? TimeInterval.MaxValue : value);
		}
	}

	[ConfigSetting("EnableStaticAssemblies", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableStaticAssemblies { get; set; }

	[ConfigSetting("EnableThreadThrottling", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableThreadThrottling { get; set; }

	[ConfigSetting("EnablePrioritizedThreadThrottling", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnablePrioritizedThreadThrottling { get; set; }

	[ConfigSetting("CompactLargeObjectHeapInterval", "1.00:00:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public TimeInterval CompactLargeObjectHeapInterval { get; set; }

	[ConfigSetting("LargeObjectCompactionThresholdPercent", 100, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public SimpleConfigSetting<int> LargeObjectCompactionThresholdPercent { get; set; }

	[ConfigSetting("EnablePreScaledMedia", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnablePreScaledMedia { get; set; }

	[ConfigSetting("Multitenant", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> Multitenant { get; set; }

	[ConfigSetting("CompileBusinessApplicationAtStartup", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> CompileBusinessApplicationAtStartup { get; set; }

	[ConfigSetting("ReportPDFFontEmbedding", true, ConfigSettingAttribute.ConfigCategory.Reports, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ReportPdfFontEmbedding { get; set; }

	[ConfigSetting("ReportAppDomainIsolation", true, ConfigSettingAttribute.ConfigCategory.Reports, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ReportAppDomainIsolation { get; set; }

	[ConfigSetting("PreviousReportAppDomainIsolation", true, ConfigSettingAttribute.ConfigCategory.Reports, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> PreviousReportAppDomainIsolation { get; set; }

	[ConfigSetting("EnableDebugging", false, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableDebugging { get; set; }

	[ConfigSetting("DebuggingAllowed", true, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> DebuggingAllowed { get; set; }

	[ConfigSetting("DebuggingBaseAppAllowed", true, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> DebuggingBaseAppAllowed { get; set; }

	[ConfigSetting("MaxConcurrentCalls", 1000, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public PositiveIntegerIncludingMaxValue MaxConcurrentCalls { get; set; }

	[ConfigSetting("ReportAppDomainRecycleCount", 1000, ConfigSettingAttribute.ConfigCategory.Reports, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public PositiveIntegerIncludingMaxValue ReportAppDomainRecycleCount { get; set; }

	[ConfigSetting("LegacyReportAppDomainIsolation", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> LegacyReportAppDomainIsolation { get; set; }

	[ConfigSetting("ClientServicesMaxUploadSize", 350, ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public MaxMegabyteSize ClientServicesMaxUploadSize { get; set; }

	[ConfigSetting("ClientServicesReconnectPeriod", "00:10:00", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public TimeInterval ClientServicesReconnectPeriod { get; set; }

	[ConfigSetting("ClientServicesMaxItemsInObjectGraph", 512, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public MaxObjectGraphItems ClientServicesMaxItemsInObjectGraph { get; set; }

	[ConfigSetting("ODataMaxConnections", 0, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSetting<int> ODataMaxConnections { get; set; }

	[ConfigSetting("ODataMaxConnectionsPerTenant", 0, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSetting<int> ODataMaxConnectionsPerTenant { get; set; }

	[ConfigSetting("DeltaServiceClientTimeout", "00:02:00", ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public TimeInterval DeltaServiceClientTimeout
	{
		get
		{
			return deltaServiceClientTimeout;
		}
		set
		{
			deltaServiceClientTimeout = ((value.Value == TimeSpan.Zero) ? TimeInterval.MaxValue : value);
		}
	}

	[ConfigSetting("ODataServicesOperationTimeout", "MaxValue", ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public TimeInterval ODataServicesOperationTimeout
	{
		get
		{
			return odataServicesOperationTimeout;
		}
		set
		{
			odataServicesOperationTimeout = ((value.Value == TimeSpan.Zero) ? TimeInterval.MaxValue : value);
		}
	}

	[ConfigSetting("ServicePointManagerDefaultConnectionLimit", 1000, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<int> ServicePointManagerDefaultConnectionLimit { get; set; }

	[ConfigSetting("ServicePointManagerMaxServicePointIdleTime", 10000, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<int> ServicePointManagerMaxServicePointIdleTime { get; set; }

	[ConfigSetting("MaxStreamReadSize", 1000000, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public PositiveIntegerIncludingMaxValue MaxStreamReadSize { get; set; }

	[ConfigSetting("ClientServicesIdleClientTimeout", "MaxValue", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public TimeInterval ClientServicesIdleClientTimeout { get; set; }

	[ConfigSetting("ClientServicesKeepAliveInterval", "00:02:00", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public TimeInterval ClientServicesKeepAliveInterval { get; set; }

	[ConfigSetting("MetadataLockTimeout", "00:30:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true, InternalUseOnly = true)]
	public TimeInterval MetadataLockTimeout { get; set; }

	[ConfigSetting("OrphanedConnectionPurgePeriod", "00:01:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval OrphanedConnectionPurgePeriod { get; set; }

	[ConfigSetting("ClientServicesMaxNumberOfOrphanedConnections", 20, ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public PositiveIntegerIncludingMaxValue ClientServicesMaxNumberOfOrphanedConnections { get; set; }

	[ConfigSetting("ClientServicesMaxConcurrentConnections", 500, ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public PositiveIntegerIncludingMaxValue ClientServicesMaxConcurrentConnections { get; set; }

	[ConfigSetting("Binding", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<string> Binding { get; set; }

	[ConfigSetting("ManagementServicesPort", 7045, ConfigSettingAttribute.ConfigCategory.Management, ConfigSettings.EmitSetting.Allow)]
	public ServicePort ManagementServicesPort { get; set; }

	[ConfigSetting("ClientServicesPort", 7046, ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public ServicePort ClientServicesPort { get; set; }

	[ConfigSetting("SOAPServicesPort", 7047, ConfigSettingAttribute.ConfigCategory.WebServices, ConfigSettings.EmitSetting.Allow)]
	public ServicePort SOAPServicesPort { get; set; }

	[ConfigSetting("ODataServicesPort", 7048, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public ServicePort ODataServicesPort { get; set; }

	[ConfigSetting("DeveloperServicesPort", 7049, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow)]
	public ServicePort DeveloperServicesPort { get; set; }

	[ConfigSetting("ClientServicesEnabled", true, ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ClientServicesEnabled { get; set; }

	[ConfigSetting("SOAPServicesEnabled", true, ConfigSettingAttribute.ConfigCategory.WebServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> SOAPServicesEnabled { get; set; }

	[ConfigSetting("ODataServicesEnabled", true, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ODataServicesEnabled { get; set; }

	[ConfigSetting("ODataServicesV3EndpointEnabled", true, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ODataServicesV3EndpointEnabled { get; set; }

	[ConfigSetting("ODataServicesV4EndpointEnabled", true, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ODataServicesV4EndpointEnabled { get; set; }

	[ConfigSetting("ManagementServicesEnabled", true, ConfigSettingAttribute.ConfigCategory.Management, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ManagementServicesEnabled { get; set; }

	[ConfigSetting("DeveloperServicesEnabled", false, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> DeveloperServicesEnabled { get; set; }

	[ConfigSetting("ExtensionAllowedTargetLevel", "Solution", ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow)]
	public EnumConfigSetting<CompilationTarget> ExtensionAllowedTargetLevel { get; set; }

	[ConfigSetting("MSWalletPaymentsPublicKey", "{\"exponent\":\"\", \"modulus\":\"\"}", ConfigSettingAttribute.ConfigCategory.WebServices, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<string> MSWalletPaymentsPublicKey { get; set; }

	[ConfigSetting("MSWalletPaymentsTokenTimeout", "00:05:00", ConfigSettingAttribute.ConfigCategory.WebServices, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public TimeInterval MSWalletPaymentsTokenTimeout { get; set; }

	[ConfigSetting("SOAPServicesMaxMsgSize", 1024, ConfigSettingAttribute.ConfigCategory.WebServices, ConfigSettings.EmitSetting.Allow)]
	public WebServiceMaxMessageSize SOAPServicesMaxMsgSize { get; set; }

	[ConfigSetting("SOAPServicesSSLEnabled", false, ConfigSettingAttribute.ConfigCategory.WebServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> SOAPServicesSSLEnabled { get; set; }

	[ConfigSetting("ODataServicesSSLEnabled", false, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ODataServicesSSLEnabled { get; set; }

	[ConfigSetting("ODataEnableExcelAddInAnnotations", true, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ODataEnableExcelAddInAnnotations { get; set; }

	[ConfigSetting("PublicODataBaseUrl", "", ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<string> PublicODataBaseUrl { get; set; }

	[ConfigSetting("PublicSOAPBaseUrl", "", ConfigSettingAttribute.ConfigCategory.WebServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<string> PublicSOAPBaseUrl { get; set; }

	[ConfigSetting("DiscoveryServiceUri", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<string> DiscoveryServiceUri { get; set; }

	[ConfigSetting("FixedTenantWebServiceEndpointBaseUri", "", ConfigSettingAttribute.ConfigCategory.WebServices, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Uri> FixedTenantWebServiceEndpointBaseUri
	{
		get
		{
			return GetAppSpecificSettings(GetCurrentAppId?.Invoke())?.FixedTenantWebServiceEndpointBaseUri ?? fixedTenantWebServiceBaseUri;
		}
		set
		{
			if (GetAppSpecificSettings(GetCurrentAppId?.Invoke()) == null)
			{
				fixedTenantWebServiceBaseUri = value;
			}
		}
	}

	[ConfigSetting("FixedWebClientEndpointBaseUri", "", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Uri> FixedWebClientEndpointBaseUri
	{
		get
		{
			return GetAppSpecificSettings(GetCurrentAppId?.Invoke())?.FixedWebClientEndpointBaseUri ?? fixedWebClientEndpointBaseUri;
		}
		set
		{
			if (GetAppSpecificSettings(GetCurrentAppId?.Invoke()) == null)
			{
				fixedWebClientEndpointBaseUri = value;
			}
		}
	}

	[ConfigSetting("DeveloperServicesSSLEnabled", false, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> DeveloperServicesSSLEnabled { get; set; }

	[ConfigSetting("PublicWebBaseUrl", "", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<string> PublicWebBaseUrl { get; set; }

	[ConfigSetting("PublicWinBaseUrl", "", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<string> PublicWinBaseUrl { get; set; }

	[ConfigSetting("NavHttpClientMaxResponseContentSize", 150, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public MaxMegabyteSize NavHttpClientMaxResponseContentSize { get; set; }

	[ConfigSetting("NavHttpClientMaxTimeout", "00:05:00", ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public TimeInterval NavHttpClientMaxTimeout { get; set; }

	[ConfigSetting("DebuggerSignalRConnectionTimeout", "00:30:00", ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public TimeInterval DebuggerSignalRConnectionTimeout { get; set; }

	[ConfigSetting("SqlConnectionIdleTimeout", "00:05:00", ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public TimeInterval SqlConnectionIdleTimeout { get; set; }

	[ConfigSetting("MaxNumberOfCachedCommands", 500, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<int> MaxNumberOfCachedSqlCommands { get; set; }

	[ConfigSetting("MaxNumberOfCachedSqlCommandsSharedSchema", 3000, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<int> MaxNumberOfCachedSqlCommandsSharedSchema { get; set; }

	[ConfigSetting("DebuggerSignalRDisconnectTimeout", "00:30:00", ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public TimeInterval DebuggerSignalRDisconnectTimeout { get; set; }

	[ConfigSetting("DebuggerSignalRKeepAliveTimeout", "00:10:00", ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public TimeInterval DebuggerSignalRKeepAliveTimeout { get; set; }

	[ConfigSetting("DebuggedClientKeepAliveInterval", "00:30:00", ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public TimeInterval DebuggedClientKeepAliveInterval { get; set; }

	[ConfigSetting("AppProtocolName", "ms-dynamicsnav", ConfigSettingAttribute.ConfigCategory.WebServices, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<string> AppProtocolName
	{
		get
		{
			return GetAppSpecificSettings(GetCurrentAppId?.Invoke())?.AppProtocolName ?? appProtocolName;
		}
		set
		{
			if (GetAppSpecificSettings(GetCurrentAppId?.Invoke()) == null)
			{
				appProtocolName = value;
			}
		}
	}

	[ConfigSetting("AzureActiveDirectoryClientApplicationId", "00000000-0000-0000-0000-000000000000", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Guid> AzureActiveDirectoryClientApplicationId
	{
		get
		{
			return GetAppSpecificSettings(GetCurrentAppId?.Invoke())?.AzureActiveDirectoryClientApplicationId ?? azureActiveDirctoryClientApplicationId;
		}
		set
		{
			if (GetAppSpecificSettings(GetCurrentAppId?.Invoke()) == null)
			{
				azureActiveDirctoryClientApplicationId = value;
			}
		}
	}

	[ConfigSetting("AzureActiveDirectoryMaxUserLicenseCacheTime", "00:15:00", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval AzureActiveDirectoryMaxUserLicenseCacheTime { get; set; }

	[ConfigSetting("AzureActiveDirectoryIdleUserLicenseCacheTime", "00:02:00", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval AzureActiveDirectoryIdleUserLicenseCacheTime { get; set; }

	[ConfigSetting("AuthenticationCacheExpirationTime", "00:15:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval AuthenticationCacheExpirationTime { get; set; }

	[ConfigSetting("MicrosoftGraphSharedContactsMailboxName", "SystemMailbox{D0E409A0-AF9B-4720-92FE-AAC869B0D201}", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<string> MicrosoftGraphSharedContactsMailboxName { get; set; }

	[ConfigSetting("DefaultClient", "Windows", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<NavClientType> DefaultClient { get; set; }

	[ConfigSetting("ServicesOptionFormat", Microsoft.Dynamics.Nav.Types.ServicesOptionFormat.OptionCaption, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public EnumConfigSetting<ServicesOptionFormat> ServicesOptionFormat { get; set; }

	[ConfigSetting("ServicesLanguage", "en-US", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public LanguageSetting ServicesLanguage { get; set; }

	[ConfigSetting("CalendarTwoDigitYearMax", -1, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<int> CalendarTwoDigitYearMax { get; set; }

	[ConfigSetting("DefaultLanguage", "en-US", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public LanguageSetting DefaultLanguage
	{
		get
		{
			return GetAppSpecificSettings(GetCurrentAppId?.Invoke())?.DefaultLanguage ?? defaultLanguage;
		}
		set
		{
			defaultLanguage = value;
		}
	}

	[ConfigSetting("SupportedLanguages", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SupportedServerLanguageSettings SupportedLanguages
	{
		get
		{
			return GetAppSpecificSettings(GetCurrentAppId?.Invoke())?.SupportedLanguages ?? supportedServerLanguages;
		}
		set
		{
			supportedServerLanguages = value;
		}
	}

	[ConfigSetting("ServicesUseNTLMAuthentication", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ServicesUseNTLMAuthentication { get; set; }

	[ConfigSetting("ServicesDefaultTimeZone", "UTC", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public ServicesTimeZone ServicesDefaultTimeZone { get; set; }

	[ConfigSetting("CloudReadyReplicationTableCacheTimeout", "00:05:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval CloudReadyReplicationTableCacheTimeout { get; set; }

	[ConfigSetting("ServicesDefaultCompany", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<string> ServicesDefaultCompany { get; set; }

	[ConfigSetting("ClientServicesProhibitedFileTypes", "ade;adp;asp;bas;bat;chm;cmd;com;cpl;csh;dll;exe;fxp;gadget;hlp;hta;inf;ins;isp;its;js;jse;ksh;lnk;mad;maf;mag;mam;maq;mar;mas;mat;mau;mav;maw;mda;mdb;mde;mdt;mdw;mdz;msc;msi;msp;mst;ops;pcd;pif;prf;prg;pst;reg;scf;scr;sct;shb;shs;url;vb;vbe;vbs;vsmacros;vss;vst;vsw;ws;wsc;wsf;wsh", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public FileTypeFilter ClientServicesProhibitedFileTypes { get; set; }

	[ConfigSetting("ClientServicesAllowedFileTypes", "", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public FileTypeFilter ClientServicesAllowedFileTypes { get; set; }

	[ConfigSetting("ClientServicesCompressionThreshold", 64, ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public CompressionThreshold ClientServicesCompressionThreshold { get; set; }

	[ConfigSetting("ClientServicesChunkSize", 28, ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public ChunkSize ClientServicesChunkSize { get; set; }

	[ConfigSetting("ODataServicesMaxPageSize", 20000, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public PositiveIntegerIncludingMaxValue ODataServicesMaxPageSize { get; set; }

	[ConfigSetting("UseQueryForFind", false, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> UseQueryForFind { get; set; }

	[ConfigSetting("BufferedInsertEnabled", true, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> BufferedInsertEnabled { get; set; }

	[ConfigSetting("SqlParametersByOrdinal", true, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> SqlParametersByOrdinal { get; set; }

	[ConfigSetting("SqlPartitionsCount", 1, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<int> SqlPartitionsCount { get; set; }

	[ConfigSetting("SqlPartitionBucketSize", 200, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<int> SqlPartitionBucketSize { get; set; }

	[ConfigSetting("DisableSmartSql", false, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<bool> DisableSmartSql { get; set; }

	[ConfigSetting("ClientServicesCredentialType", NavClientCredentialType.Windows, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public EnumConfigSetting<NavClientCredentialType> ClientServicesCredentialType { get; set; }

	[ConfigSetting("ClientServicesTokenSigningKey", "", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Denied)]
	public SwtTokenSigningKey ClientServicesTokenSigningKey { get; set; }

	[ConfigSetting("ServicesCertificateThumbprint", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public ClientServicesCertificateThumbprint ServicesCertificateThumbprint { get; set; }

	[ConfigSetting("ServicesCertificateValidationEnabled", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ServicesCertificateValidationEnabled { get; set; }

	[ConfigSetting("DisableTokenSigningCertificateValidation", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> DisableTokenSigningCertificateValidation { get; set; }

	[ConfigSetting("TokenSigningCertificateValidationMode", CertificateValidationMode.IssuerNameValidation, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public EnumConfigSetting<CertificateValidationMode> TokenSigningCertificateValidationMode { get; set; }

	[ConfigSetting("CheckCertificateRevocationList", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> CheckCertificateRevocationList { get; set; }

	[ConfigSetting("NASServicesStartupCodeunit", 0, ConfigSettingAttribute.ConfigCategory.NASService, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<int> NASServicesStartupCodeunit { get; set; }

	[ConfigSetting("NASServicesStartupMethod", "", ConfigSettingAttribute.ConfigCategory.NASService, ConfigSettings.EmitSetting.Allow)]
	public ALMethodNameSetting NASServicesStartupMethod { get; set; }

	[ConfigSetting("NASServicesStartupArgument", "", ConfigSettingAttribute.ConfigCategory.NASService, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<string> NASServicesStartupArgument { get; set; }

	[ConfigSetting("NASServicesEnableDebugging", false, ConfigSettingAttribute.ConfigCategory.NASService, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> NASServicesEnableDebugging { get; set; }

	[ConfigSetting("NASServicesRunWithAdminRights", false, ConfigSettingAttribute.ConfigCategory.NASService, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> NASServicesRunWithAdminRights { get; set; }

	[ConfigSetting("SqlConnectionTimeout", "00:01:30", ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public TimeInterval SqlConnectionTimeout { get; set; }

	[ConfigSetting("SqlLongRunningThreshold", 1000, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public PositiveIntegerIncludingMaxValue SqlLongRunningThreshold { get; set; }

	[ConfigSetting("SqlBulkImportBatchSize", 448, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public NonnegativeIntegerIncludingMaxValue SqlBulkImportBatchSize { get; set; }

	[ConfigSetting("EnableSqlConnectionEncryption", false, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableSqlConnectionEncryption { get; set; }

	[ConfigSetting("TrustSqlServerCertificate", false, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> TrustSqlServerCertificate { get; set; }

	[ConfigSetting("SqlCommandTimeout", "00:30:00", ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public TimeInterval SqlCommandTimeout { get; set; }

	[ConfigSetting("SqlLockTimeoutOverride", 0, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public SqlLockTimeout SqlLockTimeoutOverride { get; set; }

	[ConfigSetting("SessionEventTablePurgeLookupPeriod", "1.00:00:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval SessionEventTablePurgeLookupPeriod { get; set; }

	[ConfigSetting("SessionEventTableRetainPeriod", 0, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public NonnegativeIntegerIncludingMaxValue SessionEventTableRetainPeriod { get; set; }

	[ConfigSetting("SessionEventTableRetainInterval", "90.00:00:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public TimeInterval SessionEventTableRetainInterval { get; set; }

	[ConfigSetting("NonInteractiveSessionsLogRetainPeriod", 0, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public NonnegativeIntegerIncludingMaxValue NonInteractiveSessionsLogRetainPeriod { get; set; }

	[ConfigSetting("NonInteractiveSessionsLogRetainInterval", "5.00:00:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public TimeInterval NonInteractiveSessionsLogRetainInterval { get; set; }

	public int SessionEventTableRetainPeriodInSeconds
	{
		get
		{
			if (SessionEventTableRetainPeriod.Value != 0)
			{
				return SessionEventTableRetainPeriod.Value * 30 * 24 * 60 * 60;
			}
			return SessionEventTableRetainInterval.TotalSeconds;
		}
	}

	public int NonInteractiveSessionsLogRetainPeriodInSeconds
	{
		get
		{
			if (NonInteractiveSessionsLogRetainPeriod.Value != 0)
			{
				return NonInteractiveSessionsLogRetainPeriod.Value * 24 * 60 * 60;
			}
			return NonInteractiveSessionsLogRetainInterval.TotalSeconds;
		}
	}

	[ConfigSetting("DataCacheSize", 10, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public DataCacheSize DataCacheSize { get; set; }

	[ConfigSetting("CacheSynchronizationPeriod", "00:00:30", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval CacheSynchronizationPeriod { get; set; }

	[ConfigSetting("HeartbeatPeriod", "00:00:30", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval HeartbeatPeriod { get; set; }

	[ConfigSetting("HealthMonitorPeriod", "00:00:30", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval HealthMonitorPeriod { get; set; }

	[ConfigSetting("MaxStaleTenantConfigPeriod", "00:05:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval MaxStaleTenantConfigPeriod { get; set; }

	[ConfigSetting("HealthMonitorDelayBeforeServerShutdown", "00:05:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval HealthMonitorDelayBeforeServerShutdown { get; set; }

	[ConfigSetting("TenantStateReportingPeriod", "00:05:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval TenantStateReportingPeriod { get; set; }

	[ConfigSetting("StateSynchronizationTimeout", "00:10:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval StateSynchronizationTimeout { get; set; }

	[ConfigSetting("StateSynchronizationAdditionalConfigurationVersionRefreshPeriod", "00:00:02", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval StateSynchronizationAdditionalConfigurationVersionRefreshPeriod { get; set; }

	[ConfigSetting("EnableTaskScheduler", true, ConfigSettingAttribute.ConfigCategory.TaskScheduler, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableTaskScheduler { get; set; }

	[ConfigSetting("TaskSchedulerPeriod", "00:00:02", ConfigSettingAttribute.ConfigCategory.TaskScheduler, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval TaskSchedulerPeriod { get; set; }

	[ConfigSetting("TaskSchedulerMaximumConcurrentRunningTasks", 10, ConfigSettingAttribute.ConfigCategory.TaskScheduler, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public PositiveIntegerIncludingMaxValue TaskSchedulerMaxConcurrentRunningTasks { get; set; }

	[ConfigSetting("TaskSchedulerSystemTaskStartTime", "00:00:00", ConfigSettingAttribute.ConfigCategory.TaskScheduler, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public TimeInterval TaskSchedulerSystemTaskStartTime { get; set; }

	[ConfigSetting("TaskSchedulerSystemTaskEndTime", "23:59:59", ConfigSettingAttribute.ConfigCategory.TaskScheduler, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public TimeInterval TaskSchedulerSystemTaskEndTime { get; set; }

	[ConfigSetting("ResultSetGroupCacheUsesGlobalCache", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> ResultSetGroupCacheUsesGlobalCache { get; set; }

	[ConfigSetting("UseCalculatedFieldsCache", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> UseCalculatedFieldsCache { get; set; }

	[ConfigSetting("UseResultSetCache", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> UseResultSetCache { get; set; }

	[ConfigSetting("EnableTempTableSizeReporting", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableTempTableSizeReporting { get; set; }

	[ConfigSetting("UseCurrentKeyForIsEmptyOnTempTables", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> UseCurrentKeyForIsEmptyOnTempTables { get; set; }

	[ConfigSetting("DocumentServicesProvider", "SHAREPOINTONLINE", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<string> DocumentServicesProvider { get; set; }

	[ConfigSetting("UIElementRemovalOption", ElementRemovalOption.LicenseFileAndUserPermissions, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public EnumConfigSetting<ElementRemovalOption> UIElementRemovalOption { get; set; }

	[ConfigSetting("EnableFullALFunctionTracing", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableFullALFunctionTracing { get; set; }

	[ConfigSetting("EnableALServerFileAccess", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableALServerFileAccess { get; set; }

	[ConfigSetting("ServerFileCacheDirectory", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<string> ServerFileCacheDirectory { get; set; }

	[ConfigSetting("ClientBuildRestriction", SameBuildRestriction.WarnClient, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public EnumConfigSetting<SameBuildRestriction> ClientBuildRestriction { get; set; }

	[ConfigSetting("EnableApplicationChannelLog", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableApplicationChannelLog { get; set; }

	[ConfigSetting("DisableDataCache", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> DisableDataCache { get; set; }

	[ConfigSetting("EnableServiceBusCacheSync", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableServiceBusCacheSync { get; set; }

	[ConfigSetting("ServiceBusMessageCountThreshold", 1000, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public PositiveIntegerIncludingMaxValue ServiceBusMessageCountThreshold { get; set; }

	[ConfigSetting("WildcardEvaluationTimeout", "00:00:05", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval WildcardEvaluationTimeout { get; set; }

	[ConfigSetting("EnableODataStrictFiltering", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableODataStrictFiltering { get; set; }

	[ConfigSetting("DisableMultipleTenantSqlConnection", false, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> DisableMultipleTenantSqlConnection { get; set; }

	[ConfigSetting("DisableAzureLicenseLogging", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> DisableAzureLicenseLogging { get; set; }

	[ConfigSetting("AzureLicenseLoggingLogMicrosoftAccount", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> AzureLicenseLoggingLogMicrosoftAccount { get; set; }

	[ConfigSetting("TraceLevel", Verbosity.Normal, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public EnumConfigSetting<Verbosity> TraceLevel { get; set; }

	[ConfigSetting("ExternalTraceLevel", SourceLevels.Error, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public EnumConfigSetting<SourceLevels> ExternalTraceLevel { get; set; }

	[ConfigSetting("IfxTraceLevel", Verbosity.Off, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public EnumConfigSetting<Verbosity> IfxMaximumTraceLevel { get; set; }

	[ConfigSetting("MaximumTraceLevelOverrideDuration", "01:00:00", ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public TimeInterval MaximumTraceLevelOverrideDuration { get; set; }

	[ConfigSetting("IfxSessionName", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public SimpleConfigSetting<string> IfxSessionName { get; set; }

	[ConfigSetting("MdmAccountName", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public SimpleConfigSetting<string> MdmAccountName { get; set; }

	[ConfigSetting("EncryptionProvider", Microsoft.Dynamics.Nav.Types.EncryptionProvider.LocalKeyFile, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public EnumConfigSetting<EncryptionProvider> EncryptionProvider { get; set; }

	[ConfigSetting("AzureKeyVaultClientId", "00000000-0000-0000-0000-000000000000", ConfigSettingAttribute.ConfigCategory.AzureKeyVault, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<Guid> AzureKeyVaultClientId { get; set; }

	[ConfigSetting("AzureKeyVaultClientCertificateStoreLocation", StoreLocation.LocalMachine, ConfigSettingAttribute.ConfigCategory.AzureKeyVault, ConfigSettings.EmitSetting.Allow)]
	public EnumConfigSetting<StoreLocation> AzureKeyVaultClientCertificateStoreLocation { get; set; }

	[ConfigSetting("AzureKeyVaultClientCertificateStoreName", StoreName.My, ConfigSettingAttribute.ConfigCategory.AzureKeyVault, ConfigSettings.EmitSetting.Allow)]
	public EnumConfigSetting<StoreName> AzureKeyVaultClientCertificateStoreName { get; set; }

	[ConfigSetting("AzureKeyVaultClientCertificateThumbprint", "", ConfigSettingAttribute.ConfigCategory.AzureKeyVault, ConfigSettings.EmitSetting.Allow)]
	public ClientServicesCertificateThumbprint AzureKeyVaultClientCertificateThumbprint { get; set; }

	[ConfigSetting("AzureKeyVaultKeyUri", "", ConfigSettingAttribute.ConfigCategory.AzureKeyVault, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<Uri> AzureKeyVaultKeyUri { get; set; }

	[ConfigSetting("ExchangeAuthenticationMetadataLocation", "https://outlook.office365.com/", ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<string> ExchangeAuthenticationMetadataLocation { get; set; }

	[ConfigSetting("ClientServicesFederationMetadataLocation", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public DecodedUriWithPlaceholderSetting WSFederationMetadataLocation { get; set; }

	[ConfigSetting("WSFederationLoginEndpoint", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public DecodedUriWithPlaceholderSetting WSFederationLoginEndpoint { get; set; }

	[ConfigSetting("AzureActiveDirectoryClientId", "00000000-0000-0000-0000-000000000000", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<Guid> AzureActiveDirectoryClientId { get; set; }

	[ConfigSetting("AzureActiveDirectoryClientSecret", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<string> AzureActiveDirectoryClientSecret { get; set; }

	[ConfigSetting("AzureActiveDirectoryClientCertificateThumbprint", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public ClientServicesCertificateThumbprint AzureActiveDirectoryClientCertificateThumbprint { get; set; }

	[ConfigSetting("AppIdUri", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<Uri> AppIdUri
	{
		get
		{
			return GetAppSpecificSettings(GetCurrentAppId?.Invoke())?.AppIdUri ?? azureActiveDirectoryClientApplicationAppIdUri;
		}
		set
		{
			if (GetAppSpecificSettings(GetCurrentAppId?.Invoke()) == null)
			{
				azureActiveDirectoryClientApplicationAppIdUri = value;
			}
		}
	}

	[ConfigSetting("ValidAudiences", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public ValidAudiences ValidAudiences { get; set; }

	[ConfigSetting("TokenAuthorityEndpoint", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Uri> TokenAuthorityEndpoint { get; set; }

	[ConfigSetting("GraphApiEndpoint", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Uri> GraphApiEndpoint { get; set; }

	[ConfigSetting("TrustedServerToServerApplicationIds", "00000003-0000-0000-c000-000000000000", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<string> TrustedServerToServerApplicationIds { get; set; }

	[ConfigSetting("OpenIdConfigurationEndpoint", "https://login.windows.net/common/.well-known/openid-configuration", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<string> OpenIdConfigurationEndpoint { get; set; }

	[ConfigSetting("TrustedServerToServerTokenIssuers", "https://sts.windows.net", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<string> TrustedServerToServerTokenIssuers { get; set; }

	[ConfigSetting("DisableWriteInsideTryFunctions", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> DisableWriteInsideTryFunctions { get; set; }

	[ConfigSetting("EnableMembershipEntitlement", false, ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableMembershipEntitlement { get; set; }

	[ConfigSetting("EnableInvoiceEntitlementCheck", true, ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableInvoiceEntitlementCheck { get; set; }

	[ConfigSetting("OfficePortalEndpoint", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Uri> OfficePortalEndpoint { get; set; }

	[ConfigSetting("InvoiceAccessControlEndpoint", "{OFFICEPORTAL}/admin/api/users/{UPN}/accesscontrol/Invoice", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<string> InvoiceAccessControlEndpoint { get; set; }

	[ConfigSetting("DefaultApplicationId", "NAV", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	[ValidValueSet(new string[] { "NAV", "INV", "FIN", "C5", "ACC" })]
	public SimpleConfigSettingNoMaxValue<string> DefaultApplicationId { get; set; }

	[ConfigSetting("IsPreviewRelease", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> IsPreviewRelease { get; set; }

	[ConfigSetting("MaxRowsToExportToExcel", "MaxValue", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public PositiveIntegerIncludingMaxValue MaxRowsToExportToExcel { get; set; }

	[ConfigSetting("EnableDataExportImport", true, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableDataExportImport { get; set; }

	[ConfigSetting("TenantDetectionMaxRetryCount", 12, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TenantPollLimits TenantDetectionMaxRetryCount { get; set; }

	[ConfigSetting("TenantMaxPollCountPerCycle", 10, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TenantPollLimits TenantMaxPollCountPerCycle { get; set; }

	[ConfigSetting("EnableSaveToWordForRdlcReports", true, ConfigSettingAttribute.ConfigCategory.Reports, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableSaveToWordForRdlcReports { get; set; }

	[ConfigSetting("EnableSaveToExcelForRdlcReports", true, ConfigSettingAttribute.ConfigCategory.Reports, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableSaveToExcelForRdlcReports { get; set; }

	[ConfigSetting("EnableSaveFromReportPreview", true, ConfigSettingAttribute.ConfigCategory.Reports, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableSaveFromReportPreview { get; set; }

	[ConfigSetting("CalculateBestPaperSizeForReportPrinting", true, ConfigSettingAttribute.ConfigCategory.Reports, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> CalculateBestPaperSizeForReportPrinting { get; set; }

	[ConfigSetting("FeatureSwitchOverrides", "EnableStreamingReportDataSet", ConfigSettingAttribute.ConfigCategory.FeatureSwitches, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<string> FeatureSwitchOverrides { get; set; }

	[ConfigSetting("UseSimplifiedFilters", false, ConfigSettingAttribute.ConfigCategory.ClientServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> UseSimplifiedFilters { get; set; }

	[ConfigSetting("ExtensionsUsageReportingInterval", "00:15:00", ConfigSettingAttribute.ConfigCategory.Extensions, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public TimeInterval ExtensionsUsageReportingInterval { get; set; }

	[ConfigSetting("ExcelAddInAzureActiveDirectoryClientId", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<string> ExcelAddInAzureActiveDirectoryClientId { get; set; }

	[ConfigSetting("MicrosoftGraphEndpoint", "https://outlook.office365.com/api/beta", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Uri> MicrosoftGraphEndpoint { get; set; }

	[ConfigSetting("WebhooksAdapterUri", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Uri> WebhooksAdapterUri { get; set; }

	[ConfigSetting("ExtendedSecurityTokenLifetime", 0, ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow)]
	public ExtendedTokenLifetime ExtendedSecurityTokenLifetime { get; set; }

	[ConfigSetting("EnableSaasExtensionInstall", false, ConfigSettingAttribute.ConfigCategory.Extensions, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableSaasExtensionInstall { get; set; }

	[ConfigSetting("ApiServicesEnabled", false, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ApiServicesEnabled { get; set; }

	[ConfigSetting("ApiSubscriptionsEnabled", true, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> ApiSubscriptionsEnabled { get; set; }

	[ConfigSetting("ApiSubscriptionExpiration", 3, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<double> ApiSubscriptionExpiration { get; set; }

	[ConfigSetting("ApiSubscriptionNotificationUrlTimeout", 5000, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<int> ApiSubscriptionNotificationUrlTimeout { get; set; }

	[ConfigSetting("ApiSubscriptionSendingNotificationTimeout", 30000, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<int> ApiSubscriptionSendingNotificationTimeout { get; set; }

	[ConfigSetting("ApiSubscriptionDelayTime", 30000, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<int> ApiSubscriptionDelayTime { get; set; }

	[ConfigSetting("ApiSubscriptionMaxNumberOfNotifications", 100, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<int> ApiSubscriptionMaxNumberOfNotifications { get; set; }

	[ConfigSetting("ApiSubscriptionMaxNumberOfSubscriptions", 200, ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<int> ApiSubscriptionMaxNumberOfSubscriptions { get; set; }

	[ConfigSetting("SecurityProtocol", "Tls12", ConfigSettingAttribute.ConfigCategory.Compatibility, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<SecurityProtocolType> SecurityProtocol { get; set; }

	[ConfigSetting("DisableSharedSqlCommandCacheAcrossTenant", false, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> DisableSharedSqlCommandCacheAcrossTenant { get; set; }

	[ConfigSetting("UseIncrementalCompanyDelete", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<bool> UseIncrementalCompanyDelete { get; set; }

	[ConfigSetting("IncrementalCompanyDeleteLockTimeout", "00:01:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true, InternalUseOnly = true)]
	public TimeInterval IncrementalCompanyDeleteLockTimeout { get; set; }

	[ConfigSetting("PublishPackageIdLockTimeout", "00:30:00", ConfigSettingAttribute.ConfigCategory.Extensions, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true, InternalUseOnly = true)]
	public TimeInterval PublishPackageIdLockTimeout { get; set; }

	[ConfigSetting("TenantEnvironmentType", NavTenantEnvironmentType.Production, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public EnumConfigSetting<NavTenantEnvironmentType> TenantEnvironmentType { get; set; }

	[ConfigSetting("EnableSymbolLoadingAtServerStartup", false, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> EnableSymbolLoadingAtServerStartup { get; set; }

	[ConfigSetting("LicensingServiceUri", "", ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Uri> LicensingServiceUri { get; set; }

	public string LicensingServiceClientType
	{
		get
		{
			if (licensingServiceClientType == null)
			{
				licensingServiceClientType = (EnableLicensingService.Value ? "ServiceFabricService" : "WindowsService");
			}
			return licensingServiceClientType;
		}
	}

	[ConfigSetting("EnableLicensingService", false, ConfigSettingAttribute.ConfigCategory.AzureActiveDirectory, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableLicensingService { get; set; }

	[ConfigSetting("XmlMetadataCacheSize", 500, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public PositiveIntegerIncludingMaxValue XmlMetadataCacheSize { get; set; }

	[ConfigSetting("DeltaServiceEndpoint", "", ConfigSettingAttribute.ConfigCategory.ODataServices, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<Uri> DeltaServiceEndpoint { get; set; }

	[ConfigSetting("LocalServiceApiKey", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Denied, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<string> LocalServiceApiKey { get; set; }

	[ConfigSetting("AllowSessionWhileSyncAndDataUpgrade", false, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<bool> AllowSessionWhileSyncAndDataUpgrade { get; set; }

	[ConfigSetting("ExternalNavAppDeploymentEndpoint", "", ConfigSettingAttribute.ConfigCategory.Extensions, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Uri> ExternalNavAppDeploymentEndpoint { get; set; }

	[ConfigSetting("ExternalNavAppDeploymentRequestTimeout", "00:03:00", ConfigSettingAttribute.ConfigCategory.Extensions, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true, InternalUseOnly = true)]
	public TimeInterval ExternalNavAppDeploymentRequestTimeout { get; set; }

	[ConfigSetting("ReportSettingsLookupForm", 1560, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<int> ReportSettingsLookupForm { get; set; }

	[ConfigSetting("SearchTimeout", "00:00:10", ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public TimeInterval SearchTimeout { get; set; }

	[ConfigSetting("DisableQueryHintOptimizeForUnknown", false, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<bool> DisableQueryHintOptimizeForUnknown { get; set; }

	[ConfigSetting("DisableQueryHintForceOrder", true, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<bool> DisableQueryHintForceOrder { get; set; }

	[ConfigSetting("DisableQueryHintLoopJoin", true, ConfigSettingAttribute.ConfigCategory.Database, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<bool> DisableQueryHintLoopJoin { get; set; }

	[ConfigSetting("OverwriteExistingTranslations", true, ConfigSettingAttribute.ConfigCategory.Extensions, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<bool> OverwriteExistingTranslations { get; set; }

	[ConfigSetting("HybridDeploymentProxyEndpoint", "", ConfigSettingAttribute.ConfigCategory.HybridDeploy, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<Uri> HybridDeploymentProxyEndpoint { get; set; }

	[ConfigSetting("HybridDeploymentProxyRequestTimeout", "00:03:00", ConfigSettingAttribute.ConfigCategory.HybridDeploy, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true, InternalUseOnly = true)]
	public TimeInterval HybridDeploymentProxyRequestTimeout { get; set; }

	[ConfigSetting("ProductNames_ProductName", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<string> ProductNamesProductName { get; set; }

	[ConfigSetting("ProductNames_ProductNameShort", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<string> ProductNamesProductNameShort { get; set; }

	[ConfigSetting("ProductNames_MarketingName", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<string> ProductNamesMarketingName { get; set; }

	[ConfigSetting("ReverseProxyCertificateThumbprint", "", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public ClientServicesCertificateThumbprint ReverseProxyCertificateThumbprint { get; set; }

	[ConfigSetting("ManagementServicesIdleClientTimeout", "02:00:00", ConfigSettingAttribute.ConfigCategory.Management, ConfigSettings.EmitSetting.Allow, InternalUseOnly = false)]
	public TimeInterval ManagementServicesIdleClientTimeout { get; set; }

	[ConfigSetting("ALUpdateDialogThreshold", 50, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public PositiveIntegerIncludingMaxValue ALUpdateDialogThreshold { get; set; }

	[ConfigSetting("TableSyncTimeout", "00:30:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true, InternalUseOnly = true)]
	public TimeInterval TableSyncTimeout { get; set; }

	[ConfigSetting("NavAppMetadataRetrieverTimeout", "00:30:00", ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true, InternalUseOnly = true)]
	public TimeInterval NavAppMetadataRetrieverTimeout { get; set; }

	public Uri AppIdUriOrFederationLoginEndpointRealm
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(AppIdUri.Value.ToString()))
			{
				return AppIdUri.Value;
			}
			if (string.IsNullOrWhiteSpace(WSFederationLoginEndpoint.Value.ToString()))
			{
				return null;
			}
			return SimpleConfigSettingNoMaxValue<Uri>.Parse(AadHelper.FindRealm(WSFederationLoginEndpoint.Value), "WSFederationLoginEndpoint.wtrealm").Value;
		}
	}

	[ConfigSetting("CacheUserDefinedWebServices", true, ConfigSettingAttribute.ConfigCategory.WebServices, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> CacheUserDefinedWebServices { get; set; }

	[ConfigSetting("LockoutPolicyFailedAuthenticationCount", 0, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<int> LockoutPolicyFailedAuthenticationCount { get; set; }

	[ConfigSetting("LockoutPolicyFailedAuthenticationWindow", 0, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSettingNoMaxValue<int> LockoutPolicyFailedAuthenticationWindow { get; set; }

	[ConfigSetting("EnableV2MissingHashFixerOnStartup", true, ConfigSettingAttribute.ConfigCategory.Extensions, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableV2MissingHashFixerOnStartup { get; set; }

	[ConfigSetting("EnableUserConsistencyValidationOnTasks", true, ConfigSettingAttribute.ConfigCategory.TaskScheduler, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableUserConsistencyValidationOnTasks { get; set; }

	[ConfigSetting("NavAppPackageBlobBatchReadSize", 25, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true, InternalUseOnly = true)]
	public NonnegativeIntegerIncludingMaxValue NavAppPackageBlobBatchReadSize { get; set; }

	[ConfigSetting("MaxNumberOfCachedOptionCaptions", 10, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSettingNoMaxValue<int> MaxNumberOfCachedOptionCaptions { get; set; }

	[ConfigSetting("EnableMultithreadedCompilation", true, ConfigSettingAttribute.ConfigCategory.Development, ConfigSettings.EmitSetting.Allow, DynamicallyUpdatable = true)]
	public SimpleConfigSettingNoMaxValue<bool> EnableMultithreadedCompilation { get; set; }

	[ConfigSetting("LegacyOptionCaptionResolution", false, ConfigSettingAttribute.ConfigCategory.Compatibility, ConfigSettings.EmitSetting.Allow)]
	public SimpleConfigSetting<bool> LegacyOptionCaptionResolution { get; set; }

	[ConfigSetting("ProtectNavAppSourceFiles", true, ConfigSettingAttribute.ConfigCategory.Extensions, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true, DynamicallyUpdatable = true)]
	public SimpleConfigSetting<bool> ProtectNavAppSourceFiles { get; set; }

	[ConfigSetting("CacheActiveExternalUsersForWebServiceRequests", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<bool> CacheActiveExternalUsersForWebServiceRequests { get; set; }

	[ConfigSetting("CacheActiveDeviceOnlyUsersForWebServiceRequests", true, ConfigSettingAttribute.ConfigCategory.General, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<bool> CacheActiveDeviceOnlyUsersForWebServiceRequests { get; set; }

	[ConfigSetting("EnableRadPublishing", true, ConfigSettingAttribute.ConfigCategory.Extensions, ConfigSettings.EmitSetting.Allow, InternalUseOnly = true)]
	public SimpleConfigSetting<bool> EnableRadPublishing { get; set; }

	public string AadTenantId
	{
		get
		{
			if (string.IsNullOrWhiteSpace(WSFederationLoginEndpoint.Value.ToString()))
			{
				return "common";
			}
			return AadHelper.FindTenantId(WSFederationLoginEndpoint.Value);
		}
	}

	public static event EventHandler<SettingsChangedEventArgs> SettingsChanged;

	public static void CreateDefaultInstance()
	{
		CreateInstance((SettingsProvider)null);
	}

	public static void CreateInstance(SettingsProvider settingsProvider)
	{
		CreateInstance(settingsProvider, AppSpecificServerSettingsProvider.GetAppSpecificSettings(settingsProvider));
	}

	public static void CreateInstance(string fileName)
	{
		CreateInstance(new ConfigurationSettingsProvider(fileName));
	}

	private static void CreateInstance(SettingsProvider settingsProvider, ConcurrentDictionary<string, AppSpecificServerSettings> appSpecificSettings)
	{
		if (instance != null)
		{
			return;
		}
		lock (SingletonLock)
		{
			if (instance != null)
			{
				return;
			}
			instance = new ServerUserSettings(settingsProvider, appSpecificSettings);
			instance.LoadAndValidateSettings();
			if (string.IsNullOrEmpty(instance.FeatureSwitchOverrides.Value))
			{
				string environmentVariable = Environment.GetEnvironmentVariable("NAV_INTERNAL_FEATURESWITCHOVERRIDES");
				if (!string.IsNullOrEmpty(environmentVariable))
				{
					instance.FeatureSwitchOverrides = new SimpleConfigSetting<string>(environmentVariable);
				}
			}
		}
	}

	public static void LogServerStartupSettings(ServerUserSettings target)
	{
		int num = 0;
		int num2 = 1;
		Guid guid = Guid.NewGuid();
		StringBuilder stringBuilder = new StringBuilder(string.Format(CultureInfo.CurrentCulture, "Server Startup Configurations{0}", Environment.NewLine));
		PropertyInfo[] properties = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		int num3 = (int)Math.Ceiling((double)properties.Length / 50.0) + 1;
		stringBuilder = stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, "Page = {0} of {1}{2}Correlation ID = {3}{2}", num2, num3, Environment.NewLine, guid));
		PropertyInfo[] array = properties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!(propertyInfo != null))
			{
				continue;
			}
			ConfigSettingAttribute customAttribute = propertyInfo.GetCustomAttribute<ConfigSettingAttribute>();
			if (customAttribute != null && customAttribute.EmitSetting != ConfigSettings.EmitSetting.Denied)
			{
				stringBuilder = stringBuilder.Append(customAttribute.GetFormattedDescription(target, propertyInfo));
				num++;
				if (num % 50 == 0)
				{
					NavDiagnostics.GlobalInstance?.SendTraceTag("00000N3", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, stringBuilder.ToString());
					stringBuilder.Clear();
					num2++;
					stringBuilder = stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, "Page = {0} of {1}{2}Correlation ID = {3}{2}", num2, num3, Environment.NewLine, guid));
				}
			}
		}
		if (!string.IsNullOrEmpty(stringBuilder.ToString()))
		{
			NavDiagnostics.GlobalInstance?.SendTraceTag("00000N3", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, stringBuilder.ToString());
		}
		LogApplicationSpecificSettings(target, stringBuilder, string.Format(CultureInfo.CurrentCulture, "Page = {0} of {1}{2}Correlation ID = {3}{2}", num2 + 1, num3, Environment.NewLine, guid), "00000N3");
	}

	private static void LogApplicationSpecificSettings(ServerUserSettings target, StringBuilder serverConfigLogString, string logHeader, string tag)
	{
		foreach (string saasApplicationIdType in ApplicationIdTypes.SaasApplicationIdTypes)
		{
			target.GetAppSpecificSettings(saasApplicationIdType);
		}
		serverConfigLogString.Clear();
		serverConfigLogString = serverConfigLogString.Append(logHeader);
		target.appSpecificSettings.ForEach((KeyValuePair<string, AppSpecificServerSettings> pair) =>
		{
			if (pair.Value != null)
			{
				serverConfigLogString.AppendLine("AppSpecific values for " + pair.Key);
				pair.Value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ForEach((PropertyInfo pi) =>
				{
					ConfigSettingAttribute customAttribute = pi.GetCustomAttribute<ConfigSettingAttribute>();
					if (customAttribute != null)
					{
						serverConfigLogString = serverConfigLogString.Append(customAttribute.GetFormattedDescription(pair.Value, pi));
					}
				});
			}
		});
		NavDiagnostics.GlobalInstance?.SendTraceTag(tag, Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, serverConfigLogString.ToString());
	}

	public static IXPathNavigable GetSettingsFromExecutableWithDefaultValuesApplied(string exePath)
	{
		string tempFileName = Path.GetTempFileName();
		try
		{
			XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
			System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(exePath);
			ServerSettingsBase<ServerUserSettings>.ApplyDefaultSettings(configuration);
			configuration.AppSettings.File = null;
			configuration.SaveAs(tempFileName);
			using (XmlReader reader = XmlHelper.SafeXmlReader(tempFileName))
			{
				xmlDocument.Load(reader);
			}
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

	public static IXPathNavigable GetSettingsWithDefaultValuesApplied(string appConfigPath)
	{
		string tempFileName = Path.GetTempFileName();
		try
		{
			XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
			System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
			{
				ExeConfigFilename = appConfigPath
			}, ConfigurationUserLevel.None);
			ServerSettingsBase<ServerUserSettings>.ApplyDefaultSettings(configuration);
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

	public static System.Configuration.Configuration GetConfigurationWithDefaultsApplied(string appConfigPath)
	{
		System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
		{
			ExeConfigFilename = appConfigPath
		}, ConfigurationUserLevel.None);
		ServerSettingsBase<ServerUserSettings>.ApplyDefaultSettings(configuration);
		return configuration;
	}

	public void ReloadSettings()
	{
		if (settingsProvider != null)
		{
			settingsProvider.Refresh();
			LoadAndValidateSettings();
		}
	}

	public AppSpecificServerSettings GetAppSpecificSettings(string applicationId)
	{
		if (string.IsNullOrEmpty(applicationId))
		{
			return null;
		}
		if (appSpecificSettings.ContainsKey(applicationId))
		{
			return appSpecificSettings[applicationId];
		}
		AppSpecificConfigurationSettingsProvider appSpecificConfigurationSettingsProvider = new AppSpecificConfigurationSettingsProvider(applicationId);
		if (!appSpecificConfigurationSettingsProvider.ConfigFileExists())
		{
			appSpecificSettings.TryAdd(applicationId, null);
			return null;
		}
		AppSpecificServerSettings appSpecificServerSettings = new AppSpecificServerSettings();
		LoadProperties(appSpecificServerSettings, appSpecificConfigurationSettingsProvider);
		ValidateServerSettings(appSpecificServerSettings);
		appSpecificSettings.TryAdd(applicationId, appSpecificServerSettings);
		return appSpecificSettings[applicationId];
	}

	public static void UpdateDynamicSetting(string settingName, string settingValue)
	{
		if (string.IsNullOrEmpty(settingName))
		{
			throw new ArgumentNullException("settingName");
		}
		if (Instance.dynamicSettingsUpdaters.TryGetValue(settingName, out var value))
		{
			value(settingName, settingValue);
			return;
		}
		throw NavDiagnostics.GetMostSpecificInstance.SendExceptionTag("00000SZ", Category.Runtime, new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.SettingNameIsNotDynamic, settingName)));
	}

	private void CombineDatabaseServerAndInstance()
	{
		ServerName = DatabaseServer.Value.TrimEnd('\\');
		if (!string.IsNullOrEmpty(DatabaseInstance.Value))
		{
			ServerName = ServerName + "\\" + DatabaseInstance.Value;
		}
	}

	private ServerUserSettings(SettingsProvider settingsProvider, ConcurrentDictionary<string, AppSpecificServerSettings> appSpecificSettings)
	{
		this.settingsProvider = settingsProvider;
		this.appSpecificSettings = appSpecificSettings;
		if (settingsProvider is ConfigurationSettingsProvider configurationSettingsProvider)
		{
			SettingsFileName = configurationSettingsProvider.SettingsFileName;
		}
	}

	private void LoadSettings()
	{
		LoadProperties(this);
		CombineDatabaseServerAndInstance();
	}

	private void LoadAndValidateSettings()
	{
		LoadSettings();
		ValidateServerSettings();
	}

	internal static void ValidateServerSettings(AppSpecificServerSettings appSpecificSettings = null)
	{
		if (appSpecificSettings != null)
		{
			if (!string.IsNullOrEmpty(appSpecificSettings.SupportedLanguages.Value) && !appSpecificSettings.SupportedLanguages.SupportedLanguagesStrings.Contains(appSpecificSettings.DefaultLanguage.Value, StringComparer.OrdinalIgnoreCase))
			{
				throw new NavConfigurationException(string.Format(CultureInfo.InvariantCulture, "Configuration value {0}:'{1}' must be included in the list of Supported Languages '{2}'.", "DefaultLanguage", appSpecificSettings.DefaultLanguage.Value, appSpecificSettings.SupportedLanguages.Value));
			}
			return;
		}
		if (!string.IsNullOrEmpty(Instance.SupportedLanguages.Value) && !Instance.SupportedLanguages.SupportedLanguagesStrings.Contains(Instance.DefaultLanguage.Value, StringComparer.OrdinalIgnoreCase))
		{
			throw new NavConfigurationException(string.Format(CultureInfo.InvariantCulture, "Configuration value {0}:'{1}' must be included in the list of Supported Languages '{2}'.", "DefaultLanguage", Instance.DefaultLanguage.Value, Instance.SupportedLanguages.Value));
		}
		if (!string.IsNullOrEmpty(Instance.SupportedLanguages.Value) && !Instance.SupportedLanguages.SupportedLanguagesStrings.Contains(Instance.ServicesLanguage.Value, StringComparer.OrdinalIgnoreCase))
		{
			throw new NavConfigurationException(string.Format(CultureInfo.InvariantCulture, "Configuration value {0}:'{1}' must be included in the list of Supported Languages '{2}'.", "ServicesLanguage", Instance.ServicesLanguage.Value, Instance.SupportedLanguages.Value));
		}
	}

	private void LoadProperties(object target, SettingsProvider provider = null)
	{
		if (provider == null)
		{
			provider = settingsProvider;
		}
		PropertyInfo[] properties = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			ConfigSettingAttribute customAttribute = propertyInfo.GetCustomAttribute<ConfigSettingAttribute>();
			if (customAttribute == null)
			{
				continue;
			}
			ValidValueSetAttribute customAttribute2 = propertyInfo.GetCustomAttribute<ValidValueSetAttribute>();
			string value = string.Empty;
			bool flag = provider?.TryGetConfigurationSettingValue(customAttribute.SettingName, out value, customAttribute2?.ValidValues) ?? false;
			Type propertyType = propertyInfo.PropertyType;
			if (string.IsNullOrEmpty(value))
			{
				if ((propertyType == typeof(TimeInterval)) & flag)
				{
					throw new NavConfigurationException(Lang.UnableToParseConfigSetting, customAttribute.SettingName, propertyType.Name);
				}
				value = customAttribute.DefaultValue.ToString();
			}
			try
			{
				if (SettingsHelper.TryParseAsMaxValue(value, customAttribute.SettingName, propertyType, out var maxValue))
				{
					value = maxValue.ToString();
				}
				propertyInfo.SetValue(target, propertyType.InvokeMember("Parse", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod, null, null, new object[2] { value, customAttribute.SettingName }, CultureInfo.CurrentCulture), null);
				if (customAttribute.DynamicallyUpdatable && !dynamicSettingsUpdaters.ContainsKey(customAttribute.SettingName))
				{
					MethodInfo method = propertyType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod, null, new Type[2]
					{
						typeof(string),
						typeof(string)
					}, null);
					dynamicSettingsUpdaters.TryAdd(customAttribute.SettingName, CreateValueUpdater(customAttribute.SettingName, method, propertyInfo));
				}
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}
	}

	internal static string ReadProductName(string name)
	{
		string settingName = "ProductNames_" + name;
		if (!ServerSettingsBase<ServerUserSettings>.SettingExists(settingName))
		{
			return null;
		}
		string text = ServerSettingsBase<ServerUserSettings>.ReadSettingValue(Instance, settingName).ToString();
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return null;
	}

	private Action<string, string> CreateValueUpdater(string settingName, MethodInfo methodInfo, PropertyInfo propInfo)
	{
		return (string name, string value) =>
		{
			try
			{
				object obj = methodInfo.Invoke(this, new object[2] { value, name });
				NavDiagnostics.GlobalInstance?.SendTraceTag("00000UN", Category.Configuration, Verbosity.Normal, PrivacyClassification.SystemMetadata, "The dynamically updatable setting '{0}'is being changed from {1} to {2}.", name, propInfo.GetValue(this)?.ToString() ?? "No value", obj?.ToString() ?? "No value");
				propInfo.GetSetMethod().Invoke(this, new object[1] { obj });
				SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(name));
			}
			catch (Exception ex)
			{
				NavConfigurationException ex2 = new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ErrorUpdatingDynamicSetting, settingName, ex.Message), ex);
				NavDiagnostics.GlobalInstance?.SendExceptionTag("00000T0", Category.Runtime, ex2);
				throw ex2;
			}
		};
	}
}
