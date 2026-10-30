using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDataUpgradeProgressMonitor : INavDataUpgradeProgressMonitor, IExecutionListener
{
	private bool isMonitoring;

	private NavDataUpgradeWorkflowStartRequest startRequest;

	private readonly List<string> upgradePerDatabaseFunctions = new List<string>();

	private readonly IList<NavDataUpgradeExecutionDetails> executionDetails = new List<NavDataUpgradeExecutionDetails>();

	public IReadOnlyCollection<NavDataUpgradeExecutionDetails> ExecutionDetails => new ReadOnlyCollection<NavDataUpgradeExecutionDetails>(executionDetails);

	public void ReportError(int codeunitId, string methodName, string companyName, Exception exception)
	{
		string content;
		if (companyName == null)
		{
			content = "null";
		}
		else
		{
			content = (string.IsNullOrEmpty(companyName) ? "empty" : companyName);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000DX", Category.DataUpgrade, Verbosity.Error, "Application upgrade error codeunit id = {0}, method = {1}, company = {2}, exception = {3}", DiagnosticParameter.SystemMetadata(codeunitId), DiagnosticParameter.SystemMetadata(methodName), DiagnosticParameter.Oii(content), DiagnosticParameter.Oii(exception?.ToString()));
		NavDataUpgradeMethodExecutionDetails entryToUpdate = FindMatchingCodeunitMethodAmongMonitoredEntries(codeunitId, methodName, companyName);
		bool shouldFail = !startRequest.ParallelExecutionEnabled && startRequest.ContinueOnErrorEnabled;
		UpdateEntryError(entryToUpdate, exception, shouldFail, postUpgrade: false);
	}

	public void ReportEventError(NavUpgradeActivityStep upgradeStep, string companyName, Exception exception, bool postUpgrade)
	{
		string content;
		if (companyName == null)
		{
			content = "null";
		}
		else
		{
			content = (string.IsNullOrEmpty(companyName) ? "empty" : companyName);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001FA", Category.DataUpgrade, Verbosity.Error, "Extension upgrade error in '{0}', upgrade step = {1}, company = {2}, exception = {3}", DiagnosticParameter.SystemMetadata(upgradeStep.ModuleName), DiagnosticParameter.SystemMetadata(upgradeStep.StepName), DiagnosticParameter.Oii(content), DiagnosticParameter.Oii(exception?.ToString()));
		NavDataUpgradeEventExecutionDetails entryToUpdate = FindMatchingAppEventAmongMonitoredEntries(upgradeStep, companyName);
		UpdateEntryError(entryToUpdate, exception, shouldFail: true, postUpgrade);
	}

	public void StartMonitoring(IEnumerable<Tuple<int, string>> methodsToExecutePerCompany, IEnumerable<Tuple<int, string>> methodsToExecutePerDatabase, IEnumerable<NavAppDataUpgradeActivities> appUpgradeActivities, Tuple<int, string> companyInitializationMethod, NavDataUpgradeWorkflowStartRequest request)
	{
		startRequest = request;
		executionDetails.Clear();
		foreach (string company in startRequest.Companies)
		{
			foreach (Tuple<int, string> item in methodsToExecutePerCompany)
			{
				executionDetails.Add(new NavDataUpgradeMethodExecutionDetails(item.Item1, item.Item2, Guid.Empty, company));
			}
		}
		foreach (Tuple<int, string> item2 in methodsToExecutePerDatabase)
		{
			upgradePerDatabaseFunctions.Add(FormattableString.Invariant($"{item2.Item1}.{item2.Item2}"));
			executionDetails.Add(new NavDataUpgradeMethodExecutionDetails(item2.Item1, item2.Item2, Guid.Empty));
		}
		AddUpgradeEventExecutionDetails();
		if (!request.ExcludeExtensions && appUpgradeActivities != null)
		{
			AddAppUpgradeExecutionDetails(appUpgradeActivities);
		}
		if (!request.SkipCompanyInitialization)
		{
			foreach (string company2 in startRequest.Companies)
			{
				executionDetails.Add(new NavDataUpgradeMethodExecutionDetails(companyInitializationMethod.Item1, companyInitializationMethod.Item2, Guid.Empty, company2));
			}
		}
		ExecutionListener.AddListener(this);
		isMonitoring = true;
	}

	private void AddAppUpgradeExecutionDetails(IEnumerable<NavAppDataUpgradeActivities> appUpgradeActivities)
	{
		foreach (NavAppDataUpgradeActivities appUpgradeActivity in appUpgradeActivities)
		{
			NavAppRuntimeMetadata appBeingTargeted = appUpgradeActivity.Context.AppBeingTargeted;
			NavUpgradeActivityStep navUpgradeActivityStep = NavUpgradeActivityStep.HandleSpecialRecords((INavAppReference)(object)appBeingTargeted);
			executionDetails.Add(new NavDataUpgradeEventExecutionDetails(navUpgradeActivityStep.ModuleName, navUpgradeActivityStep.StepName, ((WrappedValue<Guid>)(object)appBeingTargeted.AppId).Value));
			AddUpgradeEventExecutionDetails((INavAppReference)(object)appBeingTargeted);
		}
	}

	private void AddUpgradeEventExecutionDetails(INavAppReference appModule = null)
	{
		NavUpgradeActivityStep[] array = new NavUpgradeActivityStep[4]
		{
			NavUpgradeActivityStep.CheckPreconditions(appModule),
			NavUpgradeActivityStep.OnUpgrade(appModule),
			NavUpgradeActivityStep.ValidateUpgrade(appModule),
			NavUpgradeActivityStep.AfterUpgradeCommit(appModule)
		};
		NavUpgradeActivityStep[] array2 = array;
		foreach (NavUpgradeActivityStep navUpgradeActivityStep in array2)
		{
			foreach (string item in startRequest.Companies.Concat(new string[1]))
			{
				executionDetails.Add(new NavDataUpgradeEventExecutionDetails(navUpgradeActivityStep.ModuleName, navUpgradeActivityStep.StepName, (appModule != null) ? ((WrappedValue<Guid>)(object)appModule.AppId).Value : Guid.Empty, item));
			}
		}
	}

	public void StopMonitoring()
	{
		if (!isMonitoring)
		{
			return;
		}
		ExecutionListener.RemoveListener(this);
		isMonitoring = false;
		foreach (NavDataUpgradeExecutionDetails item in ExecutionDetails.Where((NavDataUpgradeExecutionDetails next) => next.State == NavDataUpgradeMethodExecutionState.FailedPendingResume))
		{
			item.State = NavDataUpgradeMethodExecutionState.Failed;
		}
		foreach (NavDataUpgradeExecutionDetails item2 in ExecutionDetails.Where((NavDataUpgradeExecutionDetails next) => next.State == NavDataUpgradeMethodExecutionState.CanceledPendingResume))
		{
			item2.State = NavDataUpgradeMethodExecutionState.Canceled;
		}
		foreach (NavDataUpgradeExecutionDetails item3 in ExecutionDetails.Where((NavDataUpgradeExecutionDetails next) => next.State == NavDataUpgradeMethodExecutionState.Pending))
		{
			item3.State = NavDataUpgradeMethodExecutionState.NotExecuted;
		}
		upgradePerDatabaseFunctions.Clear();
	}

	public void ProcessUpgradeStepComplete(NavUpgradeActivityStep upgradeStep, string companyName)
	{
		NavDataUpgradeEventExecutionDetails entryToUpdate = FindMatchingAppEventAmongMonitoredEntries(upgradeStep, companyName);
		UpdateEntryCompleted(entryToUpdate);
	}

	public void ProcessExit(NavMethodScope methodScope)
	{
		NavDataUpgradeMethodExecutionDetails navDataUpgradeMethodExecutionDetails = FindMatchingCodeunitMethodAmongMonitoredEntries(methodScope);
		if (!methodScope.HasUnhandledException)
		{
			if (navDataUpgradeMethodExecutionDetails != null && navDataUpgradeMethodExecutionDetails.State == NavDataUpgradeMethodExecutionState.Pending)
			{
				navDataUpgradeMethodExecutionDetails.Start();
				navDataUpgradeMethodExecutionDetails.SessionId = methodScope.Session.Id;
			}
			UpdateEntryCompleted(navDataUpgradeMethodExecutionDetails);
		}
	}

	public void ProcessUpgradeStepStarting(NavUpgradeActivityStep upgradeStep, string companyName, int sessionId)
	{
		NavDataUpgradeEventExecutionDetails entryToUpdate = FindMatchingAppEventAmongMonitoredEntries(upgradeStep, companyName);
		UpdateEntryStarting(entryToUpdate, sessionId);
	}

	public void ProcessStatementHit(NavMethodScope methodScope)
	{
		NavDataUpgradeMethodExecutionDetails entryToUpdate = FindMatchingCodeunitMethodAmongMonitoredEntries(methodScope);
		UpdateEntryStarting(entryToUpdate, methodScope.Session.Id);
	}

	public void ProcessException(NavMethodScope methodScope, Exception exception)
	{
		NavDataUpgradeMethodExecutionDetails navDataUpgradeMethodExecutionDetails = FindMatchingCodeunitMethodAmongMonitoredEntries(methodScope);
		if (navDataUpgradeMethodExecutionDetails == null)
		{
			return;
		}
		if (exception is NavSessionTerminatedException && methodScope.Session.AccessLock.IsStoppingOrDisposed)
		{
			navDataUpgradeMethodExecutionDetails.State = NavDataUpgradeMethodExecutionState.CanceledPendingResume;
			return;
		}
		navDataUpgradeMethodExecutionDetails.Error = exception.Message;
		if (!startRequest.ParallelExecutionEnabled && startRequest.ContinueOnErrorEnabled)
		{
			navDataUpgradeMethodExecutionDetails.State = NavDataUpgradeMethodExecutionState.Failed;
		}
		else
		{
			navDataUpgradeMethodExecutionDetails.State = NavDataUpgradeMethodExecutionState.FailedPendingResume;
		}
	}

	public bool CanCoexistWithOtherListener(IExecutionListener otherListener)
	{
		return true;
	}

	private NavDataUpgradeMethodExecutionDetails FindMatchingCodeunitMethodAmongMonitoredEntries(NavMethodScope methodScope)
	{
		if (startRequest.TenantId == null || !startRequest.TenantId.Equals(methodScope.Session.Tenant.Id, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		if (!IsCurrentlyExecutingNavUpgradeCode(methodScope))
		{
			return null;
		}
		NavApplicationObjectBase applicationObject = methodScope.ApplicationObject;
		if (upgradePerDatabaseFunctions.Contains(FormattableString.Invariant($"{methodScope.ApplicationObject.ObjectId.ObjectNumber}.{methodScope.ScopeName}")))
		{
			return FindMatchingCodeunitMethodAmongMonitoredEntries(applicationObject.ObjectId.ObjectNumber, methodScope.ScopeName, null);
		}
		string companyNameFromToken = methodScope.Session.Database.GetCompanyNameFromToken(methodScope.Session.Company.CompanyNameToken);
		return FindMatchingCodeunitMethodAmongMonitoredEntries(applicationObject.ObjectId.ObjectNumber, methodScope.ScopeName, companyNameFromToken);
	}

	private NavDataUpgradeMethodExecutionDetails FindMatchingCodeunitMethodAmongMonitoredEntries(int codeunitId, string methodName, string companyName)
	{
		return executionDetails.OfType<NavDataUpgradeMethodExecutionDetails>().FirstOrDefault((NavDataUpgradeMethodExecutionDetails next) => next.CodeunitId == codeunitId && next.FunctionName.Equals(methodName, StringComparison.OrdinalIgnoreCase) && ((string.IsNullOrEmpty(companyName) && string.IsNullOrEmpty(next.CompanyName)) || string.Equals(next.CompanyName, companyName, StringComparison.OrdinalIgnoreCase)));
	}

	private NavDataUpgradeEventExecutionDetails FindMatchingAppEventAmongMonitoredEntries(NavUpgradeActivityStep step, string companyName)
	{
		return executionDetails.OfType<NavDataUpgradeEventExecutionDetails>().FirstOrDefault((NavDataUpgradeEventExecutionDetails next) => string.Equals(next.ModuleName, step.ModuleName, StringComparison.OrdinalIgnoreCase) && string.Equals(next.WorkflowStep, step.StepName, StringComparison.OrdinalIgnoreCase) && ((string.IsNullOrEmpty(companyName) && string.IsNullOrEmpty(next.CompanyName)) || string.Equals(next.CompanyName, companyName, StringComparison.OrdinalIgnoreCase)));
	}

	internal static bool IsCurrentlyExecutingNavUpgradeCode(NavMethodScope methodScope)
	{
		if (methodScope.ApplicationObject == null)
		{
			return false;
		}
		if (!(methodScope.ApplicationObject is NavUpgradeCodeunit))
		{
			if (methodScope.ApplicationObject.ObjectId.ObjectType == ObjectType.CodeUnit)
			{
				return methodScope.ApplicationObject.ObjectId.ObjectNumber == 2;
			}
			return false;
		}
		return true;
	}

	private static void UpdateEntryStarting(NavDataUpgradeExecutionDetails entryToUpdate, int sessionId)
	{
		if (entryToUpdate != null && entryToUpdate.State != NavDataUpgradeMethodExecutionState.InProgress)
		{
			entryToUpdate.Error = null;
			entryToUpdate.SessionId = sessionId;
			entryToUpdate.State = NavDataUpgradeMethodExecutionState.InProgress;
			entryToUpdate.Start();
		}
	}

	private static void UpdateEntryError(NavDataUpgradeExecutionDetails entryToUpdate, Exception exception, bool shouldFail, bool postUpgrade)
	{
		if (entryToUpdate != null)
		{
			entryToUpdate.Error = exception?.Message;
			entryToUpdate.PostUpgrade = postUpgrade;
			if (shouldFail)
			{
				entryToUpdate.State = NavDataUpgradeMethodExecutionState.Failed;
			}
			else
			{
				entryToUpdate.State = NavDataUpgradeMethodExecutionState.FailedPendingResume;
			}
		}
	}

	private static void UpdateEntryCompleted(NavDataUpgradeExecutionDetails entryToUpdate)
	{
		if (entryToUpdate != null)
		{
			entryToUpdate.State = NavDataUpgradeMethodExecutionState.Completed;
			entryToUpdate.Stop();
			entryToUpdate.Error = null;
		}
	}
}
