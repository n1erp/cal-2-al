using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTenantChangedEventArgs : EventArgs
{
	public string TenantId { get; set; }

	public NavTenantChangedEventArgs(string tenantId)
	{
		TenantId = tenantId;
	}
}
