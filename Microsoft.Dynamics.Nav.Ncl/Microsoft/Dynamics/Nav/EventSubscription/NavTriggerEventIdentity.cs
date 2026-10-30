using System;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Diagnostics;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public struct NavTriggerEventIdentity : IEquatable<NavTriggerEventIdentity>
{
	private readonly NavTriggerEventType eventType;

	private readonly string eventId;

	public NavTriggerEventType EventType => eventType;

	public string EventId => eventId;

	public bool IsFieldBasedEvent => NavEventSubscription.TriggerEventTypeIsFieldBased(EventType);

	public bool HasValue => EventType != NavTriggerEventType.None;

	public NavTriggerEventIdentity(NavTriggerEventType eventType, string eventId = null, string elementName = null, int objectId = -1, ObjectType objectType = ObjectType.TableData)
	{
		this.eventType = eventType;
		this.eventId = eventId;
		if (NavCurrentThread.Session != null && NavCurrentThread.Session.IsEventSessionRecorderEnabled && NavServerEventSource.Log != null)
		{
			NavServerEventWriter.WriteEventRaised(NavCurrentThread.Session.EventTelemetryRegistrar, (int)objectType, objectId, eventType.ToString(), elementName ?? string.Empty, 2);
		}
		if (IsFieldBasedEvent && eventId == null)
		{
			throw new ArgumentNullException("eventId");
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is NavTriggerEventIdentity)
		{
			return Equals((NavTriggerEventIdentity)obj);
		}
		return false;
	}

	public bool Equals(NavTriggerEventIdentity other)
	{
		if (EventType == other.EventType)
		{
			return EventId == other.EventId;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return EventType.GetHashCode() ^ EventId.GetHashCode();
	}

	public static bool operator ==(NavTriggerEventIdentity first, NavTriggerEventIdentity second)
	{
		return first.Equals(second);
	}

	public static bool operator !=(NavTriggerEventIdentity first, NavTriggerEventIdentity second)
	{
		return !first.Equals(second);
	}
}
