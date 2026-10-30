using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class TestTenantDatabaseSchemaResponse : ServerResponse
{
	[DataMember]
	private readonly IEnumerable<NavTenantDatabaseSchemaFault> errors;

	public IEnumerable<NavTenantDatabaseSchemaFault> Errors => errors;

	public TestTenantDatabaseSchemaResponse(IEnumerable<NavTenantDatabaseSchemaFault> errors)
	{
		this.errors = errors;
	}
}
