using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavTenantSettings : IExtensibleDataObject
{
	public const int MaxDisplayNameLength = 250;

	public const int MaxUrlNameLength = 30;

	private static readonly string[] EmptyAlternateId = new string[0];

	private bool readOnly;

	private Dictionary<string, object> tenantSettings = new Dictionary<string, object>();

	private string[] alternateIdCache;

	private string concatenatedAlternateIdCache;

	public ExtensionDataObject ExtensionData { get; set; }

	[ConfigParameter]
	[RegexStringValidator("^[A-Za-z0-9]([A-Za-z0-9\\-]{0,61}[A-Za-z0-9])?$")]
	[DataMember]
	[TelemetryOii]
	public string Id
	{
		get
		{
			return Get<string>(null, "Id");
		}
		set
		{
			Set(value, "Id");
		}
	}

	[ConfigParameter]
	[DataMember]
	[TelemetryOii]
	public string TenantDatabaseId
	{
		get
		{
			return Get<string>(null, "TenantDatabaseId");
		}
		set
		{
			Set(value, "TenantDatabaseId");
		}
	}

	[ConfigParameter]
	[DataMember]
	[TelemetryOii]
	public string DatabaseName
	{
		get
		{
			return Get<string>(null, "DatabaseName");
		}
		set
		{
			Set(value, "DatabaseName");
		}
	}

	[ConfigParameter]
	[DataMember]
	[TelemetrySystemMetadata]
	public string DatabaseServer
	{
		get
		{
			return Get<string>(null, "DatabaseServer");
		}
		set
		{
			Set(value, "DatabaseServer");
		}
	}

	[DataMember]
	[TelemetryOii]
	private string[] alternateId
	{
		get
		{
			if (alternateIdCache == null)
			{
				alternateIdCache = Get(EmptyAlternateId, "alternateId");
			}
			return alternateIdCache;
		}
		set
		{
			alternateIdCache = null;
			concatenatedAlternateIdCache = null;
			Set(value, "alternateId");
		}
	}

	public string ConcatenatedAlternateId
	{
		get
		{
			if (concatenatedAlternateIdCache == null)
			{
				string[] value = alternateId;
				concatenatedAlternateIdCache = string.Join(";", value);
			}
			return concatenatedAlternateIdCache;
		}
	}

	[ConfigParameter]
	[TypeConverter(typeof(ReadOnlyCollectionConverter))]
	public ReadOnlyCollection<string> AlternateId
	{
		get
		{
			return new ReadOnlyCollection<string>(alternateId);
		}
		private set
		{
			alternateId = value?.ToArray() ?? EmptyAlternateId;
		}
	}

	[ConfigParameter]
	[DataMember]
	[TelemetrySystemMetadata]
	public bool AllowAppDatabaseWrite
	{
		get
		{
			return Get(defaultValue: false, "AllowAppDatabaseWrite");
		}
		set
		{
			Set(value, "AllowAppDatabaseWrite");
		}
	}

	[ConfigParameter]
	[DataMember]
	[TelemetrySystemMetadata]
	public bool NasServicesEnabled
	{
		get
		{
			return Get(defaultValue: false, "NasServicesEnabled");
		}
		set
		{
			Set(value, "NasServicesEnabled");
		}
	}

	[ConfigParameter]
	[DataMember]
	[TelemetrySystemMetadata]
	public bool RunNasWithAdminRights
	{
		get
		{
			return Get(defaultValue: false, "RunNasWithAdminRights");
		}
		set
		{
			Set(value, "RunNasWithAdminRights");
		}
	}

	[ConfigParameter(CmdletParameter = false)]
	[TypeConverter(typeof(EncryptionProviderConverter))]
	[DataMember]
	[TelemetrySystemMetadata]
	public EncryptionProvider EncryptionProvider
	{
		get
		{
			return Get(EncryptionProvider.LocalKeyFile, "EncryptionProvider");
		}
		set
		{
			Set(value, "EncryptionProvider");
		}
	}

	[ConfigParameter(CmdletParameter = false)]
	[DataMember]
	public AzureKeyVaultSettings AzureKeyVaultSettings
	{
		get
		{
			return Get<AzureKeyVaultSettings>(null, "AzureKeyVaultSettings");
		}
		set
		{
			Set(value, "AzureKeyVaultSettings");
		}
	}

	[ConfigParameter]
	[StringValidator(MinLength = 0, MaxLength = 30)]
	[DataMember]
	[TelemetryOii]
	public string DefaultCompany
	{
		get
		{
			return Get("", "DefaultCompany");
		}
		set
		{
			Set(value, "DefaultCompany");
		}
	}

	[ConfigParameter]
	[TypeConverter(typeof(TimeZoneInfoConverter))]
	[TelemetrySystemMetadata]
	public TimeZoneInfo DefaultTimeZone
	{
		get
		{
			return Get<TimeZoneInfo>(null, "DefaultTimeZone");
		}
		set
		{
			Set(value, "DefaultTimeZone");
		}
	}

	[ConfigParameter(CmdletParameter = false)]
	[DataMember]
	[TelemetryEuii]
	public string DatabaseUserName
	{
		get
		{
			return Get<string>(null, "DatabaseUserName");
		}
		set
		{
			Set(value, "DatabaseUserName");
		}
	}

	[ConfigParameter]
	[StringValidator(MaxLength = 250)]
	[DataMember]
	[TelemetryOii]
	public string DisplayName
	{
		get
		{
			return Get<string>(null, "DisplayName");
		}
		set
		{
			Set(value, "DisplayName");
		}
	}

	[ConfigParameter]
	[TypeConverter(typeof(NavTenantEnvironmentTypeConverter))]
	[DataMember]
	[TelemetrySystemMetadata]
	public NavTenantEnvironmentType EnvironmentType
	{
		get
		{
			return Get(NavTenantEnvironmentType.Production, "EnvironmentType");
		}
		set
		{
			Set(value, "EnvironmentType");
		}
	}

	[ConfigParameter(CmdletParameter = false)]
	[TypeConverter(typeof(ProtectedDatabasePasswordConverter))]
	[DataMember]
	public ProtectedDatabasePassword ProtectedDatabasePassword { get; set; }

	[ConfigParameter]
	[StringValidator(MinLength = 0, MaxLength = 30)]
	[DataMember]
	[TelemetryOii]
	public string UrlName
	{
		get
		{
			return Get<string>(null, "UrlName");
		}
		set
		{
			if (value != null && !Uri.IsWellFormedUriString(value, UriKind.Relative))
			{
				throw new NavNCLArgumentException("Tenant url name is not a valid relative uri.");
			}
			Set(value, "UrlName");
		}
	}

	[DataMember]
	[TelemetrySystemMetadata]
	private string DefaultTimeZoneSerialized
	{
		get
		{
			return DefaultTimeZone?.ToSerializedString();
		}
		set
		{
			DefaultTimeZone = ((value == null) ? null : TimeZoneInfo.FromSerializedString(value));
		}
	}

	[ConfigParameter]
	[DataMember]
	[TelemetrySystemMetadata]
	public string ExchangeAuthenticationMetadataLocation
	{
		get
		{
			return Get<string>(null, "ExchangeAuthenticationMetadataLocation");
		}
		set
		{
			Set(value, "ExchangeAuthenticationMetadataLocation");
		}
	}

	[ConfigParameter]
	[DataMember]
	[TelemetryOii]
	public string AadTenantId
	{
		get
		{
			return Get<string>(null, "AadTenantId");
		}
		set
		{
			Set(value, "AadTenantId");
		}
	}

	internal NavTenantSettings()
	{
	}

	public NavTenantSettings(NavTenantSettings navTenantSettings, IEnumerable<string> alternateId)
		: this(navTenantSettings)
	{
		this.alternateId = alternateId?.ToArray() ?? EmptyAlternateId;
	}

	public NavTenantSettings(NavTenantSettings navTenantSettings)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize(memoryStream, navTenantSettings.tenantSettings);
			binaryFormatter.Binder = new NavSerializationBinder();
			memoryStream.Position = 0L;
			tenantSettings = (Dictionary<string, object>)binaryFormatter.Deserialize(memoryStream);
		}
		ProtectedDatabasePassword = navTenantSettings.ProtectedDatabasePassword;
	}

	public NavTenantSettings(string id, string tenantDatabaseId, IEnumerable<string> alternateId = null)
		: this()
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		if (string.IsNullOrEmpty(tenantDatabaseId))
		{
			throw new ArgumentNullException("tenantDatabaseId");
		}
		Id = id;
		TenantDatabaseId = tenantDatabaseId;
		this.alternateId = alternateId?.ToArray() ?? EmptyAlternateId;
	}

	public NavTenantSettings(string id, string databaseName, string databaseServer, string databaseUserName, ProtectedDatabasePassword databasePassword, IEnumerable<string> alternateId = null, bool allowAppDatabaseWrite = false, bool nasServicesEnabled = false, string defaultCompany = "", TimeZoneInfo defaultTimeZone = null, bool runNasWithAdminRights = false, EncryptionProvider encryptionProvider = EncryptionProvider.LocalKeyFile, AzureKeyVaultSettings azureKeyVaultSettings = null, string exchangeAuthenticationMetadataLocation = null, string azureActiveDirectoryTenantId = null, string displayName = null, string urlName = null, NavTenantEnvironmentType environmentType = NavTenantEnvironmentType.Production)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		if (string.IsNullOrEmpty(databaseName))
		{
			throw new ArgumentNullException("databaseName");
		}
		Id = id;
		this.alternateId = alternateId?.ToArray() ?? EmptyAlternateId;
		DatabaseServer = databaseServer;
		DatabaseName = databaseName;
		DatabaseUserName = databaseUserName;
		ProtectedDatabasePassword = databasePassword;
		AllowAppDatabaseWrite = allowAppDatabaseWrite;
		NasServicesEnabled = nasServicesEnabled;
		DefaultCompany = defaultCompany;
		DefaultTimeZone = defaultTimeZone;
		RunNasWithAdminRights = runNasWithAdminRights;
		EncryptionProvider = encryptionProvider;
		AzureKeyVaultSettings = azureKeyVaultSettings;
		ExchangeAuthenticationMetadataLocation = exchangeAuthenticationMetadataLocation;
		AadTenantId = azureActiveDirectoryTenantId;
		DisplayName = displayName;
		UrlName = urlName;
		EnvironmentType = environmentType;
	}

	public Uri GetWSFederationLoginEndpointByApplicationId(string applicationId)
	{
		Uri uri = ServerUserSettings.Instance.GetAppSpecificSettings(applicationId)?.AppIdUri.Value;
		return AadHelper.CalculateWSFederationLoginEndpoint(ServerUserSettings.Instance.WSFederationLoginEndpoint.Value, AadTenantId, uri ?? ServerUserSettings.Instance.AppIdUri.Value);
	}

	public Uri GetWSFederationMetadataLocation()
	{
		return AadHelper.CalculateWSFederationMetadataLocation(ServerUserSettings.Instance.WSFederationMetadataLocation.Value, AadTenantId);
	}

	public void MakeImmutable()
	{
		readOnly = true;
	}

	protected T Get<T>(T defaultValue, [CallerMemberName] string propertyName = null)
	{
		if (propertyName != null && tenantSettings.TryGetValue(propertyName, out var value) && value != null)
		{
			return (T)value;
		}
		return defaultValue;
	}

	protected void Set<T>(T value, [CallerMemberName] string propertyName = null)
	{
		if (readOnly)
		{
			throw new SettingsPropertyIsReadOnlyException();
		}
		if (propertyName != null)
		{
			tenantSettings[propertyName] = value;
		}
	}

	[OnDeserializing]
	private void OnDeserializing(StreamingContext context)
	{
		tenantSettings = new Dictionary<string, object>();
	}
}
