using Microsoft.Diagnostics.Tracing;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavServerTraceHelper
{
	public static bool IsTracingEnabled(EventKeywords keywords)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return NavServerEventSource.Log.IsEnabled(keywords);
	}

	public static NavServerServiceCallTraceSpan EnterSpan(EventTask task, string serverInstanceName, string tenantId, int sessionId, string userName, int objectId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		NavServerServiceCallTraceSpan span = new NavServerServiceCallTraceSpan(task, serverInstanceName, tenantId, sessionId, userName, objectId);
		return EnterSpan(span);
	}

	public static NavServerServiceCallTraceSpan EnterSpan(EventTask task, string serverInstanceName, string tenantId, int sessionId, string userName, ConnectionType connectionType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		NavServerServiceCallTraceSpan span = new NavServerServiceCallTraceSpan(task, serverInstanceName, tenantId, sessionId, userName, connectionType.ToString());
		return EnterSpan(span);
	}

	internal static bool IsSqlTracingEnabled()
	{
		return IsTracingEnabled((EventKeywords)2);
	}

	internal static NavServerSqlTraceSpan EnterSpan(EventTask task, string serverInstanceName, string tenantId, int sessionId, string userName, string value = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		NavServerSqlTraceSpan span = new NavServerSqlTraceSpan(task, serverInstanceName, tenantId, sessionId, userName, value);
		return EnterSpan(span);
	}

	private static T EnterSpan<T>(T span) where T : NavServerTraceSpan
	{
		span.Enter();
		return span;
	}
}
