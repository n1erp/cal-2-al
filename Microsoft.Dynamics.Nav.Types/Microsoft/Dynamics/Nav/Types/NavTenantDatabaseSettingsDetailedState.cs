using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavTenantDatabaseSettingsDetailedState : IExtensibleDataObject
{
	[DataMember]
	[TelemetrySystemMetadata]
	public NavTenantDatabaseState State { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string StateDetails { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }

	public NavTenantDatabaseSettingsDetailedState()
	{
		State = NavTenantDatabaseState.Mounted;
		StateDetails = string.Empty;
	}

	public NavTenantDatabaseSettingsDetailedState(NavTenantDatabaseState state)
	{
		State = state;
		StateDetails = string.Empty;
	}

	public NavTenantDatabaseSettingsDetailedState(NavTenantDatabaseState state, string stateDetails)
	{
		State = state;
		StateDetails = stateDetails;
	}
}
