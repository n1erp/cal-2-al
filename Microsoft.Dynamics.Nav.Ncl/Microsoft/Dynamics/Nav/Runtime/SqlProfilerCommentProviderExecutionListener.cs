using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SqlProfilerCommentProviderExecutionListener : TreeObject, IExecutionListener
{
	private NavSession owningNavSession;

	public SqlProfilerCommentProviderExecutionListener(NavSession navSession)
		: base(navSession)
	{
		owningNavSession = navSession;
	}

	public void ProcessExit(NavMethodScope methodScope)
	{
		NavCurrentThread.Session.SqlProfilerCommentProvider.AlStatementExecutedOrMethodExited = true;
	}

	public void ProcessStatementHit(NavMethodScope methodScope)
	{
		if (methodScope.Session == owningNavSession)
		{
			NavCurrentThread.Session.SqlProfilerCommentProvider.AlStatementExecutedOrMethodExited = true;
		}
	}

	public void ProcessException(NavMethodScope methodScope, Exception exception)
	{
	}

	public bool CanCoexistWithOtherListener(IExecutionListener otherListener)
	{
		return true;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				owningNavSession = null;
				ExecutionListener.RemoveListener(this);
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
