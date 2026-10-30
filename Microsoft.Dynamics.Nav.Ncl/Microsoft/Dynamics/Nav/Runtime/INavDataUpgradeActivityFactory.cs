using System;
using System.Activities;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Workflow;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface INavDataUpgradeActivityFactory
{
	Activity CreateWorkflowForSerialExecutionOfNavUpgrade(IEnumerable<string> companies, bool continueOnError, bool skipCompanyInitialization, bool singleTransactionMode, IEnumerable<NavAppDataUpgradeActivities> extensionUpgrades);

	Activity CreateWorkflowForParallelExecutionOfNavUpgrade(IEnumerable<string> companies, bool continueOnError, bool skipCompanyInitialization, IEnumerable<NavAppDataUpgradeActivities> extensionUpgrades);

	Activity CreateDataUpgradeTransactionActivity(string displayName = null, bool forceCreateNew = false);

	IList<Activity> CreateActivitiesForRunningUpgradePerCompany(IEnumerable<string> companies, NavExecuteUpgradeCodeunitMethodOnErrorAction onErrorAction);

	IList<Activity> CreateActivitiesForRunningUpgradePerDatabase(NavExecuteUpgradeCodeunitMethodOnErrorAction onErrorAction);

	IList<Activity> CreateActivityForRunningCompanyInitialization(IEnumerable<string> companies, NavExecuteUpgradeCodeunitMethodOnErrorAction onErrorAction);

	IList<Activity> CreateActivitiesForRunningCheckPreconditionMethods(IEnumerable<string> companies);

	IList<Activity> CreateActivitiesForRunningProvidedCodeunitMethods(IEnumerable<string> companies, IList<Tuple<int, string>> codeunitsMethods, NavExecuteUpgradeCodeunitMethodOnErrorAction onErrorAction);

	Activity CreateSingleCodeunitMethodExecutionActivity(int codeunitId, string methodName, string companyName, NavExecuteUpgradeCodeunitMethodOnErrorAction onErrorAction);
}
