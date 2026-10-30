using System.Diagnostics;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

[DebuggerDisplay("Module: {ModuleName}, Step: {StepName}")]
public class NavUpgradeActivityStep
{
	public const string BaseModuleName = "BASE";

	public string ModuleName
	{
		[DebuggerStepThrough]
		get;
	}

	public string StepName
	{
		[DebuggerStepThrough]
		get;
	}

	[DebuggerStepThrough]
	private NavUpgradeActivityStep(string moduleName, string stepName)
	{
		ModuleName = moduleName;
		StepName = stepName;
	}

	public static NavUpgradeActivityStep CheckPreconditions(INavAppReference appModule = null)
	{
		return new NavUpgradeActivityStep(((appModule != null) ? NavAppReferenceExtensions.FriendlyDisplayName(appModule) : null) ?? "BASE", "CheckPreconditions");
	}

	public static NavUpgradeActivityStep OnUpgrade(INavAppReference appModule = null)
	{
		return new NavUpgradeActivityStep(((appModule != null) ? NavAppReferenceExtensions.FriendlyDisplayName(appModule) : null) ?? "BASE", "OnUpgrade");
	}

	public static NavUpgradeActivityStep ValidateUpgrade(INavAppReference appModule = null)
	{
		return new NavUpgradeActivityStep(((appModule != null) ? NavAppReferenceExtensions.FriendlyDisplayName(appModule) : null) ?? "BASE", "ValidateUpgrade");
	}

	public static NavUpgradeActivityStep AfterUpgradeCommit(INavAppReference appModule = null)
	{
		return new NavUpgradeActivityStep(((appModule != null) ? NavAppReferenceExtensions.FriendlyDisplayName(appModule) : null) ?? "BASE", "AfterUpgradeCommit");
	}

	public static NavUpgradeActivityStep HandleSpecialRecords(INavAppReference appModule)
	{
		return new NavUpgradeActivityStep(NavAppReferenceExtensions.FriendlyDisplayName(appModule), "HandleSpecialRecords");
	}

	public static NavUpgradeActivityStep ClearGlobalCache(INavAppReference appModule)
	{
		return new NavUpgradeActivityStep(NavAppReferenceExtensions.FriendlyDisplayName(appModule), "ClearGlobalCache");
	}

	public static NavUpgradeActivityStep SetTransactionalPhaseComplete(INavAppReference appModule)
	{
		return new NavUpgradeActivityStep(NavAppReferenceExtensions.FriendlyDisplayName(appModule), "SetTransactionalPhaseComplete");
	}

	public static NavUpgradeActivityStep UpdateAppDatabase(INavAppReference appModule)
	{
		return new NavUpgradeActivityStep(NavAppReferenceExtensions.FriendlyDisplayName(appModule), "UpdateAppDatabase");
	}
}
