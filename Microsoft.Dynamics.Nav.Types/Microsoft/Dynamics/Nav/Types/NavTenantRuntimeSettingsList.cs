using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavTenantRuntimeSettingsList : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	public IList<NavTenantRuntimeSettings> TenantRuntimeSettingsList { get; private set; }

	public NavTenantRuntimeSettingsList()
	{
	}

	public NavTenantRuntimeSettingsList(IList<NavTenantRuntimeSettings> tenantRuntimeSettingsList)
	{
		TenantRuntimeSettingsList = tenantRuntimeSettingsList;
	}
}
