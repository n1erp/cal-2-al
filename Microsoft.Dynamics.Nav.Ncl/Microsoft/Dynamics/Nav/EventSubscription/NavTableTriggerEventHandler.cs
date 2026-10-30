using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

internal class NavTableTriggerEventHandler : NavTriggerEventHandler
{
	private static readonly List<NavTriggerEventInfo> staticTriggerEventInfos = (from mi in typeof(NavTableTriggerEventHandler).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		where mi.IsDefined(typeof(NavEventAttribute))
		select new NavTriggerEventInfo(mi)).ToList();

	private static readonly Dictionary<string, NavEventPublisherMethodInfo> staticTriggerMethodInfoByMethodName = staticTriggerEventInfos.ToDictionary((NavTriggerEventInfo ei) => ei.TriggerEventMethod.Name, (NavTriggerEventInfo tei) => new NavEventPublisherMethodInfo(tei.TriggerEventMethod), StringComparer.OrdinalIgnoreCase);

	public static IReadOnlyList<NavTriggerEventInfo> StaticTriggerEventInfos => staticTriggerEventInfos;

	public override IReadOnlyList<NavTriggerEventInfo> TriggerEventInfos => staticTriggerEventInfos;

	public static NavEventPublisherMethodInfo FindStaticTriggerMethodInfoByName(string name)
	{
		if (staticTriggerMethodInfoByMethodName.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public NavTableTriggerEventHandler(NCLMetaApplicationObject parent)
		: base(parent)
	{
	}

	public override NavEventScope GetEventScope(NavTriggerEventType triggerEventType, EventScopeGetOption getOption = EventScopeGetOption.None)
	{
		if (triggerEventType == NavTriggerEventType.OnBeforeValidateEvent || triggerEventType == NavTriggerEventType.OnAfterValidateEvent)
		{
			throw new InvalidOperationException();
		}
		return base.GetEventScope(triggerEventType, getOption);
	}

	public override NavEventScope GetNamedEventScope(NavTriggerEventIdentity eventIdentity, EventScopeGetOption getOption = EventScopeGetOption.None)
	{
		throw new InvalidOperationException();
	}

	[NavEvent(NavTriggerEventType.OnBeforeInsertEvent)]
	public void OnBeforeInsertEvent(INavRecordHandle rec, bool runTrigger)
	{
		FireEvent(rec.Target, NavTriggerEventType.OnBeforeInsertEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.RunTrigger => runTrigger, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnAfterInsertEvent)]
	public void OnAfterInsertEvent(INavRecordHandle rec, bool runTrigger)
	{
		FireEvent(rec.Target, NavTriggerEventType.OnAfterInsertEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.RunTrigger => runTrigger, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnBeforeModifyEvent)]
	public void OnBeforeModifyEvent(INavRecordHandle rec, INavRecordHandle xRec, bool runTrigger)
	{
		FireEvent(rec.Target, NavTriggerEventType.OnBeforeModifyEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.XRec => xRec, 
			NavTriggerParameter.RunTrigger => runTrigger, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnAfterModifyEvent)]
	public void OnAfterModifyEvent(INavRecordHandle rec, INavRecordHandle xRec, bool runTrigger)
	{
		FireEvent(rec.Target, NavTriggerEventType.OnAfterModifyEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.XRec => xRec, 
			NavTriggerParameter.RunTrigger => runTrigger, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnBeforeDeleteEvent)]
	public void OnBeforeDeleteEvent(INavRecordHandle rec, bool runTrigger)
	{
		FireEvent(rec.Target, NavTriggerEventType.OnBeforeDeleteEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.RunTrigger => runTrigger, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnAfterDeleteEvent)]
	public void OnAfterDeleteEvent(INavRecordHandle rec, bool runTrigger)
	{
		FireEvent(rec.Target, NavTriggerEventType.OnAfterDeleteEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.RunTrigger => runTrigger, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnBeforeRenameEvent)]
	public void OnBeforeRenameEvent(INavRecordHandle rec, INavRecordHandle xRec, bool runTrigger)
	{
		FireEvent(rec.Target, NavTriggerEventType.OnBeforeRenameEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.XRec => xRec, 
			NavTriggerParameter.RunTrigger => runTrigger, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnAfterRenameEvent)]
	public void OnAfterRenameEvent(INavRecordHandle rec, INavRecordHandle xRec, bool runTrigger)
	{
		FireEvent(rec.Target, NavTriggerEventType.OnAfterRenameEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.XRec => xRec, 
			NavTriggerParameter.RunTrigger => runTrigger, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnBeforeValidateEvent)]
	public void OnBeforeValidateEvent(INavRecordHandle rec, INavRecordHandle xRec, int currFieldNo)
	{
	}

	[NavEvent(NavTriggerEventType.OnAfterValidateEvent)]
	public void OnAfterValidateEvent(INavRecordHandle rec, INavRecordHandle xRec, int currFieldNo)
	{
	}

	internal static void FireOnValidateEvent(NavTriggerEventType triggerEventType, INavRecordHandle rec, NCLMetaField metaField)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		metaField.GetEventScope(triggerEventType)?.CheckAndFireTriggerEvents(rec.Target, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.XRec => rec.Target.OldRecord, 
			NavTriggerParameter.CurrFieldNo => rec.Target.CurrFieldNo, 
			_ => throw new InvalidOperationException(), 
		});
	}
}
