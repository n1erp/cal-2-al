namespace Microsoft.Dynamics.Nav.Runtime;

public class StandardServiceTopology : IServiceTopology
{
	public bool IsServiceRunningInLocalEnvironment => true;

	public bool IsMediaStoredInAzure => false;

	public bool IsUniqueAuthenticationEmailRequired => true;

	public string DomainDirectoryAccessIdentity => NavEnvironment.ServiceAccountName;

	public bool EnableAdministrationAuthenticationDelegation => false;

	public bool UseServiceFabricServerLicense => false;

	public bool AllowToRegisterServicePrincipalName => true;

	public bool ReplaceReverseProxyUriWithLimitedReverseProxyUri => false;

	public string LocalNavAdministratorGroupName => null;

	public bool EnableInternalApi => false;

	public bool EnableServiceBusCacheSync => false;

	public bool UseExternalTaskScheduler => false;

	public bool CanTrackSessionsInMemory => false;
}
