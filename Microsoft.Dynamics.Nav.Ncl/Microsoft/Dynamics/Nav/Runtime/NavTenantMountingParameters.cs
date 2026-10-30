using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTenantMountingParameters
{
	public NavTenantSettings TenantSettings { get; set; }

	public bool OverwriteTenantIdInDatabase { get; set; }

	public bool VerifyDatabaseConnection { get; set; }

	public bool EnsureDatabaseExclusiveStateWhileOverwritingTenantId { get; set; }

	public bool ExclusiveAccessMode { get; set; }

	public bool AllowAsync { get; set; }

	public bool VerifyServerInstanceKey { get; set; }

	public NavTenantMountingParameters()
	{
	}

	public NavTenantMountingParameters(NavTenantSettings tenantSettings)
	{
		TenantSettings = tenantSettings;
	}
}
