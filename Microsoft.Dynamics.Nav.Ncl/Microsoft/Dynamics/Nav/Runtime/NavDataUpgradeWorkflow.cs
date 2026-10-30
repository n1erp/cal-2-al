using System;
using System.Activities;
using System.Activities.Hosting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Workflow;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDataUpgradeWorkflow : NavWorkflow<NavDataUpgradeWorkflowStartRequest, NavDataUpgradeWorkflowResumeRequest, NavDataUpgradeProcessExecutionState>
{
	private NavDataUpgradeWorkflowStartRequest currentStartRequest = new NavDataUpgradeWorkflowStartRequest();

	private INavWorkflowTenantSessionManager dataUpgradeTenantSessionHandler;

	private readonly ITenantSessionHandler tenantSessionHandler;

	private readonly INavDataUpgradeMetadata upgradeMetadata;

	private INavDataUpgradeProgressMonitor upgradeProgressMonitor;

	private Action<NavDataUpgradeProcessExecutionState> OnDone;

	private NavDataUpgradeProcessExecutionState cachedWorkflowStatus;

	public IEnumerable<NavAppJointUpgradeContext> ExtensionUpgradeContexts { get; set; }

	public NavDataUpgradeWorkflow(NavTenant navTenant, INavDataUpgradeMetadata upgradeMetadata)
		: this(navTenant, upgradeMetadata, new NavDataUpgradeProgressMonitor())
	{
	}

	internal NavDataUpgradeWorkflow(ITenantSessionHandler tenantSessionHandler, INavDataUpgradeMetadata upgradeMetadata, INavDataUpgradeProgressMonitor dataUpgradeProgressMonitor)
	{
		this.upgradeMetadata = upgradeMetadata;
		this.tenantSessionHandler = tenantSessionHandler;
		upgradeProgressMonitor = dataUpgradeProgressMonitor;
	}

	public override NavDataUpgradeProcessExecutionState GetWorkflowStatus()
	{
		if (cachedWorkflowStatus != null)
		{
			return cachedWorkflowStatus;
		}
		return NavDataUpgradeProcessExecutionState.Generate(upgradeProgressMonitor.ExecutionDetails, State, tenantSessionHandler.Id, currentStartRequest.SingleTransaction);
	}

	public override void StartWorkflow(NavDataUpgradeWorkflowStartRequest startRequest)
	{
		string content = (NavEnvironment.Instance.Tenants.IsSystemTenantCreatedNoWait ? NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion : string.Empty);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000DY", Category.DataUpgrade, Verbosity.Normal, "Start upgrade workflow on tenant {0}, parallel = {1}, ClientSessionId = {2}, TargetApplicationVersion = {3}", DiagnosticParameter.Oii(startRequest.TenantId), DiagnosticParameter.SystemMetadata(startRequest.ParallelExecutionEnabled), DiagnosticParameter.SystemMetadata(NavDiagnostics.ClientSessionId), DiagnosticParameter.SystemMetadata(content));
		if (State != NavWorkflowState.NotStarted && State != NavWorkflowState.Stopped && State != NavWorkflowState.Completed)
		{
			throw new NavUpgradeProcessExecutionException(Lang.UpgradeProcessAlreadyInProgressError);
		}
		if (startRequest.ParallelExecutionEnabled && startRequest.SingleTransaction)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot use parameter {0} with parameter {1} set to {2}", "SingleTransaction", "ParallelExecutionEnabled", startRequest.ParallelExecutionEnabled));
		}
		IEnumerable<NavAppDataUpgradeActivities> enumerable = new List<NavAppDataUpgradeActivities>();
		List<NavAppJointUpgradeContext> list = new List<NavAppJointUpgradeContext>();
		if (!startRequest.ExcludeExtensions)
		{
			NavTenant tenantById = NavEnvironment.Instance.Tenants.GetTenantById(startRequest.TenantId, includeFailed: true, enablePolling: true);
			Guid activityId = Guid.NewGuid();
			NavAppPackagePoolSelector poolSelector = NavAppPackagePoolSelector.CreateForUpgradeContextFactory(NavEnvironment.Instance.NavAppMetadataRetriever, (NavSqlConnectionScope tenantConScope) => new DatabaseAppSyncTrackingRepository(tenantConScope.Connection.Database, tenantConScope));
			list = new NavAppWorkflowUpgradeContextFactory(poolSelector, NavAppInstallationManagement.GetAppsInstalledForTenantInTenantDatabase, (NavSqlConnectionScope tenantConScope, string tenantId) => new DatabaseUninstalledAppRepository(tenantConScope.Connection.Database, tenantConScope).GetAllForTenant(tenantId), NavGlobal.NavAppGroupResolver).GetOrderedAppContextsForDataUpgrade(tenantById, activityId, skipUninstalledExtensions: true);
			enumerable = NavAppDataUpgradeActivityFactory.CreateActivitiesForNavAppUpgrade(list, startRequest.Companies);
		}
		NavDataUpgradeActivityFactory navDataUpgradeActivityFactory = new NavDataUpgradeActivityFactory(upgradeMetadata);
		Activity workflowDefinition = ((!startRequest.ParallelExecutionEnabled) ? navDataUpgradeActivityFactory.CreateWorkflowForSerialExecutionOfNavUpgrade(startRequest.Companies, startRequest.ContinueOnErrorEnabled, startRequest.SkipCompanyInitialization, startRequest.SingleTransaction, enumerable) : navDataUpgradeActivityFactory.CreateWorkflowForParallelExecutionOfNavUpgrade(startRequest.Companies, startRequest.ContinueOnErrorEnabled, startRequest.SkipCompanyInitialization, enumerable));
		OnDone = startRequest.OnDone;
		WorkflowHost = new WorkflowApplication(workflowDefinition);
		AddExtension(new NavDataUpgradeScope());
		AddExtension(upgradeMetadata);
		AddExtension(new NavUpgradeCodeunitObjectFactory());
		AddExtension(upgradeProgressMonitor);
		dataUpgradeTenantSessionHandler = new NavWorkflowTenantSessionManager(tenantSessionHandler, !startRequest.ParallelExecutionEnabled, startRequest.LanguageLcid);
		dataUpgradeTenantSessionHandler.ClientSessionId = NavDiagnostics.ClientSessionId;
		dataUpgradeTenantSessionHandler.ClientActivityId = NavDiagnostics.ClientActivityId;
		AddExtension(dataUpgradeTenantSessionHandler);
		AddExtension(new NavSystemCodeunitUpgradeEventExecutorProvider());
		if (dataUpgradeTenantSessionHandler.SingleSessionMode && startRequest.StopRunningSessions)
		{
			dataUpgradeTenantSessionHandler.CreateTenantExclusiveActionScope();
			State = NavWorkflowState.InProgress;
		}
		IEnumerable<Tuple<int, string>> methodsToExecutePerCompany = upgradeMetadata.CheckPreconditionMethods.Union(upgradeMetadata.UpgradePerCompanyMethods);
		IEnumerable<Tuple<int, string>> methodsToExecutePerDatabase = upgradeMetadata.UpgradePerDatabaseMethods.AsEnumerable();
		upgradeProgressMonitor.StartMonitoring(methodsToExecutePerCompany, methodsToExecutePerDatabase, enumerable, upgradeMetadata.CompanyInitializationMethod, startRequest);
		startRequest.ClientSessionId = NavDiagnostics.ClientSessionId;
		currentStartRequest = startRequest;
		if (list.Any())
		{
			NavEnvironment.Instance.NavAppGroupUpdater.PrepareAppGroup(list.First().TargetGroup);
		}
		ExtensionUpgradeContexts = list?.ToImmutableArray() ?? default(ImmutableArray<NavAppJointUpgradeContext>);
		cachedWorkflowStatus = null;
		base.StartWorkflow(startRequest);
	}

	public override void StopWorkflow()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000DZ", Category.DataUpgrade, Verbosity.Normal, "Stop upgrade workflow on tenant {0}", DiagnosticParameter.Oii(currentStartRequest.TenantId));
		if (State == NavWorkflowState.NotStarted || State == NavWorkflowState.Stopped || State == NavWorkflowState.Completed)
		{
			throw new NavUpgradeProcessExecutionException(Lang.UpgradeProcessNotFoundError);
		}
		Cancel();
	}

	public override void ResumeWorkflow(NavDataUpgradeWorkflowResumeRequest resumeRequest)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000E0", Category.DataUpgrade, Verbosity.Normal, "Resume upgrade workflow on tenant {0}", DiagnosticParameter.Oii(currentStartRequest.TenantId));
		if (State != NavWorkflowState.Suspended)
		{
			throw new NavUpgradeProcessExecutionException(Lang.UpgradeProcessInSuspendStateNotFound);
		}
		if (resumeRequest.ResumeAllMode)
		{
			ResumeAll();
		}
		else
		{
			Resume(resumeRequest.CodeunitId, resumeRequest.MethodName, resumeRequest.CompanyName);
		}
	}

	private void CacheWorkflowStatusAndClearActivities(NavDataUpgradeProcessExecutionState status)
	{
		if ((status.State == NavWorkflowState.Completed || status.State == NavWorkflowState.Stopped) && OnDone != null)
		{
			cachedWorkflowStatus = status;
			OnDone(status);
			upgradeProgressMonitor = new NavDataUpgradeProgressMonitor();
		}
	}

	protected override void OnAborted(WorkflowApplicationAbortedEventArgs args)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000VH", Category.DataUpgrade, Verbosity.Error, "Aborted upgrade workflow on tenant {0}, clientSessionId = {1}", DiagnosticParameter.Oii(currentStartRequest.TenantId), DiagnosticParameter.SystemMetadata(currentStartRequest.ClientSessionId));
		try
		{
			dataUpgradeTenantSessionHandler.StopSynchronouslyExecutionAtAllSessions();
		}
		finally
		{
			upgradeProgressMonitor.StopMonitoring();
			State = NavWorkflowState.Stopped;
			CacheWorkflowStatusAndClearActivities(GetWorkflowStatus());
		}
		base.OnAborted(args);
	}

	protected override void OnCompleted(WorkflowApplicationCompletedEventArgs args)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000E1", Category.DataUpgrade, Verbosity.Normal, "Complete upgrade workflow on tenant {0}, clientSessionId = {1}, completionState = {2}", DiagnosticParameter.Oii(currentStartRequest.TenantId), DiagnosticParameter.SystemMetadata(currentStartRequest.ClientSessionId), DiagnosticParameter.SystemMetadata(args.CompletionState));
		ActivityInstanceState completionState = args.CompletionState;
		try
		{
			try
			{
				dataUpgradeTenantSessionHandler.RunActionInSystemSession((NavSession sessionToUse) =>
				{
					if (completionState != ActivityInstanceState.Canceled && completionState != ActivityInstanceState.Faulted)
					{
						sessionToUse.Tenant.RefreshState();
					}
					else
					{
						sessionToUse.Diagnostics.SendTraceTag("00000Z5", Category.DataUpgrade, Verbosity.Error, "Complete upgrade workflow on tenant {0}, clientSessionId = {1} completed with workflow state {2}. TenantDataVersion NOT upgraded from {3} to {4}", DiagnosticParameter.Oii(currentStartRequest.TenantId), DiagnosticParameter.SystemMetadata(currentStartRequest.ClientSessionId), DiagnosticParameter.SystemMetadata(args.CompletionState), DiagnosticParameter.SystemMetadata(sessionToUse.Database.TenantProperties.TenantDataVersion), DiagnosticParameter.SystemMetadata(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion));
					}
				});
			}
			catch (NavBaseException exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000VY", Category.Workflow, exception);
			}
			dataUpgradeTenantSessionHandler.StopSynchronouslyExecutionAtAllSessions();
		}
		finally
		{
			upgradeProgressMonitor.StopMonitoring();
			if (completionState == ActivityInstanceState.Canceled || completionState == ActivityInstanceState.Faulted)
			{
				State = NavWorkflowState.Stopped;
			}
			else
			{
				State = NavWorkflowState.Completed;
			}
			CacheWorkflowStatusAndClearActivities(GetWorkflowStatus());
		}
		base.OnCompleted(args);
	}

	protected override void OnIdle(WorkflowApplicationIdleEventArgs args)
	{
		if (!args.Bookmarks.Any())
		{
			return;
		}
		if (upgradeProgressMonitor.ExecutionDetails.All((NavDataUpgradeExecutionDetails next) => next.State != NavDataUpgradeMethodExecutionState.InProgress))
		{
			State = NavWorkflowState.Suspended;
		}
		if (currentStartRequest.ParallelExecutionEnabled && !currentStartRequest.ContinueOnErrorEnabled)
		{
			IEnumerable<NavDataUpgradeExecutionDetails> enumerable = upgradeProgressMonitor.ExecutionDetails.Where((NavDataUpgradeExecutionDetails next) => next.State == NavDataUpgradeMethodExecutionState.InProgress);
			foreach (NavDataUpgradeExecutionDetails item in enumerable)
			{
				dataUpgradeTenantSessionHandler.StopSynchronouslyExecutionAndDisposeSession(item.SessionId, removedFromTrackedCollection: true);
			}
		}
		else if (!currentStartRequest.SingleTransaction && !currentStartRequest.ParallelExecutionEnabled)
		{
			dataUpgradeTenantSessionHandler.StopSynchronouslyExecutionAtAllSessions();
		}
		base.OnIdle(args);
	}

	protected override UnhandledExceptionAction OnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs args)
	{
		if (args != null)
		{
			try
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000023L", Category.DataUpgrade, Verbosity.Error, "Unhanded excpetion during upgrade workflow on tenant {0}, clientSessionId = {1}, exceptionSourceInstanceId: {2}, exceptionSource activity: {3}, workflow instanceId: {4}, unhandledException: {5}", DiagnosticParameter.Oii(currentStartRequest?.TenantId), DiagnosticParameter.SystemMetadata(currentStartRequest?.ClientSessionId), DiagnosticParameter.SystemMetadata(args.ExceptionSourceInstanceId), DiagnosticParameter.SystemMetadata(args.ExceptionSource?.DisplayName), DiagnosticParameter.SystemMetadata(args.InstanceId), DiagnosticParameter.SystemMetadata(args.UnhandledException?.Message));
			}
			catch (Exception exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000023O", Category.DataUpgrade, exception);
			}
		}
		else
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000023P", Category.DataUpgrade, Verbosity.Error, PrivacyClassification.SystemMetadata, "Empty args for unhanded excpetion during upgrade workflow.");
		}
		try
		{
			dataUpgradeTenantSessionHandler.StopSynchronouslyExecutionAtAllSessions();
		}
		finally
		{
			State = NavWorkflowState.Stopped;
			upgradeProgressMonitor.StopMonitoring();
			CacheWorkflowStatusAndClearActivities(GetWorkflowStatus());
		}
		return UnhandledExceptionAction.Cancel;
	}

	private void Resume(int codeunitId, string methodName, string companyName)
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForCodeunitExecutionActivity(codeunitId, methodName, companyName);
		Resume(bookmarkName);
	}

	private void Resume(string bookmarkName)
	{
		if (ResumeBookmark(bookmarkName, new object()) != BookmarkResumptionResult.Success)
		{
			throw new NavUpgradeProcessExecutionException(Lang.SuspendedExecutionOfSpecificUpgradeCodeunitMethodNotFound);
		}
		State = NavWorkflowState.InProgress;
	}

	private void ResumeAll()
	{
		ReadOnlyCollection<BookmarkInfo> bookmarks;
		try
		{
			bookmarks = GetBookmarks();
		}
		catch (WorkflowApplicationCompletedException)
		{
			throw new NavUpgradeProcessExecutionException(Lang.UpgradeProcessAlreadyCompleted);
		}
		foreach (BookmarkInfo item in bookmarks)
		{
			Resume(item.BookmarkName);
		}
	}
}
