using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class AzureServiceFabricWithContainersTopology : IServiceTopology
{
	public bool IsServiceRunningInLocalEnvironment => false;

	public bool IsMediaStoredInAzure => true;

	public bool IsUniqueAuthenticationEmailRequired => false;

	public string DomainDirectoryAccessIdentity => NavEnvironment.ServiceAccountName;

	public bool EnableAdministrationAuthenticationDelegation => true;

	public bool UseServiceFabricServerLicense => true;

	public bool AllowToRegisterServicePrincipalName => false;

	public bool ReplaceReverseProxyUriWithLimitedReverseProxyUri => true;

	public bool EnableInternalApi => true;

	public string LocalNavAdministratorGroupName => null;

	public bool EnableServiceBusCacheSync
	{
		get
		{
			if (ServerUserSettings.Instance.EnableServiceBusCacheSync.Value)
			{
				return !ServerUserSettings.Instance.DisableDataCache.Value;
			}
			return false;
		}
	}

	public bool UseExternalTaskScheduler => FeatureSwitches.Instance.EnableExternalTaskScheduler;

	public bool CanTrackSessionsInMemory => true;
}
