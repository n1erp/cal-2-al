using Microsoft.Diagnostics.Tracing;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavServerSqlTraceSpan : NavServerTraceSpan
{
	private readonly string value;

	private bool isStarted;

	internal NavServerSqlTraceSpan(EventTask task, string serverInstanceName, string tenantId, int sessionId, string userName, string value)
		: base(task, serverInstanceName, tenantId, sessionId, userName)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		this.value = EventPayloadHelper.TruncateIfLarge(value);
	}

	public override void Enter()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected I4, but got Unknown
		if (!isStarted)
		{
			isStarted = true;
			EventTask task = Task;
			switch (task - 1)
			{
			case 1:
				NavServerEventSource.Log.WriteSqlExecuteNonQueryStartEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
				break;
			case 2:
				NavServerEventSource.Log.WriteSqlExecuteReaderStartEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
				break;
			case 0:
				NavServerEventSource.Log.WriteSqlExecuteScalarStartEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
				break;
			case 3:
				NavServerEventSource.Log.WriteSqlReadNextResultStartEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
				break;
			case 4:
				NavServerEventSource.Log.WriteSqlReadNextRowStartEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
				break;
			case 5:
				NavServerEventSource.Log.WriteSqlBeginTransactionStartEvent(TenantId, SessionId, UserName, ServerInstanceName);
				break;
			case 6:
				NavServerEventSource.Log.WriteSqlPrepareStartEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
				break;
			case 7:
				NavServerEventSource.Log.WriteSqlOpenConnectionStartEvent(TenantId, SessionId, UserName, ServerInstanceName);
				break;
			case 8:
				NavServerEventSource.Log.WriteSqlCommitStartEvent(TenantId, SessionId, UserName, ServerInstanceName);
				break;
			case 9:
				NavServerEventSource.Log.WriteSqlRollbackStartEvent(TenantId, SessionId, UserName, ServerInstanceName);
				break;
			}
		}
	}

	public override void Leave()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected I4, but got Unknown
		EventTask task = Task;
		switch (task - 1)
		{
		case 1:
			NavServerEventSource.Log.WriteSqlExecuteNonQueryStopEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
			break;
		case 2:
			NavServerEventSource.Log.WriteSqlExecuteReaderStopEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
			break;
		case 0:
			NavServerEventSource.Log.WriteSqlExecuteScalarStopEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
			break;
		case 3:
			NavServerEventSource.Log.WriteSqlReadNextResultStopEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
			break;
		case 4:
			NavServerEventSource.Log.WriteSqlReadNextRowStopEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
			break;
		case 5:
			NavServerEventSource.Log.WriteSqlBeginTransactionStopEvent(TenantId, SessionId, UserName, ServerInstanceName);
			break;
		case 6:
			NavServerEventSource.Log.WriteSqlPrepareStopEvent(TenantId, SessionId, UserName, value, ServerInstanceName);
			break;
		case 7:
			NavServerEventSource.Log.WriteSqlOpenConnectionStopEvent(TenantId, SessionId, UserName, ServerInstanceName);
			break;
		case 8:
			NavServerEventSource.Log.WriteSqlCommitStopEvent(TenantId, SessionId, UserName, ServerInstanceName);
			break;
		case 9:
			NavServerEventSource.Log.WriteSqlRollbackStopEvent(TenantId, SessionId, UserName, ServerInstanceName);
			break;
		}
	}
}
