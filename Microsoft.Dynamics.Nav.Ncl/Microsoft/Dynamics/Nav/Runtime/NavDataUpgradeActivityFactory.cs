using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Runtime.Workflow;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDataUpgradeActivityFactory : INavDataUpgradeActivityFactory
{
	private readonly INavDataUpgradeMetadata upgradeMetadata;

	public NavDataUpgradeActivityFactory(INavDataUpgradeMetadata upgradeMetadata)
	{
		this.upgradeMetadata = upgradeMetadata ?? throw new ArgumentNullException("upgradeMetadata");
	}

	public Activity CreateWorkflowForSerialExecutionOfNavUpgrade(IEnumerable<string> companies, bool continueOnError, bool skipCompanyInitialization, bool singleTransactionMode, IEnumerable<NavAppDataUpgradeActivities> navAppUpgradeActivities)
	{
		Sequence sequence = new Sequence();
		List<string> companies2 = new List<string>(companies);
		navAppUpgradeActivities = navAppUpgradeActivities?.ToList() ?? new List<NavAppDataUpgradeActivities>();
		IList<Activity> activitiesToExecute = CreateActivitiesForRunningCheckPreconditionMethods(companies2);
		NavExecuteUpgradeCodeunitMethodOnErrorAction onErrorAction = ((!continueOnError) ? NavExecuteUpgradeCodeunitMethodOnErrorAction.SuspendItself : NavExecuteUpgradeCodeunitMethodOnErrorAction.IgnoreErrorAndFinish);
		if (singleTransactionMode)
		{
			onErrorAction = NavExecuteUpgradeCodeunitMethodOnErrorAction.TerminateProcess;
		}
		IList<Activity> first = CreateActivitiesForRunningUpgradePerCompany(companies2, onErrorAction);
		IList<Activity> second = CreateActivitiesForRunningUpgradePerDatabase(onErrorAction);
		IList<Activity> first2 = CreateActivitiesForRunningValidateUpgradePerCompanyEvents(companies2);
		IList<Activity> second2 = CreateActivitiesForRunningValidateUpgradePerDatabaseEvents();
		IList<Activity> first3 = CreateActivitiesForRunningAfterCommitPerCompanyEvents(companies2);
		IList<Activity> second3 = CreateActivitiesForRunningAfterCommitPerDatabaseEvents();
		NavSerialExecutionActivity item = new NavSerialExecutionActivity(activitiesToExecute, "BASE - PRECONDITIONS");
		NavSerialExecutionActivity item2 = new NavSerialExecutionActivity(first.Union(second), "BASE - ONUPGRADE");
		NavSerialExecutionActivity item3 = new NavSerialExecutionActivity(first2.Union(second2), "BASE - VALIDATEUPGRADE");
		NavSerialExecutionActivity item4 = new NavSerialExecutionActivity(first3.Union(second3), "BASE - AFTERCOMMIT");
		NavSerialExecutionActivity navSerialExecutionActivity = null;
		if (!skipCompanyInitialization)
		{
			IList<Activity> activitiesToExecute2 = CreateActivityForRunningCompanyInitialization(companies2, onErrorAction);
			navSerialExecutionActivity = new NavSerialExecutionActivity(activitiesToExecute2, "COMPANYINITIALIZE");
		}
		if (singleTransactionMode)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000UR", Category.DataUpgrade, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Modify upgrade Sequence to use transaction.");
			NavSerialExecutionActivity item5 = new NavSerialExecutionActivity(navAppUpgradeActivities.Select((NavAppDataUpgradeActivities a) => a.HandleSpecialRecords), "SPECIALRECORDS");
			NavSerialExecutionActivity item6 = new NavSerialExecutionActivity(navAppUpgradeActivities.Select((NavAppDataUpgradeActivities a) => a.CheckPreconditions), "CHECKPRECONDITIONS");
			NavSerialExecutionActivity item7 = new NavSerialExecutionActivity(navAppUpgradeActivities.Select((NavAppDataUpgradeActivities a) => a.OnUpgrade), "ONUPGRADE");
			NavSerialExecutionActivity item8 = new NavSerialExecutionActivity(navAppUpgradeActivities.Select((NavAppDataUpgradeActivities a) => a.ValidateUpgrade), "VALIDATEUPGRADE");
			NavSerialExecutionActivity item9 = new NavSerialExecutionActivity(navAppUpgradeActivities.Select((NavAppDataUpgradeActivities a) => a.SetTransactionalPhaseComplete), "SETTRANSACTIONALPHASECOMPLETE");
			NavSerialExecutionActivity item10 = new NavSerialExecutionActivity(new Activity[1] { navAppUpgradeActivities.FirstOrDefault()?.ClearGlobalCache }.Where((Activity a) => a != null), "CLEARGLOBALCACHE");
			sequence.Activities.Add(CreateDataUpgradeTransactionActivity("SingleTransaction - Start transaction"));
			sequence.Activities.Add(item5);
			sequence.Activities.Add(item);
			sequence.Activities.Add(item6);
			sequence.Activities.Add(item2);
			sequence.Activities.Add(item7);
			sequence.Activities.Add(item3);
			sequence.Activities.Add(item8);
			sequence.Activities.Add(item10);
			if (navSerialExecutionActivity != null)
			{
				sequence.Activities.Add(navSerialExecutionActivity);
			}
			sequence.Activities.Add(CreateDataUpgradeSetVersionNumberActivity());
			sequence.Activities.Add(CreateDataUpgradeTransactionActivity("SingleTransaction - Commit transaction"));
			sequence.Activities.Add(item9);
		}
		else
		{
			sequence.Activities.Add(item);
			sequence.Activities.Add(item2);
			sequence.Activities.Add(item3);
			foreach (NavAppDataUpgradeActivities navAppUpgradeActivity in navAppUpgradeActivities)
			{
				string displayName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)navAppUpgradeActivity.Context.AppBeingTargeted, "Serial - Start single session");
				string displayName2 = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)navAppUpgradeActivity.Context.AppBeingTargeted, "Serial - Start transaction");
				string displayName3 = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)navAppUpgradeActivity.Context.AppBeingTargeted, "Serial - Commit transaction");
				string displayName4 = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)navAppUpgradeActivity.Context.AppBeingTargeted, "Serial - Stop single session");
				sequence.Activities.Add(NavDataUpgradeSessionManagementConfigurationActivity.StartSingleSessionMode(displayName));
				sequence.Activities.Add(CreateDataUpgradeTransactionActivity(displayName2, forceCreateNew: true));
				sequence.Activities.Add(navAppUpgradeActivity.HandleSpecialRecords);
				sequence.Activities.Add(navAppUpgradeActivity.CheckPreconditions);
				sequence.Activities.Add(navAppUpgradeActivity.OnUpgrade);
				sequence.Activities.Add(navAppUpgradeActivity.ValidateUpgrade);
				sequence.Activities.Add(navAppUpgradeActivity.ClearGlobalCache);
				sequence.Activities.Add(CreateDataUpgradeTransactionActivity(displayName3));
				sequence.Activities.Add(navAppUpgradeActivity.SetTransactionalPhaseComplete);
				sequence.Activities.Add(NavDataUpgradeSessionManagementConfigurationActivity.StopSingleSessionMode(displayName4));
			}
			if (navSerialExecutionActivity != null)
			{
				sequence.Activities.Add(navSerialExecutionActivity);
			}
			sequence.Activities.Add(CreateDataUpgradeSetVersionNumberActivity());
		}
		sequence.Activities.Add(item4);
		sequence.Activities.Add(new NavSerialExecutionActivity(navAppUpgradeActivities.Select((NavAppDataUpgradeActivities a) => a.AfterUpgradeCommit), "AFTERUPGRADE"));
		sequence.Activities.Add(new NavSerialExecutionActivity(navAppUpgradeActivities.Select((NavAppDataUpgradeActivities a) => a.UpdateAppDatabase), "UPDATEINSTALLRECORDS"));
		return sequence;
	}

	private static Activity CreateDataUpgradeSetVersionNumberActivity()
	{
		return new NavDataUpgradeSetVersionNumberActivity(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, "Set version number");
	}

	public Activity CreateWorkflowForParallelExecutionOfNavUpgrade(IEnumerable<string> companies, bool continueOnError, bool skipCompanyInitialization, IEnumerable<NavAppDataUpgradeActivities> navAppUpgradeMetadata)
	{
		navAppUpgradeMetadata = navAppUpgradeMetadata?.ToList() ?? new List<NavAppDataUpgradeActivities>();
		if (navAppUpgradeMetadata.Any())
		{
			string text = string.Join("; ", navAppUpgradeMetadata.Select((NavAppDataUpgradeActivities m) =>
			{
				NavAppRuntimeMetadata appBeingTargeted = m.Context.AppBeingTargeted;
				return (appBeingTargeted == null) ? null : appBeingTargeted.FriendlyDisplayName;
			}));
			throw new NavUpgradeProcessExecutionException(Lang.NavAppParallelUpgradeNotSupported.FormatWithResolvedCulture(text));
		}
		Sequence sequence = new Sequence();
		IList<Activity> activitiesToExecute = CreateActivitiesForRunningCheckPreconditionMethods(companies);
		IList<Activity> first = CreateActivitiesForRunningUpgradePerCompany(companies, NavExecuteUpgradeCodeunitMethodOnErrorAction.SuspendItself);
		IList<Activity> second = CreateActivitiesForRunningUpgradePerDatabase(NavExecuteUpgradeCodeunitMethodOnErrorAction.SuspendItself);
		NavParallelExecutionActivity item = new NavParallelExecutionActivity(activitiesToExecute);
		NavParallelExecutionActivity item2 = new NavParallelExecutionActivity(first.Union(second));
		sequence.Activities.Add(item);
		sequence.Activities.Add(item2);
		if (!skipCompanyInitialization)
		{
			IList<Activity> activitiesToExecute2 = CreateActivityForRunningCompanyInitialization(companies, NavExecuteUpgradeCodeunitMethodOnErrorAction.SuspendItself);
			NavParallelExecutionActivity item3 = new NavParallelExecutionActivity(activitiesToExecute2);
			sequence.Activities.Add(item3);
		}
		sequence.Activities.Add(CreateDataUpgradeSetVersionNumberActivity());
		return sequence;
	}

	public Activity CreateDataUpgradeTransactionActivity(string displayName = null, bool forceCreateNew = false)
	{
		return new NavDataUpgradeTransactionActivity(displayName, forceCreateNew);
	}

	public IList<Activity> CreateActivitiesForRunningUpgradePerCompany(IEnumerable<string> companies, NavExecuteUpgradeCodeunitMethodOnErrorAction onErrorAction)
	{
		return CreateActivitiesForRunningProvidedCodeunitMethods(companies, upgradeMetadata.UpgradePerCompanyMethods, onErrorAction).Concat(CreateActivitiesForRunningOnUpgradePerCompanyEvents(companies)).ToList();
	}

	public IList<Activity> CreateActivitiesForRunningUpgradePerDatabase(NavExecuteUpgradeCodeunitMethodOnErrorAction onErrorAction)
	{
		return CreateActivitiesForRunningProvidedCodeunitMethods(new string[1] { string.Empty }, upgradeMetadata.UpgradePerDatabaseMethods, onErrorAction).Concat(CreateActivitiesForRunningOnUpgradePerDatabaseEvents()).ToList();
	}

	public IList<Activity> CreateActivityForRunningCompanyInitialization(IEnumerable<string> companies, NavExecuteUpgradeCodeunitMethodOnErrorAction onErrorAction)
	{
		return CreateActivitiesForRunningProvidedCodeunitMethods(companies, new Tuple<int, string>[1] { upgradeMetadata.CompanyInitializationMethod }, onErrorAction);
	}

	public IList<Activity> CreateActivitiesForRunningCheckPreconditionMethods(IEnumerable<string> companies)
	{
		return CreateActivitiesForRunningProvidedCodeunitMethods(companies, upgradeMetadata.CheckPreconditionMethods, NavExecuteUpgradeCodeunitMethodOnErrorAction.TerminateProcess).Concat(CreateActivitiesForRunningCheckPreconditionPerCompanyEvents(companies)).Concat(CreateActivitiesForRunningCheckPerconditionPerDatabaseeEvents()).ToList();
	}

	public static IList<Activity> CreateActivitiesForRunningCheckPreconditionPerCompanyEvents(IEnumerable<string> companies)
	{
		return CreateActivitiesForRunningUpgradeEvents(companies, NavUpgradeEvent.OnCheckPreconditionsPerCompany, NavUpgradeActivityStep.CheckPreconditions(), transactionalSession: true, postUpgrade: false);
	}

	public static IList<Activity> CreateActivitiesForRunningCheckPerconditionPerDatabaseeEvents()
	{
		return CreateActivitiesForRunningUpgradeEvents(null, NavUpgradeEvent.OnCheckPreconditionsPerDatabase, NavUpgradeActivityStep.CheckPreconditions(), transactionalSession: true, postUpgrade: false);
	}

	public static IList<Activity> CreateActivitiesForRunningOnUpgradePerCompanyEvents(IEnumerable<string> companies)
	{
		return CreateActivitiesForRunningUpgradeEvents(companies, NavUpgradeEvent.OnUpgradePerCompany, NavUpgradeActivityStep.OnUpgrade(), transactionalSession: true, postUpgrade: false);
	}

	public static IList<Activity> CreateActivitiesForRunningOnUpgradePerDatabaseEvents()
	{
		return CreateActivitiesForRunningUpgradeEvents(null, NavUpgradeEvent.OnUpgradePerDatabase, NavUpgradeActivityStep.OnUpgrade(), transactionalSession: true, postUpgrade: false);
	}

	public static IList<Activity> CreateActivitiesForRunningValidateUpgradePerCompanyEvents(IEnumerable<string> companies)
	{
		return CreateActivitiesForRunningUpgradeEvents(companies, NavUpgradeEvent.OnValidateUpgradePerCompany, NavUpgradeActivityStep.ValidateUpgrade(), transactionalSession: true, postUpgrade: false);
	}

	public static IList<Activity> CreateActivitiesForRunningValidateUpgradePerDatabaseEvents()
	{
		return CreateActivitiesForRunningUpgradeEvents(null, NavUpgradeEvent.OnValidateUpgradePerDatabase, NavUpgradeActivityStep.ValidateUpgrade(), transactionalSession: true, postUpgrade: false);
	}

	public static IList<Activity> CreateActivitiesForRunningAfterCommitPerCompanyEvents(IEnumerable<string> companies)
	{
		return CreateActivitiesForRunningUpgradeEvents(companies, NavUpgradeEvent.OnAfterUpgradeCommitPerCompany, NavUpgradeActivityStep.AfterUpgradeCommit(), transactionalSession: false, postUpgrade: true);
	}

	public static IList<Activity> CreateActivitiesForRunningAfterCommitPerDatabaseEvents()
	{
		return CreateActivitiesForRunningUpgradeEvents(null, NavUpgradeEvent.OnAfterUpgradeCommitPerDatabase, NavUpgradeActivityStep.AfterUpgradeCommit(), transactionalSession: false, postUpgrade: true);
	}

	private static IList<Activity> CreateActivitiesForRunningUpgradeEvents(IEnumerable<string> companies, NavUpgradeEvent upgradeEvent, NavUpgradeActivityStep step, bool transactionalSession, bool postUpgrade)
	{
		List<Activity> list = new List<Activity>();
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForBaseUpgradeActivity(upgradeEvent.ToString());
		list.Add(new ExecuteNavUpgradeEventActivity(companies, upgradeEvent, bookmarkName, step, transactionalSession, postUpgrade));
		return list;
	}

	public IList<Activity> CreateActivitiesForRunningProvidedCodeunitMethods(IEnumerable<string> companies, IList<Tuple<int, string>> codeunitsMethods, NavExecuteUpgradeCodeunitMethodOnErrorAction onErrorAction)
	{
		IList<Activity> list = new List<Activity>();
		foreach (string company in companies)
		{
			foreach (Tuple<int, string> codeunitsMethod in codeunitsMethods)
			{
				Activity item = CreateSingleCodeunitMethodExecutionActivity(codeunitsMethod.Item1, codeunitsMethod.Item2, company, onErrorAction);
				list.Add(item);
			}
		}
		return list;
	}

	public Activity CreateSingleCodeunitMethodExecutionActivity(int codeunitId, string methodName, string companyName, NavExecuteUpgradeCodeunitMethodOnErrorAction onErrorAction)
	{
		string text = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForCodeunitExecutionActivity(codeunitId, methodName, companyName);
		return new NavExecuteUpgradeCodeunitMethodWithRetryActivity
		{
			CompanyName = companyName,
			CodeunitId = codeunitId,
			MethodName = methodName,
			BookmarkName = text,
			DisplayName = text,
			OnErrorAction = onErrorAction
		};
	}
}
