using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ConnectionRequest : IExtensibleDataObject
{
	private int localId;

	private ExtensionDataObject unknownDataBag;

	private string tenantId;

	private string clientHostName;

	private string signInRelativePath;

	private string exclusiveAccessTicket;

	public virtual ExtensionDataObject ExtensionData
	{
		get
		{
			return unknownDataBag;
		}
		set
		{
			unknownDataBag = value;
		}
	}

	[DataMember]
	[TelemetrySystemMetadata]
	public int LCID { get; set; } = NavResourceManager.DefaultLanguageId;

	[DataMember]
	[TelemetrySystemMetadata]
	public bool UseTimeZoneForCurrentSessionOnly { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool UseLocaleForCurrentSessionOnly { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool DisableUserProfileSave { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public int LocalId
	{
		get
		{
			if (localId != 0)
			{
				return localId;
			}
			return LCID;
		}
		set
		{
			localId = value;
		}
	}

	[DataMember]
	[TelemetrySystemMetadata]
	public int DefaultLCID { get; set; } = NavResourceManager.DefaultLanguageId;

	[DataMember]
	[TelemetryOii]
	public string TenantId
	{
		get
		{
			return tenantId ?? string.Empty;
		}
		set
		{
			tenantId = value;
		}
	}

	[DataMember]
	[TelemetryOii]
	public string ClientHostName
	{
		get
		{
			return clientHostName ?? string.Empty;
		}
		set
		{
			clientHostName = value;
		}
	}

	[DataMember]
	[TelemetrySystemMetadata]
	public string SignInRelativePath
	{
		get
		{
			return signInRelativePath ?? string.Empty;
		}
		set
		{
			signInRelativePath = value;
		}
	}

	[DataMember]
	[TelemetrySystemMetadata]
	public string ExclusiveAccessTicket
	{
		get
		{
			return exclusiveAccessTicket ?? string.Empty;
		}
		set
		{
			exclusiveAccessTicket = value;
		}
	}

	[DataMember]
	[TelemetrySystemMetadata]
	public string ProfileName { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public Guid ProfileAppId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public ProfileScope ProfileScope { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool DebugMode { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool ConfigurationModeEnabled { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool PersonalizationDisabled { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string TimeZoneId { get; set; } = TimeZoneInfo.Utc.StandardName;

	[DataMember]
	[TelemetrySystemMetadata]
	public ConnectionType ClientConnectionType { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public int ClientCompatibilityVersion { get; set; } = 14002;

	[DataMember]
	[TelemetrySystemMetadata]
	public Version ClientAssemblyFileVersion { get; set; }

	[DataMember]
	[TelemetryAccountData]
	public byte[] PrivateLicense { get; set; }

	[DataMember]
	[TelemetryEuii]
	public string ClientMachineName { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool ClientIs64BitProcess { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string AppId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public Guid ApplicationExtension { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string DebuggingContext { get; set; }
}
