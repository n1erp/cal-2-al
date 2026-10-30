using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TenantTableSet
{
	public NavTenant Tenant { get; internal set; }

	public IEnumerable<TableSet> CompanyTableSets { get; internal set; }
}
