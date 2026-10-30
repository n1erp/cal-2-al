using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavTenantRuntimeSettings : NavTenantSettings
{
	[DataMember]
	public NavTenantState State
	{
		get
		{
			return Get(NavTenantState.Mounted, "State");
		}
		set
		{
			Set(value, "State");
		}
	}

	[DataMember]
	public NavTenantDatabaseSyncStatus DetailedState
	{
		get
		{
			return Get<NavTenantDatabaseSyncStatus>(null, "DetailedState");
		}
		set
		{
			Set(value, "DetailedState");
		}
	}

	[DataMember]
	public string DeletionState
	{
		get
		{
			return Get("Not deleted", "DeletionState");
		}
		set
		{
			Set(value, "DeletionState");
		}
	}

	[DataMember]
	public bool IsInExclusiveAccessMode
	{
		get
		{
			return Get(defaultValue: false, "IsInExclusiveAccessMode");
		}
		set
		{
			Set(value, "IsInExclusiveAccessMode");
		}
	}

	[DataMember]
	public string TenantDataVersion
	{
		get
		{
			return Get("Uninitialized", "TenantDataVersion");
		}
		set
		{
			Set(value, "TenantDataVersion");
		}
	}

	[DataMember]
	public string ServerInstance { get; set; }

	public NavTenantRuntimeSettings(NavTenantSettings tenantSettings)
		: base(tenantSettings)
	{
		ProtectedDatabasePassword = null;
	}
}
