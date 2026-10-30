using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

internal class NavPageTriggerEventHandler : NavTriggerEventHandler
{
	private static readonly List<NavTriggerEventInfo> staticTriggerEventInfos = (from mi in typeof(NavPageTriggerEventHandler).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		where mi.IsDefined(typeof(NavEventAttribute))
		select new NavTriggerEventInfo(mi)).ToList();

	private static readonly Dictionary<string, NavEventPublisherMethodInfo> staticTriggerMethodInfoByMethodName = staticTriggerEventInfos.ToDictionary((NavTriggerEventInfo ei) => ei.TriggerEventMethod.Name, (NavTriggerEventInfo tei) => new NavEventPublisherMethodInfo(tei.TriggerEventMethod), StringComparer.OrdinalIgnoreCase);

	public static IReadOnlyList<NavTriggerEventInfo> StaticTriggerEventInfos => staticTriggerEventInfos;

	public new NCLMetaForm Parent => (NCLMetaForm)base.Parent;

	public override IReadOnlyList<NavTriggerEventInfo> TriggerEventInfos => staticTriggerEventInfos;

	public static NavEventPublisherMethodInfo FindStaticTriggerMethodInfoByName(string name)
	{
		if (staticTriggerMethodInfoByMethodName.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public NavPageTriggerEventHandler(NCLMetaForm parent)
		: base(parent)
	{
	}

	public override NavEventScope GetEventScope(NavTriggerEventType triggerEventType, EventScopeGetOption getOption = EventScopeGetOption.None)
	{
		if (triggerEventType == NavTriggerEventType.OnBeforeValidateEvent || triggerEventType == NavTriggerEventType.OnAfterValidateEvent || triggerEventType == NavTriggerEventType.OnBeforeActionEvent || triggerEventType == NavTriggerEventType.OnAfterActionEvent)
		{
			throw new InvalidOperationException();
		}
		return base.GetEventScope(triggerEventType, getOption);
	}

	[NavEvent(NavTriggerEventType.OnOpenPageEvent)]
	public void OnOpenPageEvent(NavForm page, INavRecordHandle rec)
	{
		FireEvent(page, NavTriggerEventType.OnOpenPageEvent, (NavTriggerParameter ntp) =>
		{
			if (ntp == NavTriggerParameter.Rec)
			{
				return rec;
			}
			throw new InvalidOperationException();
		});
	}

	[NavEvent(NavTriggerEventType.OnClosePageEvent)]
	public void OnClosePageEvent(NavForm page, INavRecordHandle rec)
	{
		FireEvent(page, NavTriggerEventType.OnClosePageEvent, (NavTriggerParameter ntp) =>
		{
			if (ntp == NavTriggerParameter.Rec)
			{
				return rec;
			}
			throw new InvalidOperationException();
		});
	}

	[NavEvent(NavTriggerEventType.OnAfterGetRecordEvent)]
	public void OnAfterGetRecordEvent(NavForm page, INavRecordHandle rec)
	{
		FireEvent(page, NavTriggerEventType.OnAfterGetRecordEvent, (NavTriggerParameter ntp) =>
		{
			if (ntp == NavTriggerParameter.Rec)
			{
				return rec;
			}
			throw new InvalidOperationException();
		});
	}

	[NavEvent(NavTriggerEventType.OnAfterGetCurrRecordEvent)]
	public void OnAfterGetCurrRecordEvent(NavForm page, INavRecordHandle rec)
	{
		FireEvent(page, NavTriggerEventType.OnAfterGetCurrRecordEvent, (NavTriggerParameter ntp) =>
		{
			if (ntp == NavTriggerParameter.Rec)
			{
				return rec;
			}
			throw new InvalidOperationException();
		});
	}

	[NavEvent(NavTriggerEventType.OnNewRecordEvent)]
	public void OnNewRecordEvent(NavForm page, INavRecordHandle rec, bool belowxRec, INavRecordHandle xRec)
	{
		FireEvent(page, NavTriggerEventType.OnNewRecordEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.XRec => xRec, 
			NavTriggerParameter.BelowXRec => belowxRec, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnInsertRecordEvent)]
	public void OnInsertRecordEvent(NavForm page, INavRecordHandle rec, bool belowxRec, INavRecordHandle xRec, ByRef<bool> allowInsert)
	{
		FireEvent(page, NavTriggerEventType.OnInsertRecordEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.XRec => xRec, 
			NavTriggerParameter.BelowXRec => belowxRec, 
			NavTriggerParameter.AllowInsert => allowInsert, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnModifyRecordEvent)]
	public void OnModifyRecordEvent(NavForm page, INavRecordHandle rec, INavRecordHandle xRec, ByRef<bool> allowModify)
	{
		FireEvent(page, NavTriggerEventType.OnModifyRecordEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.XRec => xRec, 
			NavTriggerParameter.AllowModify => allowModify, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnDeleteRecordEvent)]
	public void OnDeleteRecordEvent(NavForm page, INavRecordHandle rec, ByRef<bool> allowDelete)
	{
		FireEvent(page, NavTriggerEventType.OnDeleteRecordEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.AllowDelete => allowDelete, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnQueryClosePageEvent)]
	public void OnQueryClosePageEvent(NavForm page, INavRecordHandle rec, FormResult closeAction, ByRef<bool> allowClose)
	{
		FireEvent(page, NavTriggerEventType.OnQueryClosePageEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Rec => (object)rec, 
			NavTriggerParameter.CloseAction => closeAction, 
			NavTriggerParameter.AllowClose => allowClose, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnBeforeValidateEvent)]
	public void OnBeforeValidateEvent(NavForm page, INavRecordHandle rec, INavRecordHandle xRec)
	{
	}

	[NavEvent(NavTriggerEventType.OnAfterValidateEvent)]
	public void OnAfterValidateEvent(NavForm page, INavRecordHandle rec, INavRecordHandle xRec)
	{
	}

	internal void FireOnValidateEvent(NavTriggerEventIdentity triggerIdentity, NavForm page, INavRecordHandle rec, INavRecordHandle xRec)
	{
		GetNamedEventScope(triggerIdentity)?.CheckAndFireTriggerEvents(page, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Page => (object)page, 
			NavTriggerParameter.Rec => rec, 
			NavTriggerParameter.XRec => xRec, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnBeforeActionEvent)]
	public void OnBeforeActionEvent(NavForm page, INavRecordHandle rec)
	{
	}

	[NavEvent(NavTriggerEventType.OnAfterActionEvent)]
	public void OnAfterActionEvent(NavForm page, INavRecordHandle rec)
	{
	}

	[NavEvent(NavTriggerEventType.OnPageBackgroundTaskCompletedEvent)]
	public void OnPageBackgroundTaskCompletedEvent(NavForm page, int taskId, NavDictionary<NavText, NavText> results)
	{
		FireEvent(page, NavTriggerEventType.OnPageBackgroundTaskCompletedEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.TaskId => (object)taskId, 
			NavTriggerParameter.Results => results, 
			_ => throw new InvalidOperationException(), 
		});
	}

	[NavEvent(NavTriggerEventType.OnPageBackgroundTaskErrorEvent)]
	public void OnPageBackgroundTaskErrorEvent(NavForm page, int taskId, NavText errorCode, NavText errorText, NavText errorCallstack)
	{
		FireEvent(page, NavTriggerEventType.OnPageBackgroundTaskErrorEvent, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.TaskId => (object)taskId, 
			NavTriggerParameter.ErrorCode => errorCode, 
			NavTriggerParameter.ErrorText => errorText, 
			NavTriggerParameter.ErrorCallStack => errorCallstack, 
			_ => throw new InvalidOperationException(), 
		});
	}

	internal void FireOnActionEvent(NavTriggerEventIdentity triggerIdentity, NavForm page, INavRecordHandle rec)
	{
		GetNamedEventScope(triggerIdentity)?.CheckAndFireTriggerEvents(page, (NavTriggerParameter ntp) => ntp switch
		{
			NavTriggerParameter.Page => (object)page, 
			NavTriggerParameter.Rec => rec, 
			_ => throw new InvalidOperationException(), 
		});
	}
}
