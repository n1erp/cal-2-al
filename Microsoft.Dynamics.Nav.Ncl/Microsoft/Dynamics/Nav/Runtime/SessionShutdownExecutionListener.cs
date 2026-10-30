using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class SessionShutdownExecutionListener : IExecutionListener, IDisposable
{
	private readonly int sessionId;

	private readonly string tenantId;

	public SessionShutdownExecutionListener(int sessionId, string tenantId)
	{
		this.sessionId = sessionId;
		this.tenantId = tenantId;
	}

	public void ProcessExit(NavMethodScope methodScope)
	{
	}

	public void ProcessStatementHit(NavMethodScope methodScope)
	{
		NavSession session = methodScope.Session;
		if (sessionId > 0)
		{
			if (sessionId != session.Id)
			{
				return;
			}
		}
		else if (tenantId != null && !string.Equals(tenantId, session.Tenant.Id, StringComparison.Ordinal))
		{
			return;
		}
		session.ThrowSessionTerminatedExceptionIfStopping();
	}

	public void ProcessException(NavMethodScope methodScope, Exception exception)
	{
	}

	public bool CanCoexistWithOtherListener(IExecutionListener otherListener)
	{
		return true;
	}

	public void Dispose()
	{
		ExecutionListener.RemoveListener(this);
	}

	public static IDisposable SetupSessionShutdownExecutionListener(NavSession session, ShutdownScope shutdownScope)
	{
		if (shutdownScope != ShutdownScope.SingleSession)
		{
			return null;
		}
		return Register(session.Id, null);
	}

	public static SessionShutdownExecutionListener SetupSessionShutdownExecutionListener(NavTenant tenant)
	{
		return Register(0, tenant.Id);
	}

	public static SessionShutdownExecutionListener SetupSessionShutdownExecutionListener()
	{
		return Register(0, null);
	}

	private static SessionShutdownExecutionListener Register(int sessionId, string tenantId)
	{
		SessionShutdownExecutionListener sessionShutdownExecutionListener = new SessionShutdownExecutionListener(sessionId, tenantId);
		if (!ExecutionListener.AddListener(sessionShutdownExecutionListener))
		{
			return null;
		}
		return sessionShutdownExecutionListener;
	}
}
