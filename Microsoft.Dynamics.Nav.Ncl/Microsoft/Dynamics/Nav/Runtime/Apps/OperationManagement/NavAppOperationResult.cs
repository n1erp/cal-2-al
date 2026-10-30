using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal abstract class NavAppOperationResult : INavAppOperationKey
{
	public abstract NavAppTenantOperationFields.NavAppOperationType OperationType { get; }

	public NavAppOperationResultStatus ResultStatus { get; set; }

	public string Details { get; set; }

	public abstract string Key { get; }
}
