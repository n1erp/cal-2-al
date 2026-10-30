using System;

namespace Microsoft.Dynamics.Nav.Types;

public static class NavTenantHelper
{
	public const char TenantSeparatorChar = '\\';

	public const string TenantParameter = "tenant";

	public const string SystemTenantId = "nav-systemapplication";

	public const string DefaultTenantId = "default";

	public static string ExtractPropertiesFromPassword(string passwordWithExtendedProperties, out string clientHostName, out string tenantId, out string navAppId)
	{
		if (passwordWithExtendedProperties == null)
		{
			throw new ArgumentNullException("passwordWithExtendedProperties");
		}
		string[] array = passwordWithExtendedProperties.Split('\\');
		navAppId = ((array.Length == 4) ? array[0] : string.Empty);
		clientHostName = ((array.Length > 2) ? array[array.Length - 3] : string.Empty);
		tenantId = ((array.Length > 1) ? array[array.Length - 2] : string.Empty);
		return array[array.Length - 1];
	}

	public static string ExtractTenantIdFromUserName(string userNameWithTenantId, out string tenantId)
	{
		if (userNameWithTenantId == null)
		{
			throw new ArgumentNullException("userNameWithTenantId");
		}
		int num = userNameWithTenantId.IndexOf('\\');
		if (num < 0)
		{
			tenantId = string.Empty;
			return userNameWithTenantId;
		}
		tenantId = userNameWithTenantId.Substring(0, num);
		return userNameWithTenantId.Substring(num + 1);
	}
}
