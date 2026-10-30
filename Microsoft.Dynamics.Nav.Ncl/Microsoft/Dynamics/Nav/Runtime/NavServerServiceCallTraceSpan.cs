using Microsoft.Diagnostics.Tracing;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavServerServiceCallTraceSpan : NavServerTraceSpan
{
	private readonly int objectId;

	private readonly string connectionType;

	public NavServerServiceCallTraceSpan(EventTask task, string serverInstanceName, string tenantId, int sessionId, string userName, int objectId)
		: base(task, serverInstanceName, tenantId, sessionId, userName)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		this.objectId = objectId;
	}

	public NavServerServiceCallTraceSpan(EventTask task, string serverInstanceName, string tenantId, int sessionId, string userName, string connectionType)
		: base(task, serverInstanceName, tenantId, sessionId, userName)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		this.connectionType = connectionType;
	}

	public override void Enter()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected I4, but got Unknown
		EventTask task = Task;
		switch (task - 14)
		{
		case 0:
			NavServerEventSource.Log.WriteRunQueryStartEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 1:
			NavServerEventSource.Log.WriteRunReportStartEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 2:
			NavServerEventSource.Log.WriteGetPageStartEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 5:
			NavServerEventSource.Log.WriteRunXmlPortStartEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 3:
			NavServerEventSource.Log.WriteInvokeCodeUnitStartEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 4:
			NavServerEventSource.Log.WriteInvokeFormApplicationMethodStartEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 9:
			NavServerEventSource.Log.WriteCreateServiceSessionStartEvent(TenantId, SessionId, UserName, connectionType, ServerInstanceName);
			break;
		case 10:
			NavServerEventSource.Log.WriteEndServiceSessionStartEvent(TenantId, SessionId, UserName, connectionType, ServerInstanceName);
			break;
		case 6:
		case 7:
		case 8:
			break;
		}
	}

	public override void Leave()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected I4, but got Unknown
		EventTask task = Task;
		switch (task - 14)
		{
		case 0:
			NavServerEventSource.Log.WriteRunQueryStopEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 1:
			NavServerEventSource.Log.WriteRunReportStopEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 2:
			NavServerEventSource.Log.WriteGetPageStopEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 5:
			NavServerEventSource.Log.WriteRunXmlPortStopEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 3:
			NavServerEventSource.Log.WriteInvokeCodeUnitStopEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 4:
			NavServerEventSource.Log.WriteInvokeFormApplicationMethodStopEvent(TenantId, SessionId, UserName, objectId, ServerInstanceName);
			break;
		case 9:
			NavServerEventSource.Log.WriteCreateServiceSessionStopEvent(TenantId, SessionId, UserName, connectionType, ServerInstanceName);
			break;
		case 10:
			NavServerEventSource.Log.WriteEndServiceSessionStopEvent(TenantId, SessionId, UserName, connectionType, ServerInstanceName);
			break;
		case 6:
		case 7:
		case 8:
			break;
		}
	}
}
