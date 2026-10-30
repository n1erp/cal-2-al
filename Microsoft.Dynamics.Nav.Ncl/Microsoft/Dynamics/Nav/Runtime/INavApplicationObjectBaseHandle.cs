using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavApplicationObjectBaseHandle
{
	NavApplicationObjectBase Target { get; }

	bool HasTarget { get; }

	ApplicationObjectId ObjectId { get; }
}
