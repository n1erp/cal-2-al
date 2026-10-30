using System;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ClientStopExecutionListener : IExecutionListener
{
	public NavSession Session { get; private set; }

	public static void Activate(NavSession navSession)
	{
		ClientStopExecutionListener clientStopExecutionListener = new ClientStopExecutionListener(navSession);
		clientStopExecutionListener.DoActivate();
	}

	internal ClientStopExecutionListener(NavSession navSession)
	{
		if (navSession == null)
		{
			throw new ArgumentNullException("navSession");
		}
		Session = navSession;
	}

	public bool CheckForDeactivate(NavSession navSession)
	{
		if (navSession != Session)
		{
			return false;
		}
		ExecutionListener.RemoveListener(this);
		Session.ClientStopExecutionListener = null;
		Session = null;
		return true;
	}

	private void DoActivate()
	{
		if (Session.ClientStopExecutionListener == null)
		{
			Session.ClientStopExecutionListener = this;
			ExecutionListener.AddListener(this);
		}
	}

	public void ProcessExit(NavMethodScope methodScope)
	{
		if (CheckForDeactivate(methodScope.Session))
		{
			throw new NavNclClientStopException(Lang.NclAlExecutionStoppedByClient);
		}
	}

	public void ProcessStatementHit(NavMethodScope methodScope)
	{
		if (CheckForDeactivate(methodScope.Session))
		{
			throw new NavNclClientStopException(Lang.NclAlExecutionStoppedByClient);
		}
	}

	public void ProcessException(NavMethodScope methodScope, Exception exception)
	{
		CheckForDeactivate(methodScope.Session);
	}

	public bool CanCoexistWithOtherListener(IExecutionListener otherListener)
	{
		return true;
	}
}
