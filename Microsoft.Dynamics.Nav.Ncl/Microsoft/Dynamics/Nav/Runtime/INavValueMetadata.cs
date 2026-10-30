using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavValueMetadata
{
	NavType NavType { get; }

	NavNclType NclType { get; }

	int NavDefinedLengthMetadata { get; }

	NCLOptionMetadata NavOptionMetadata { get; }
}
