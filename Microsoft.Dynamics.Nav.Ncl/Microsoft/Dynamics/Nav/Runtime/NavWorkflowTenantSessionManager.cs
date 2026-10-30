using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavWorkflowTenantSessionManager : INavWorkflowTenantSessionManager
{
	private readonly int lcid;

	private readonly ITenantSessionHandler tenantSessionProvider;

	private readonly IList<IReference<NavSession>> activeSessions = new List<IReference<NavSession>>();

	private readonly IList<int> pendingStopOperationSessionIDs = new List<int>();

	public bool SingleSessionMode { get; private set; }

	public Guid ClientSessionId { get; set; }

	public Guid ClientActivityId { get; set; }

	public NavWorkflowTenantSessionManager(NavTenant navTenant, bool useSingleSession, int lcid)
		: this((ITenantSessionHandler)navTenant, useSingleSession, lcid)
	{
	}

	internal NavWorkflowTenantSessionManager(ITenantSessionHandler tenantSessionProvider, bool useSingleSession, int lcid)
	{
		if (tenantSessionProvider == null)
		{
			throw new ArgumentNullException("tenantSessionProvider");
		}
		this.tenantSessionProvider = tenantSessionProvider;
		SingleSessionMode = useSingleSession;
		this.lcid = lcid;
	}

	public void StopSynchronouslyExecutionAtAllSessions()
	{
		lock (this)
		{
			IEnumerable<int> enumerable = from m in activeSessions
				where m.Target != null && !m.Target.IsDisposed
				select m.Target.Id;
			foreach (int item in enumerable)
			{
				StopSynchronouslyExecutionAndDisposeSession(item, removedFromTrackedCollection: false);
			}
			activeSessions.Clear();
		}
	}

	public void StopSynchronouslyExecutionAndDisposeSession(int sessionId, bool removedFromTrackedCollection)
	{
		lock (this)
		{
			IReference<NavSession> reference = activeSessions.FirstOrDefault((IReference<NavSession> next) => next.Target != null && next.Target.Id == sessionId);
			if (reference == null)
			{
				return;
			}
			reference.Target.Diagnostics.SendTraceTag("00000E4", Category.DataUpgrade, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Stop sync workflow session {0}", sessionId);
			if (reference.Target.DisposeInitiated || reference.Target.IsDisposed)
			{
				activeSessions.Remove(reference);
				return;
			}
			pendingStopOperationSessionIDs.Add(sessionId);
			NavSession session = NavCurrentThread.Session;
			NavCurrentThread.Session = reference.Target;
			try
			{
				using (SessionShutdownExecutionListener.SetupSessionShutdownExecutionListener(reference.Target.Tenant))
				{
					ALSession.ALStopSession(DataError.ThrowError, sessionId);
				}
			}
			finally
			{
				if (removedFromTrackedCollection)
				{
					activeSessions.Remove(reference);
				}
				pendingStopOperationSessionIDs.Remove(sessionId);
				NavCurrentThread.Session = session;
			}
		}
	}

	public bool IsPendingStopOperation(int sessionId)
	{
		return pendingStopOperationSessionIDs.Contains(sessionId);
	}

	void INavWorkflowTenantSessionManager.RunActionInSystemSession(Action<NavSession> action)
	{
		NavSession session = GetSession();
		NavSession session2 = NavCurrentThread.Session;
		NavCurrentThread.Session = session;
		try
		{
			session.AccessLock.BeginExclusiveExecution();
			try
			{
				session.BeginTransaction();
				try
				{
					if (!session.IsOpen)
					{
						session.Open(useUserPersonalization: false);
						int culture;
						if (lcid != 0 && (culture = FindSupportedLanguage(lcid, NavEnvironment.DefaultLanguage)) != 0)
						{
							session.SetCulture(CultureInfo.GetCultureInfo(culture).Name);
						}
					}
					using (session.Diagnostics.CreateTraceScope("00000E5", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Workflow action on session {0}", session.Id)), MonitorType.None, Category.Workflow))
					{
						action(session);
					}
				}
				catch (Exception exception)
				{
					session.Diagnostics.SendExceptionTag("00000E6", Category.Workflow, exception);
					session.EndTransaction(commit: false);
					throw;
				}
				if (session.AccessLock.IsStoppingOrDisposed)
				{
					session.Diagnostics.SendTraceTag("00000E7", Category.Workflow, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Workflow is being cancelled on session {0}", session.Id);
					session.EndTransaction(commit: false);
				}
				else
				{
					session.EndTransaction(commit: true);
				}
			}
			finally
			{
				session.AccessLock.EndExclusiveExecution();
			}
		}
		finally
		{
			NavCurrentThread.Session = session2;
			if (!SingleSessionMode)
			{
				StopSynchronouslyExecutionAndDisposeSession(session.Id, removedFromTrackedCollection: true);
			}
		}
	}

	void INavWorkflowTenantSessionManager.RunActionInSystemSessionWithoutTransaction(Action<NavSession> action)
	{
		NavSession session = GetSession();
		NavSession session2 = NavCurrentThread.Session;
		NavCurrentThread.Session = session;
		try
		{
			session.AccessLock.BeginExclusiveExecution();
			try
			{
				try
				{
					if (!session.IsOpen)
					{
						session.Open(useUserPersonalization: false);
						int culture;
						if (lcid != 0 && (culture = FindSupportedLanguage(lcid, NavEnvironment.DefaultLanguage)) != 0)
						{
							session.SetCulture(CultureInfo.GetCultureInfo(culture).Name);
						}
					}
					using (session.Diagnostics.CreateTraceScope("00001F4", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Without transaction workflow action on session {0}", session.Id)), MonitorType.None, Category.Workflow))
					{
						action(session);
					}
				}
				catch (Exception exception) when (session.Diagnostics.SendExceptionTagExceptionFilter("00001F2", Category.Workflow, exception))
				{
					throw;
				}
				if (session.AccessLock.IsStoppingOrDisposed)
				{
					session.Diagnostics.SendTraceTag("00001F3", Category.Workflow, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Without transaction workflow is being cancelled on session {0}", session.Id);
				}
			}
			finally
			{
				session.AccessLock.EndExclusiveExecution();
			}
		}
		finally
		{
			NavCurrentThread.Session = session2;
			if (!SingleSessionMode)
			{
				StopSynchronouslyExecutionAndDisposeSession(session.Id, removedFromTrackedCollection: true);
			}
		}
	}

	internal virtual int FindSupportedLanguage(int requestedLcid, int defaultLcid)
	{
		return NavEnvironment.FindSupportedLanguage(requestedLcid, defaultLcid);
	}

	public void CreateTenantExclusiveActionScope()
	{
		NavSession session = GetSession();
		if (session == null)
		{
			throw new NavNCLExclusiveActionException(string.Format(CultureInfo.CurrentCulture, "Upgrade data workflow failed to get exclusive access because session is null."));
		}
		session.CreateExclusiveTenantActionScope();
	}

	public void StartSingleSessionMode()
	{
		StopSynchronouslyExecutionAtAllSessions();
		SingleSessionMode = true;
	}

	public void StopSingleSessionMode()
	{
		SingleSessionMode = false;
		StopSynchronouslyExecutionAtAllSessions();
	}

	private void RemoveDisposedSessions()
	{
		lock (this)
		{
			IEnumerable<IReference<NavSession>> enumerable = activeSessions.Where((IReference<NavSession> next) => next.Target == null || next.Target.IsDisposed || next.Target.DisposeInitiated);
			foreach (IReference<NavSession> item in enumerable)
			{
				activeSessions.Remove(item);
			}
		}
	}

	private NavSession AddSession()
	{
		lock (this)
		{
			NavSession navSession = tenantSessionProvider.CreateSystemSession();
			navSession.ClientCallbackOverride = new HeadlessClientCallback(ProductNames.ServerServiceDisplayName, navSession);
			activeSessions.Add(navSession.GetReference());
			return navSession;
		}
	}

	private NavSession GetSession()
	{
		lock (this)
		{
			RemoveDisposedSessions();
			if (!SingleSessionMode || !activeSessions.Any())
			{
				return AddSession();
			}
			return activeSessions.First().Target;
		}
	}
}
