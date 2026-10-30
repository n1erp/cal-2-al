namespace Microsoft.Dynamics.Nav.Runtime;

public interface IServiceTopology
{
	bool IsServiceRunningInLocalEnvironment { get; }

	bool IsMediaStoredInAzure { get; }

	bool IsUniqueAuthenticationEmailRequired { get; }

	string DomainDirectoryAccessIdentity { get; }

	bool EnableAdministrationAuthenticationDelegation { get; }

	string LocalNavAdministratorGroupName { get; }

	bool UseServiceFabricServerLicense { get; }

	bool AllowToRegisterServicePrincipalName { get; }

	bool ReplaceReverseProxyUriWithLimitedReverseProxyUri { get; }

	bool EnableInternalApi { get; }

	bool EnableServiceBusCacheSync { get; }

	bool UseExternalTaskScheduler { get; }

	bool CanTrackSessionsInMemory { get; }
}
