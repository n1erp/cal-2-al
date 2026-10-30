using System.Reflection;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventPublisherMethodInfo
{
	public MethodInfo RawMethodInfo { get; }

	public NavEventAttribute Attribute { get; }

	public NavEventPublisherMethodInfo(MethodInfo rawMethodInfo)
	{
		RawMethodInfo = rawMethodInfo;
		Attribute = rawMethodInfo.GetCustomAttribute<NavEventAttribute>();
	}
}
