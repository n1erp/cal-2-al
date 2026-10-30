namespace Microsoft.Dynamics.Nav.Runtime;

public enum NavUpgradeEvent
{
	OnCheckPreconditionsPerDatabase,
	OnCheckPreconditionsPerCompany,
	OnUpgradePerDatabase,
	OnUpgradePerCompany,
	OnValidateUpgradePerDatabase,
	OnValidateUpgradePerCompany,
	OnAfterUpgradeCommitPerDatabase,
	OnAfterUpgradeCommitPerCompany
}
