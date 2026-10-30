using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface INavDataUpgradeProgressMonitor : IExecutionListener
{
	IReadOnlyCollection<NavDataUpgradeExecutionDetails> ExecutionDetails { get; }

	void ProcessUpgradeStepStarting(NavUpgradeActivityStep upgradeStep, string companyName, int sessionId);

	void ProcessUpgradeStepComplete(NavUpgradeActivityStep upgradeStep, string companyName);

	void ReportError(int codeunitId, string methodName, string companyName, Exception exception);

	void ReportEventError(NavUpgradeActivityStep upgradeStep, string companyName, Exception exception, bool postUpgrade);

	void StartMonitoring(IEnumerable<Tuple<int, string>> methodsToExecutePerCompany, IEnumerable<Tuple<int, string>> methodsToExecutePerDatabase, IEnumerable<NavAppDataUpgradeActivities> appUpgradeActivities, Tuple<int, string> companyInitializationMethod, NavDataUpgradeWorkflowStartRequest request);

	void StopMonitoring();
}
