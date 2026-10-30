using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal sealed class NavAppDataInstallationContextActivityFactory : NavAppDataUpgradeActivityFactory
{
	private readonly NavAppInstallationContext installContext;

	public NavAppDataInstallationContextActivityFactory(NavAppJointUpgradeContext context, IEnumerable<string> companies)
		: base(context, companies)
	{
		if (context.ContextType != JointUpgradeContextType.Install)
		{
			throw new ArgumentException("Factory only supports contexts of type 'Install'");
		}
		installContext = context.Context as NavAppInstallationContext;
	}

	public override Activity CreateWorkflowActivityForHandlingSpecialRecords()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)installContext.AppBeingTargeted, "SPECIALRECORDS");
		return new ExecuteNavAppUpgradeSessionActionActivity<NavAppInstallationContext>(installContext, bookmarkName, NavUpgradeActivityStep.HandleSpecialRecords((INavAppReference)(object)installContext.AppBeingTargeted), transactionalSession: true, (NavAppInstallationContext instContext, NavSession session) =>
		{
			NavAppInstallationProcessor navAppInstallationProcessor = new NavAppInstallationProcessor(instContext);
			using NavSqlConnectionScope tenantDatabaseConnectionScope = NavSqlConnectionScope.Create(session.Database, TransactionType.Update);
			navAppInstallationProcessor.RunNavAppInstallSpecialRecordActions(runInSystemSession: false, doNotLoadData: false, tenantDatabaseConnectionScope);
		}, postUpgradeActivity: false);
	}

	public override Activity CreateWorkflowActivityForRunningOnUpgradeEvents()
	{
		Sequence sequence = new Sequence
		{
			DisplayName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)installContext.AppBeingTargeted, "ONUPGRADE")
		};
		NavUpgradeActivityStep upgradeStep = NavUpgradeActivityStep.OnUpgrade((INavAppReference)(object)installContext.AppBeingTargeted);
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)installContext.AppBeingTargeted, NavSystemCodeunitExtensionTriggers.CodeunitMethod.OnInstallAppPerCompany.ToString());
		sequence.Activities.Add(new ExecuteNavAppInstallEventActivity(installContext, Companies, bookmarkName, upgradeStep, transactionalSession: true, postUpgradeActivity: false));
		string bookmarkName2 = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)installContext.AppBeingTargeted, NavSystemCodeunitExtensionTriggers.CodeunitMethod.OnInstallAppPerDatabase.ToString());
		sequence.Activities.Add(new ExecuteNavAppInstallEventActivity(installContext, null, bookmarkName2, upgradeStep, transactionalSession: true, postUpgradeActivity: false));
		return sequence;
	}

	public override Activity CreateWorkflowActivityForClearingGlobalCache()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)installContext.AppBeingTargeted, "CLEARGLOBALCACHE");
		return new ExecuteNavAppUpgradeSessionActionActivity<NavAppInstallationContext>(installContext, bookmarkName, NavUpgradeActivityStep.ClearGlobalCache((INavAppReference)(object)installContext.AppBeingTargeted), transactionalSession: true, (NavAppInstallationContext instContext, NavSession session) =>
		{
			NavAppInstallationProcessor navAppInstallationProcessor = new NavAppInstallationProcessor(instContext);
			navAppInstallationProcessor.ClearGlobalCache(runInSystemSession: false);
		}, postUpgradeActivity: false);
	}

	public override Activity CreateWorkflowActivityForUpdatingAppDatabaseRecords()
	{
		string bookmarkName = NavDataUpgradeWorkflowBookmarkNameGenerator.CreateForNavAppUpgradeActivity((INavAppReference)(object)installContext.AppBeingTargeted, "UPDATEINSTALLRECORDS");
		return new ExecuteNavAppUpgradeSessionActionActivity<NavAppInstallationContext>(installContext, bookmarkName, NavUpgradeActivityStep.UpdateAppDatabase((INavAppReference)(object)installContext.AppBeingTargeted), transactionalSession: true, (NavAppInstallationContext instContext, NavSession session) =>
		{
			NavAppInstallationProcessor navAppInstallationProcessor = new NavAppInstallationProcessor(instContext);
			navAppInstallationProcessor.RunAppDatabaseInstallRecordsUpdate(runInSystemSession: false);
		}, postUpgradeActivity: false);
	}
}
