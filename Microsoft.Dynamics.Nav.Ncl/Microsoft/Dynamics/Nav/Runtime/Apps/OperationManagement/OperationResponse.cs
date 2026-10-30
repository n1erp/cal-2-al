using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class OperationResponse
{
	public NavAppTenantOperationFields.NavAppOperationStatus Status { get; set; }

	public string Details { get; set; }
}
