using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

internal abstract class NavTriggerEventHandler
{
	private NCLMetaApplicationObject parent;

	private NavEventScope[] eventScopes;

	private Dictionary<NavTriggerEventIdentity, NavEventScope> namedEventScopes;

	public abstract IReadOnlyList<NavTriggerEventInfo> TriggerEventInfos { get; }

	public NCLMetaApplicationObject Parent => parent;

	internal NavTriggerEventHandler(NCLMetaApplicationObject parent)
	{
		this.parent = parent;
	}

	public virtual NavEventScope GetEventScope(NavTriggerEventType triggerEventType, EventScopeGetOption getOption = EventScopeGetOption.None)
	{
		if (eventScopes == null)
		{
			if (getOption != EventScopeGetOption.CreateIfNotFound)
			{
				return null;
			}
			lock (this)
			{
				if (eventScopes == null)
				{
					int num = TriggerEventInfos.Max((NavTriggerEventInfo ti) => (int)ti.TriggerEventType);
					eventScopes = new NavEventScope[num + 1];
				}
			}
		}
		if (triggerEventType < NavTriggerEventType.None || (int)triggerEventType >= eventScopes.Length)
		{
			if (getOption == EventScopeGetOption.CreateIfNotFound)
			{
				throw new InvalidOperationException();
			}
			return null;
		}
		NavEventScope navEventScope = eventScopes[(int)triggerEventType];
		if (navEventScope == null && getOption == EventScopeGetOption.CreateIfNotFound)
		{
			lock (this)
			{
				if (eventScopes[(int)triggerEventType] == null)
				{
					eventScopes[(int)triggerEventType] = new NavEventScope();
				}
				navEventScope = eventScopes[(int)triggerEventType];
			}
		}
		if (navEventScope != null && (getOption == EventScopeGetOption.RemoveIfFound || getOption == EventScopeGetOption.SetStaleIfFound))
		{
			lock (this)
			{
				navEventScope = eventScopes[(int)triggerEventType];
				if (navEventScope != null && navEventScope.ClearEventScope())
				{
					eventScopes[(int)triggerEventType] = ((getOption == EventScopeGetOption.RemoveIfFound) ? null : NavEventScope.StaleEventScope);
				}
			}
		}
		return navEventScope;
	}

	public NavEventScope GetEventScope(NavTriggerEventIdentity triggerIdentity)
	{
		if (NavEventSubscription.TriggerEventTypeIsFieldBased(triggerIdentity.EventType))
		{
			return GetNamedEventScope(triggerIdentity);
		}
		return GetEventScope(triggerIdentity.EventType);
	}

	public bool IsEventSubscribed(NavTriggerEventType triggerEventType)
	{
		return GetEventScope(triggerEventType)?.HasSubscribers ?? false;
	}

	public bool IsEventSubscribed(NavTriggerEventIdentity eventIdentity)
	{
		return GetEventScope(eventIdentity)?.HasSubscribers ?? false;
	}

	public void FireEvent(NavApplicationObjectBase triggerApplicationObject, NavTriggerEventType triggerEventType, Func<NavTriggerParameter, object> parameterResolver)
	{
		GetEventScope(triggerEventType)?.CheckAndFireTriggerEvents(triggerApplicationObject, parameterResolver);
	}

	public virtual NavEventScope GetNamedEventScope(NavTriggerEventIdentity eventIdentity, EventScopeGetOption getOption = EventScopeGetOption.None)
	{
		if (namedEventScopes == null)
		{
			if (getOption != EventScopeGetOption.CreateIfNotFound)
			{
				return null;
			}
			lock (this)
			{
				if (namedEventScopes == null)
				{
					namedEventScopes = new Dictionary<NavTriggerEventIdentity, NavEventScope>();
				}
			}
		}
		NavEventScope value;
		lock (namedEventScopes)
		{
			bool flag = namedEventScopes.TryGetValue(eventIdentity, out value);
			if (!flag && getOption == EventScopeGetOption.CreateIfNotFound)
			{
				value = new NavEventScope();
				namedEventScopes.Add(eventIdentity, value);
				flag = true;
			}
			if (flag && (getOption == EventScopeGetOption.RemoveIfFound || getOption == EventScopeGetOption.SetStaleIfFound) && value.ClearEventScope())
			{
				if (getOption == EventScopeGetOption.RemoveIfFound)
				{
					namedEventScopes.Remove(eventIdentity);
				}
				else
				{
					namedEventScopes[eventIdentity] = NavEventScope.StaleEventScope;
				}
			}
		}
		return value;
	}
}
