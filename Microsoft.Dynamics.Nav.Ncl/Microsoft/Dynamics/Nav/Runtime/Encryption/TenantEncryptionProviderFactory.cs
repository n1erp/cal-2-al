using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Encryption;

public static class TenantEncryptionProviderFactory
{
	public static ISystemEncryptionProvider GetTenantEncryptionProvider(Func<NavSession> activeSessionProvider = null)
	{
		activeSessionProvider = activeSessionProvider ?? ((Func<NavSession>)(() => NavCurrentThread.Session));
		if (activeSessionProvider().Tenant.TenantSettings.EncryptionProvider == EncryptionProvider.AzureKeyVault)
		{
			NavTenant tenant = activeSessionProvider().Tenant;
			return new TenantAzureKeyVaultEncryptionProvider(tenant.TenantSettings.AzureKeyVaultSettings, tenant.AuthenticationHandler, tenant.TenantSettings.AadTenantId);
		}
		return new TenantRsaEncryptionProvider(activeSessionProvider);
	}
}
