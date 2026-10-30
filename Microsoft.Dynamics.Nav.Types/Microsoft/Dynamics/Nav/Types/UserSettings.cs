using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class UserSettings : ServerResponse
{
	[DataMember]
	private TimeSpan timeout;

	[DataMember]
	private TimeSpan orphanGracePeriod;

	[DataMember]
	private TimeSpan idleClientTimeout;

	[DataMember]
	private int lcid;

	[DataMember]
	private int defaultLcid;

	[DataMember]
	private int localId;

	[DataMember]
	private string tenantId;

	[DataMember]
	private NavTenantEnvironmentType tenantEnvironmentType;

	[DataMember]
	private string tenantEnvironmentName;

	[DataMember]
	private TenantLicenseState licenseState;

	[DataMember]
	private string companyName;

	[DataMember]
	private string profileName;

	[DataMember]
	private bool configurationModeEnabled;

	[DataMember]
	private bool personalizationDisabled;

	[DataMember]
	private bool designerEnabled;

	[DataMember]
	private int maxUploadFileSize = 5242880;

	[DataMember]
	private string serializedTimeZoneInfo;

	[DataMember]
	private bool userProfileTimeZoneChanged;

	[DataMember]
	private string userName;

	[DataMember]
	private bool isTenantAdmin;

	[DataMember]
	private Guid userGuid;

	[DataMember]
	private string userPuid;

	[DataMember]
	private string authenticationObjectId;

	[DataMember]
	private string userUpn;

	[DataMember]
	private Version serverAssemblyFileVersion;

	[DataMember]
	private IEnumerable<ConnectionWarning> serverConnectionWarnings;

	[DataMember]
	private bool enableSaveToWordForRdlcReports = true;

	[DataMember]
	private TimeSpan keepAliveInterval;

	[DataMember]
	private bool enableSaveToExcelForRdlcReports = true;

	[DataMember]
	private bool enableSaveFromReportPreview = true;

	[DataMember]
	private bool useSimplifiedFilters;

	[DataMember]
	private ClientConfiguration clientConfiguration;

	[DataMember]
	private string applicationExtensionName;

	[DataMember]
	private bool enableCompactSerialization;

	public int LCID
	{
		get
		{
			return lcid;
		}
		set
		{
			lcid = value;
		}
	}

	public int DefaultLCID
	{
		get
		{
			return defaultLcid;
		}
		set
		{
			defaultLcid = value;
		}
	}

	public int LocalId
	{
		get
		{
			return localId;
		}
		set
		{
			localId = value;
		}
	}

	public string TenantId
	{
		get
		{
			if (tenantId == null)
			{
				return string.Empty;
			}
			return tenantId;
		}
		set
		{
			tenantId = value;
		}
	}

	public NavTenantEnvironmentType TenantEnvironmentType
	{
		get
		{
			return tenantEnvironmentType;
		}
		set
		{
			tenantEnvironmentType = value;
		}
	}

	public string TenantEnvironmentName
	{
		get
		{
			return tenantEnvironmentName;
		}
		set
		{
			tenantEnvironmentName = value;
		}
	}

	public TenantLicenseState LicenseState
	{
		get
		{
			return licenseState;
		}
		set
		{
			licenseState = value;
		}
	}

	public string CompanyName
	{
		get
		{
			return companyName;
		}
		set
		{
			companyName = value;
		}
	}

	public string ProfileName
	{
		get
		{
			return profileName;
		}
		set
		{
			profileName = value;
		}
	}

	public bool ConfigurationModeEnabled
	{
		get
		{
			return configurationModeEnabled;
		}
		set
		{
			configurationModeEnabled = value;
		}
	}

	public bool PersonalizationDisabled
	{
		get
		{
			return personalizationDisabled;
		}
		set
		{
			personalizationDisabled = value;
		}
	}

	public bool DesignerEnabled
	{
		get
		{
			return designerEnabled;
		}
		set
		{
			designerEnabled = value;
		}
	}

	public TimeSpan Timeout
	{
		get
		{
			return timeout;
		}
		set
		{
			timeout = value;
		}
	}

	public TimeSpan OrphanGracePeriod
	{
		get
		{
			return orphanGracePeriod;
		}
		set
		{
			orphanGracePeriod = value;
		}
	}

	public TimeSpan IdleClientTimeout
	{
		get
		{
			return idleClientTimeout;
		}
		set
		{
			idleClientTimeout = value;
		}
	}

	public int MaxUploadFileSize
	{
		get
		{
			return maxUploadFileSize;
		}
		set
		{
			maxUploadFileSize = value;
		}
	}

	public bool UserProfileTimeZoneChanged
	{
		get
		{
			return userProfileTimeZoneChanged;
		}
		set
		{
			userProfileTimeZoneChanged = value;
		}
	}

	public TimeZoneInfo TimeZoneInfo
	{
		get
		{
			if (serializedTimeZoneInfo != null)
			{
				return TimeZoneInfo.FromSerializedString(serializedTimeZoneInfo);
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				serializedTimeZoneInfo = null;
			}
			else
			{
				serializedTimeZoneInfo = value.ToSerializedString();
			}
		}
	}

	public string UserName
	{
		get
		{
			return userName;
		}
		set
		{
			userName = value;
		}
	}

	public bool IsTenantAdmin
	{
		get
		{
			return isTenantAdmin;
		}
		set
		{
			isTenantAdmin = value;
		}
	}

	public Guid UserGuid
	{
		get
		{
			return userGuid;
		}
		set
		{
			userGuid = value;
		}
	}

	public string UserPuid
	{
		get
		{
			return userPuid;
		}
		set
		{
			userPuid = value;
		}
	}

	public string AuthenticationObjectId
	{
		get
		{
			return authenticationObjectId;
		}
		set
		{
			authenticationObjectId = value;
		}
	}

	public string UserUpn
	{
		get
		{
			return userUpn;
		}
		set
		{
			userUpn = value;
		}
	}

	public Version ServerAssemblyFileVersion
	{
		get
		{
			return serverAssemblyFileVersion;
		}
		set
		{
			serverAssemblyFileVersion = value;
		}
	}

	public IEnumerable<ConnectionWarning> ServerConnectionWarnings
	{
		get
		{
			return serverConnectionWarnings;
		}
		set
		{
			serverConnectionWarnings = value;
		}
	}

	public bool EnableSaveToWordForRdlcReports
	{
		get
		{
			return enableSaveToWordForRdlcReports;
		}
		set
		{
			enableSaveToWordForRdlcReports = value;
		}
	}

	public bool EnableSaveToExcelForRdlcReports
	{
		get
		{
			return enableSaveToExcelForRdlcReports;
		}
		set
		{
			enableSaveToExcelForRdlcReports = value;
		}
	}

	public bool EnableSaveFromReportPreview
	{
		get
		{
			return enableSaveFromReportPreview;
		}
		set
		{
			enableSaveFromReportPreview = value;
		}
	}

	public TimeSpan KeepAliveInterval
	{
		get
		{
			return keepAliveInterval;
		}
		set
		{
			keepAliveInterval = value;
		}
	}

	public bool UseSimplifiedFilters
	{
		get
		{
			return useSimplifiedFilters;
		}
		set
		{
			useSimplifiedFilters = value;
		}
	}

	[DataMember]
	public string ApplicationBuild { get; set; }

	[DataMember]
	public string ApplicationVersion { get; set; }

	public ClientConfiguration ClientConfiguration
	{
		get
		{
			return clientConfiguration;
		}
		set
		{
			clientConfiguration = value;
		}
	}

	public string ApplicationExtensionName
	{
		get
		{
			return applicationExtensionName;
		}
		set
		{
			applicationExtensionName = value;
		}
	}

	public bool EnableCompactSerialization
	{
		get
		{
			return enableCompactSerialization;
		}
		set
		{
			enableCompactSerialization = value;
		}
	}

	public void Apply(UserSettings userSettings)
	{
		if (userSettings == null)
		{
			throw new ArgumentNullException("userSettings");
		}
		ApplicationBuild = userSettings.ApplicationBuild;
		ApplicationVersion = userSettings.ApplicationVersion;
		TenantId = userSettings.TenantId;
		TenantEnvironmentType = userSettings.TenantEnvironmentType;
		TenantEnvironmentName = userSettings.TenantEnvironmentName;
		LicenseState = userSettings.LicenseState;
		CompanyName = userSettings.CompanyName;
		ConfigurationModeEnabled = userSettings.ConfigurationModeEnabled;
		ExtensionData = userSettings.ExtensionData;
		LCID = userSettings.LCID;
		MaxUploadFileSize = userSettings.MaxUploadFileSize;
		OrphanGracePeriod = userSettings.OrphanGracePeriod;
		IdleClientTimeout = userSettings.IdleClientTimeout;
		PersonalizationDisabled = userSettings.PersonalizationDisabled;
		DesignerEnabled = userSettings.DesignerEnabled;
		ProfileName = userSettings.ProfileName;
		Timeout = userSettings.Timeout;
		TimeZoneInfo = userSettings.TimeZoneInfo;
		UserGuid = userSettings.UserGuid;
		UserName = userSettings.UserName;
		IsTenantAdmin = userSettings.IsTenantAdmin;
		UserPuid = userSettings.UserPuid;
		AuthenticationObjectId = userSettings.AuthenticationObjectId;
		UserUpn = userSettings.UserUpn;
		serverAssemblyFileVersion = userSettings.serverAssemblyFileVersion;
		serverConnectionWarnings = userSettings.serverConnectionWarnings;
		UserProfileTimeZoneChanged = userSettings.UserProfileTimeZoneChanged;
		localId = userSettings.localId;
		EnableSaveToWordForRdlcReports = userSettings.EnableSaveToWordForRdlcReports;
		EnableSaveToExcelForRdlcReports = userSettings.EnableSaveToExcelForRdlcReports;
		EnableSaveFromReportPreview = userSettings.EnableSaveFromReportPreview;
		keepAliveInterval = userSettings.keepAliveInterval;
		UseSimplifiedFilters = userSettings.UseSimplifiedFilters;
		ClientConfiguration = userSettings.ClientConfiguration;
		ApplicationExtensionName = userSettings.ApplicationExtensionName;
		enableCompactSerialization = userSettings.EnableCompactSerialization;
	}
}
