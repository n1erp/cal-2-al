using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal abstract class NavAppDataUpgradeActivityFactory
{
	public const string SpecialRecordsTag = "SPECIALRECORDS";

	public const string CheckPreconditionsTag = "CHECKPRECONDITIONS";

	public const string OnUpgradeTag = "ONUPGRADE";

	public const string ValidateUpgradeTag = "VALIDATEUPGRADE";

	public const string ClearGlobalCacheTag = "CLEARGLOBALCACHE";

	public const string SetTransactionalPhaseCompleteTag = "SETTRANSACTIONALPHASECOMPLETE";

	public const string AfterUpgradeTag = "AFTERUPGRADE";

	public const string UpdateInstallRecords = "UPDATEINSTALLRECORDS";

	protected NavAppJointUpgradeContext Context { get; }

	protected IEnumerable<string> Companies { get; }

	protected NavAppDataUpgradeActivityFactory(NavAppJointUpgradeContext context, IEnumerable<string> companies)
	{
		Context = context;
		Companies = companies;
	}

	public static NavAppDataUpgradeActivityFactory CreateForInstall(NavAppJointUpgradeContext context, IEnumerable<string> companies)
	{
		return new NavAppDataInstallationContextActivityFactory(context, companies);
	}

	public static NavAppDataUpgradeActivityFactory CreateForUpgrade(NavAppJointUpgradeContext context, IEnumerable<string> companies)
	{
		return new NavAppDataUpgradeContextActivityFactory(context, companies);
	}

	public static IEnumerable<NavAppDataUpgradeActivities> CreateActivitiesForNavAppUpgrade(IEnumerable<NavAppJointUpgradeContext> contexts, IEnumerable<string> companies)
	{
		List<NavAppDataUpgradeActivities> list = new List<NavAppDataUpgradeActivities>();
		companies = companies?.ToArray();
		foreach (NavAppJointUpgradeContext context in contexts)
		{
			NavAppDataUpgradeActivityFactory navAppDataUpgradeActivityFactory = ((context.ContextType == JointUpgradeContextType.Upgrade) ? CreateForUpgrade(context, companies) : CreateForInstall(context, companies));
			list.Add(new NavAppDataUpgradeActivities(context)
			{
				HandleSpecialRecords = navAppDataUpgradeActivityFactory.CreateWorkflowActivityForHandlingSpecialRecords(),
				CheckPreconditions = navAppDataUpgradeActivityFactory.CreateWorkflowActivityForRunningCheckPreconditionEvents(),
				OnUpgrade = navAppDataUpgradeActivityFactory.CreateWorkflowActivityForRunningOnUpgradeEvents(),
				ValidateUpgrade = navAppDataUpgradeActivityFactory.CreateWorkflowActivityForRunningValidateUpgradeEvents(),
				ClearGlobalCache = navAppDataUpgradeActivityFactory.CreateWorkflowActivityForClearingGlobalCache(),
				SetTransactionalPhaseComplete = navAppDataUpgradeActivityFactory.CreateWorkflowActivityForSettingTransactionPhaseComplete(),
				AfterUpgradeCommit = navAppDataUpgradeActivityFactory.CreateWorkflowActivityForRunningAfterUpgradeEvents(),
				UpdateAppDatabase = navAppDataUpgradeActivityFactory.CreateWorkflowActivityForUpdatingAppDatabaseRecords()
			});
		}
		return list;
	}

	public virtual Activity CreateWorkflowActivityForHandlingSpecialRecords()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)Context.AppBeingTargeted, "SPECIALRECORDS");
		return new NullExecuteNavUpgradeActivity(bookmarkName, NavUpgradeActivityStep.HandleSpecialRecords((INavAppReference)(object)Context.AppBeingTargeted), transactionalSession: true, postUpgradeActivity: false);
	}

	public virtual Activity CreateWorkflowActivityForRunningCheckPreconditionEvents()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)Context.AppBeingTargeted, "CHECKPRECONDITIONS");
		return new NullExecuteNavUpgradeActivity(bookmarkName, NavUpgradeActivityStep.CheckPreconditions((INavAppReference)(object)Context.AppBeingTargeted), transactionalSession: true, postUpgradeActivity: false);
	}

	public abstract Activity CreateWorkflowActivityForRunningOnUpgradeEvents();

	public virtual Activity CreateWorkflowActivityForRunningValidateUpgradeEvents()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)Context.AppBeingTargeted, "VALIDATEUPGRADE");
		return new NullExecuteNavUpgradeActivity(bookmarkName, NavUpgradeActivityStep.ValidateUpgrade((INavAppReference)(object)Context.AppBeingTargeted), transactionalSession: true, postUpgradeActivity: false);
	}

	public virtual Activity CreateWorkflowActivityForClearingGlobalCache()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)Context.AppBeingTargeted, "CLEARGLOBALCACHE");
		return new NullExecuteNavUpgradeActivity(bookmarkName, NavUpgradeActivityStep.ClearGlobalCache((INavAppReference)(object)Context.AppBeingTargeted), transactionalSession: true, postUpgradeActivity: false);
	}

	public virtual Activity CreateWorkflowActivityForSettingTransactionPhaseComplete()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)Context.AppBeingTargeted, "SETTRANSACTIONALPHASECOMPLETE");
		return new NullExecuteNavUpgradeActivity(bookmarkName, NavUpgradeActivityStep.SetTransactionalPhaseComplete((INavAppReference)(object)Context.AppBeingTargeted), transactionalSession: true, postUpgradeActivity: false);
	}

	public abstract Activity CreateWorkflowActivityForUpdatingAppDatabaseRecords();

	public virtual Activity CreateWorkflowActivityForRunningAfterUpgradeEvents()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)Context.AppBeingTargeted, "AFTERUPGRADE");
		return new NullExecuteNavUpgradeActivity(bookmarkName, NavUpgradeActivityStep.AfterUpgradeCommit((INavAppReference)(object)Context.AppBeingTargeted), transactionalSession: false, postUpgradeActivity: true);
	}
}
