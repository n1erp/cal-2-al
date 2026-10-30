using System;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public abstract class NavAppOperationProcessor<T> where T : class, INavAppOperationContext
{
	public T Context { get; }

	protected NavAppOperationProcessor(T context)
	{
		Context = context ?? throw new ArgumentNullException("context");
	}

	protected void RunActionInSession(bool runInNewSystemSession, Action<NavSession> sessionAction, LanguageSetting language = null)
	{
		try
		{
			if (runInNewSystemSession)
			{
				((ITenantSessionHandler)Context.Tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000013V", Category.Extensions, Verbosity.Normal, "System session transition -- {0}", DiagnosticParameter.SystemMetadata(NavAppTraceHelper.CreateActivityIdString(Context.ActivityId)));
					SetupSessionAndExecuteAction(systemSession, sessionAction);
				}), true, true, false, language);
			}
			else
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001EO", Category.Extensions, Verbosity.Normal, "Running upgrade in ambient session -- {0}", DiagnosticParameter.SystemMetadata(NavAppTraceHelper.CreateActivityIdString(Context.ActivityId)));
				SetupSessionAndExecuteAction(NavCurrentThread.Session, sessionAction);
			}
		}
		catch (Exception ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001WB", Category.Extensions, ex);
			throw new NavUpgradeActivityExecutionException(ex.Message, ex);
		}
	}

	protected void RunActionInSessionWithoutTransaction(Action<NavSession> sessionAction, LanguageSetting language = null)
	{
		try
		{
			((ITenantSessionHandler)Context.Tenant).RunTenantActionInSystemSessionWithoutTransaction((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001EP", Category.Extensions, Verbosity.Normal, "System session transition [NO TRANSACTION] -- {0}", DiagnosticParameter.SystemMetadata(NavAppTraceHelper.CreateActivityIdString(Context.ActivityId)));
				SetupSessionAndExecuteAction(systemSession, sessionAction);
			}), true, true, language);
		}
		catch (Exception ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001EJ", Category.Extensions, ex);
			throw new NavUpgradeActivityExecutionException(ex.Message, ex);
		}
	}

	protected abstract void SetupSessionAndExecuteAction(NavSession systemSession, Action<NavSession> sessionAction);
}
