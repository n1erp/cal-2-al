using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal sealed class NavAppDataUpgradeContextActivityFactory : NavAppDataUpgradeActivityFactory
{
	private readonly NavAppUpgradeContext upgradeContext;

	public NavAppDataUpgradeContextActivityFactory(NavAppJointUpgradeContext context, IEnumerable<string> companies)
		: base(context, companies)
	{
		if (context.ContextType != JointUpgradeContextType.Upgrade)
		{
			throw new ArgumentException("Factory only supports contexts of type 'Upgrade'");
		}
		upgradeContext = context.Context as NavAppUpgradeContext;
	}

	public override Activity CreateWorkflowActivityForHandlingSpecialRecords()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)upgradeContext.AppBeingTargeted, "SPECIALRECORDS");
		return new ExecuteNavAppUpgradeSessionActionActivity<NavAppUpgradeContext>(upgradeContext, bookmarkName, NavUpgradeActivityStep.HandleSpecialRecords((INavAppReference)(object)upgradeContext.AppBeingTargeted), transactionalSession: true, (NavAppUpgradeContext upgContext, NavSession session) =>
		{
			NavAppUpgradeManager navAppUpgradeManager = new NavAppUpgradeManager(upgContext);
			using NavSqlConnectionScope tenantDatabaseConnectionScope = NavSqlConnectionScope.Create(session.Database, TransactionType.Update);
			navAppUpgradeManager.RunNavAppUpgradeSpecialActions(runInSystemSession: false, tenantDatabaseConnectionScope);
		}, postUpgradeActivity: false);
	}

	public override Activity CreateWorkflowActivityForRunningCheckPreconditionEvents()
	{
		return CreateSequenceForCompanyAndDatabaseEvents(NavUpgradeEvent.OnCheckPreconditionsPerCompany, NavUpgradeEvent.OnCheckPreconditionsPerDatabase, NavUpgradeActivityStep.CheckPreconditions, "CHECKPRECONDITIONS");
	}

	public override Activity CreateWorkflowActivityForRunningOnUpgradeEvents()
	{
		return CreateSequenceForCompanyAndDatabaseEvents(NavUpgradeEvent.OnUpgradePerCompany, NavUpgradeEvent.OnUpgradePerDatabase, NavUpgradeActivityStep.OnUpgrade, "ONUPGRADE");
	}

	public override Activity CreateWorkflowActivityForRunningValidateUpgradeEvents()
	{
		return CreateSequenceForCompanyAndDatabaseEvents(NavUpgradeEvent.OnValidateUpgradePerCompany, NavUpgradeEvent.OnValidateUpgradePerDatabase, NavUpgradeActivityStep.ValidateUpgrade, "VALIDATEUPGRADE");
	}

	public override Activity CreateWorkflowActivityForClearingGlobalCache()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)upgradeContext.AppBeingTargeted, "CLEARGLOBALCACHE");
		return new ExecuteNavAppUpgradeSessionActionActivity<NavAppUpgradeContext>(upgradeContext, bookmarkName, NavUpgradeActivityStep.ClearGlobalCache((INavAppReference)(object)upgradeContext.AppBeingTargeted), transactionalSession: true, (NavAppUpgradeContext upgContext, NavSession session) =>
		{
			NavAppUpgradeManager navAppUpgradeManager = new NavAppUpgradeManager(upgContext);
			navAppUpgradeManager.ClearGlobalCache(runInSystemSession: false);
		}, postUpgradeActivity: false);
	}

	public override Activity CreateWorkflowActivityForSettingTransactionPhaseComplete()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)upgradeContext.AppBeingTargeted, "SETTRANSACTIONALPHASECOMPLETE");
		return new ExecuteNavAppUpgradeSessionActionActivity<NavAppUpgradeContext>(upgradeContext, bookmarkName, NavUpgradeActivityStep.SetTransactionalPhaseComplete((INavAppReference)(object)upgradeContext.AppBeingTargeted), transactionalSession: true, (NavAppUpgradeContext upgContext, NavSession session) =>
		{
			upgContext.SetTransactionalPhaseComplete();
		}, postUpgradeActivity: false);
	}

	public override Activity CreateWorkflowActivityForUpdatingAppDatabaseRecords()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)upgradeContext.AppBeingTargeted, "UPDATEINSTALLRECORDS");
		return new ExecuteNavAppUpgradeSessionActionActivity<NavAppUpgradeContext>(upgradeContext, bookmarkName, NavUpgradeActivityStep.UpdateAppDatabase((INavAppReference)(object)upgradeContext.AppBeingTargeted), transactionalSession: true, (NavAppUpgradeContext upgContext, NavSession session) =>
		{
			NavAppUpgradeManager navAppUpgradeManager = new NavAppUpgradeManager(upgContext);
			navAppUpgradeManager.RunAppDatabaseInstallRecordsUpdate(runInSystemSession: false);
		}, postUpgradeActivity: false);
	}

	public override Activity CreateWorkflowActivityForRunningAfterUpgradeEvents()
	{
		return CreateSequenceForCompanyAndDatabaseEvents(NavUpgradeEvent.OnAfterUpgradeCommitPerCompany, NavUpgradeEvent.OnAfterUpgradeCommitPerDatabase, NavUpgradeActivityStep.AfterUpgradeCommit, "AFTERUPGRADE", transactionalSession: false, postUpgrade: true);
	}

	private Activity CreateSequenceForCompanyAndDatabaseEvents(NavUpgradeEvent perCompanyEvent, NavUpgradeEvent perDatabaseEvent, Func<INavAppReference, NavUpgradeActivityStep> stepCreator, string sequenceName, bool transactionalSession = true, bool postUpgrade = false)
	{
		NavUpgradeActivityStep step = stepCreator((INavAppReference)(object)upgradeContext.AppBeingTargeted);
		Sequence sequence = new Sequence();
		if (sequenceName != null)
		{
			sequence.DisplayName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)upgradeContext.AppBeingTargeted, sequenceName);
		}
		sequence.Activities.Add(CreateActivityForRunningUpgradeEvents(upgradeContext, Companies, perCompanyEvent, step, transactionalSession, postUpgrade));
		sequence.Activities.Add(CreateActivityForRunningUpgradeEvents(upgradeContext, null, perDatabaseEvent, step, transactionalSession, postUpgrade));
		return sequence;
	}

	private static Activity CreateActivityForRunningUpgradeEvents(NavAppUpgradeContext context, IEnumerable<string> companies, NavUpgradeEvent upgradeEvent, NavUpgradeActivityStep step, bool transactionalSession, bool postUpgrade)
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)context.AppBeingTargeted, upgradeEvent.ToString());
		return new ExecuteNavAppUpgradeEventActivity(context, companies, upgradeEvent, bookmarkName, step, transactionalSession, postUpgrade);
	}
}
