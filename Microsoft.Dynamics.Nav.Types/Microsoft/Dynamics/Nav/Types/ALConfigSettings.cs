using System;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class ALConfigSettings
{
	private ALConfigSettings()
	{
	}

	public static ALConfigSettings Instance()
	{
		return new ALConfigSettings();
	}

	public static bool EnableSaasExtensionInstallConfigSetting()
	{
		return ServerUserSettings.Instance.EnableSaasExtensionInstall.Value;
	}

	public static bool IsSaasExcelAddinEnabled()
	{
		if (Guid.TryParse(ServerUserSettings.Instance.ExcelAddInAzureActiveDirectoryClientId.Value, out var result))
		{
			return result != Guid.Empty;
		}
		return false;
	}

	public static bool ApiServicesEnabled()
	{
		return ServerUserSettings.Instance.ApiServicesEnabled.Value;
	}

	public static bool ApiSubscriptionsEnabled()
	{
		return ServerUserSettings.Instance.ApiSubscriptionsEnabled.Value;
	}

	public static int ApiSubscriptionDelayTime()
	{
		return ServerUserSettings.Instance.ApiSubscriptionDelayTime.Value;
	}

	public static int ApiSubscriptionSendingNotificationTimeout()
	{
		return ServerUserSettings.Instance.ApiSubscriptionSendingNotificationTimeout.Value;
	}

	public static int ApiSubscriptionMaxNumberOfNotifications()
	{
		return ServerUserSettings.Instance.ApiSubscriptionMaxNumberOfNotifications.Value;
	}
}
