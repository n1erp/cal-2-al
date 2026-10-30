namespace Microsoft.Dynamics.Nav.Types;

public enum ChangeNavAppStateResult
{
	AppChangeSuccessful,
	DependenciesFoundWithoutForceSet,
	DependencyCompatibilityError,
	AppNotFound,
	MultipleAppsFound,
	AppAlreadyChangedError,
	AppAlreadyChangedWarning,
	MetadataOutOfSync,
	TenantSyncFailure,
	EvaluationError,
	LanguageNotSupported,
	SymbolsPackageCannotBeInstalled,
	UnknownAppError,
	AppChangeInProgress,
	AppNotSynced,
	UpgradeNoOldVersionFound,
	AppUpgradeCodeExecutionFailure,
	SyncedVersionTooOld,
	SyncBaselineVersionTooNew,
	UpgradeTargetOlderThanCurrent,
	UnsatisfiedDependenciesForUpgrade,
	UnsatisfiedDependenciesForInstall,
	UpgradeNotSupported,
	InstallOlderVersionUninstalled,
	InstallNewerVersionUninstalled,
	UpgradeMustUninstallV1Extension,
	AppInstallCodeExecutionFailure,
	V1ExtensionsUnsupported
}
