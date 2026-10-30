using Microsoft.Dynamics.Nav.Apps.Runtime;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface INavAppOperationContext
{
	NavAppGroup TargetGroup { get; }

	NavAppRuntimeMetadata AppBeingTargeted { get; }

	string ActivityId { get; }

	NavTenant Tenant { get; }
}
