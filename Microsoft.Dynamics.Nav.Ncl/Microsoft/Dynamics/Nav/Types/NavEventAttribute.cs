using System;

namespace Microsoft.Dynamics.Nav.Types;

[AttributeUsage(AttributeTargets.Method)]
public sealed class NavEventAttribute : Attribute
{
	public NavEventType EventType { get; private set; }

	public NavTriggerEventType TriggerEventType { get; private set; }

	public bool IncludeSender { get; private set; }

	public bool AllowGlobalVarAccess { get; private set; }

	public NavEventAttribute(NavTriggerEventType triggerEventType)
	{
		EventType = NavEventType.Trigger;
		TriggerEventType = triggerEventType;
	}

	public NavEventAttribute(NavEventType eventType, bool includeSender = false, bool allowGlobalVarAccess = false)
	{
		EventType = eventType;
		IncludeSender = includeSender & (EventType == NavEventType.Business || EventType == NavEventType.Integration);
		AllowGlobalVarAccess = allowGlobalVarAccess & (EventType == NavEventType.Integration);
	}
}
