using System;
using System.Reflection;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

internal class NavTriggerEventInfo
{
	public MethodInfo TriggerEventMethod { get; private set; }

	public NavEventAttribute TriggerEventAttribute { get; private set; }

	public NavTriggerEventType TriggerEventType => TriggerEventAttribute.TriggerEventType;

	public NavTriggerEventInfo(MethodInfo eventMethodInfo)
	{
		if (eventMethodInfo == null)
		{
			throw new ArgumentNullException("eventMethodInfo");
		}
		TriggerEventMethod = eventMethodInfo;
		TriggerEventAttribute = eventMethodInfo.GetCustomAttribute<NavEventAttribute>();
	}
}
