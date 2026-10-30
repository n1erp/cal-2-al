using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavObject
{
	public abstract NavNclType NclType { get; }

	public NavType NavType => NavNclTypeHelper.GetNavTypeFromNclType(NclType);
}
