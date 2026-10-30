using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[SourceSpans(new long[] { 0L })]
public class ApplicationObjectRootScope : NavMethodScope
{
	public override bool IsInsideTryCall => false;

	public override NavApplicationObjectBase ApplicationObject { get; }

	protected override uint RawScopeId
	{
		get
		{
			return 0u;
		}
		set
		{
		}
	}

	public static T AddApplicationObjectRootScope<T>(NavApplicationObjectBase applicationObject, Func<T> action)
	{
		if (IsAlreadyInRootScope(applicationObject))
		{
			return action();
		}
		using NavMethodScope navMethodScope = new ApplicationObjectRootScope(applicationObject);
		try
		{
			return action();
		}
		catch (Exception exception) when (navMethodScope.ProcessException(exception))
		{
		}
		return default;
	}

	public static bool IsAlreadyInRootScope(NavApplicationObjectBase applicationObject)
	{
		NavMethodScope currentMethodScope = NavCurrentThread.Session.CurrentMethodScope;
		if (applicationObject != null)
		{
			if (currentMethodScope is ApplicationObjectRootScope && currentMethodScope.ApplicationObject.NclType == applicationObject.NclType)
			{
				return currentMethodScope.ApplicationObject.ObjectId == applicationObject.ObjectId;
			}
			return false;
		}
		return true;
	}

	public static void AddApplicationObjectRootScope(NavApplicationObjectBase applicationObject, Action action)
	{
		if (IsAlreadyInRootScope(applicationObject))
		{
			action();
			return;
		}
		using NavMethodScope navMethodScope = new ApplicationObjectRootScope(applicationObject);
		try
		{
			action();
		}
		catch (Exception exception) when (navMethodScope.ProcessException(exception))
		{
		}
	}

	internal ApplicationObjectRootScope(NavApplicationObjectBase applicationObject)
		: base(applicationObject, eventSource: true)
	{
		ApplicationObject = applicationObject;
	}
}
