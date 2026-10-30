using Microsoft.Dynamics.Nav.Runtime.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Utility;

internal static class IAppOriginExtensions
{
	public static bool OriginatesFrom(this IAppOrigin origin, NavAppTableContext context)
	{
		return origin.AppOrigin.Equals(context.CompanionTableAppOrigin);
	}

	public static bool OriginatesFrom(this IAppOrigin origin, NavAppOrigin targetOrigin)
	{
		return origin.AppOrigin.Equals(targetOrigin);
	}
}
