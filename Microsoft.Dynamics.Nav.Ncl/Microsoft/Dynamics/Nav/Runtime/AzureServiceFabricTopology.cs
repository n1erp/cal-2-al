using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class AzureServiceFabricTopology : IServiceTopology
{
	private const string ServiceFabricAllowedUsersGroupName = "ServiceFabricAllowedUsers";

	public bool IsServiceRunningInLocalEnvironment => false;

	public bool IsMediaStoredInAzure => true;

	public bool IsUniqueAuthenticationEmailRequired => false;

	public string DomainDirectoryAccessIdentity => "ServiceFabricAllowedUsers";

	public bool EnableAdministrationAuthenticationDelegation => false;

	public bool UseServiceFabricServerLicense => true;

	public bool AllowToRegisterServicePrincipalName => true;

	public bool ReplaceReverseProxyUriWithLimitedReverseProxyUri => false;

	public bool EnableInternalApi => true;

	public string LocalNavAdministratorGroupName => "NavAdministrators";

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
