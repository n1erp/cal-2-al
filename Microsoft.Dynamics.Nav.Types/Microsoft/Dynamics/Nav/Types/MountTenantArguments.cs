using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MountTenantArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	public NavTenantSettings TenantSettings { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool OverwriteTenantIdInDatabase { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool VerifyServerInstanceKey { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool SetSingleUserWhileOverwritingTenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool ThrowOnSingleUserError { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool Async { get; set; }

	public MountTenantArguments()
	{
	}

	public MountTenantArguments(NavTenantSettings tenantSettings, bool overwriteTenantIdInDatabase = false, bool verifyServerInstanceKey = false, bool setSingleUserWhileOverwritingTenantId = false, bool throwOnSingleUserError = false, bool async = false)
	{
		TenantSettings = tenantSettings;
		OverwriteTenantIdInDatabase = overwriteTenantIdInDatabase;
		VerifyServerInstanceKey = verifyServerInstanceKey;
		SetSingleUserWhileOverwritingTenantId = setSingleUserWhileOverwritingTenantId;
		ThrowOnSingleUserError = throwOnSingleUserError;
		Async = async;
	}
}
