using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MountTenantDatabaseArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	public NavTenantDatabaseSettings TenantDatabaseSettings { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool VerifyServerInstanceKey { get; set; }

	public MountTenantDatabaseArguments()
	{
	}

	public MountTenantDatabaseArguments(NavTenantDatabaseSettings tenantDatabaseSettings, bool verifyServerInstanceKey = false)
	{
		TenantDatabaseSettings = tenantDatabaseSettings;
		VerifyServerInstanceKey = verifyServerInstanceKey;
	}
}
