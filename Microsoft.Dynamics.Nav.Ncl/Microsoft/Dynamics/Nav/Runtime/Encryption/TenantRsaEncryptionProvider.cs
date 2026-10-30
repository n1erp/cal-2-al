using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Encryption;

public class TenantRsaEncryptionProvider : RsaEncryptionProviderBase
{
	private const string DefaultKeyFileName = "{0}_{1}.key";

	private readonly Func<NavSession> currentSessionProvider;

	protected override string StoredEncryptionFileName
	{
		get
		{
			return currentSessionProvider().Tenant.GetEncryptionKeyFileName();
		}
		set
		{
			currentSessionProvider().Tenant.SetEncryptionKeyFileName(value);
		}
	}

	protected override string StoredEncryptionKeyHash
	{
		get
		{
			return currentSessionProvider().Tenant.GetEncryptionKeyHash();
		}
		set
		{
			currentSessionProvider().Tenant.SetEncryptionKeyHash(value);
		}
	}

	public TenantRsaEncryptionProvider(Func<NavSession> activeSessionProvider = null)
	{
		currentSessionProvider = activeSessionProvider ?? ((Func<NavSession>)(() => NavCurrentThread.Session));
		Id = currentSessionProvider().Tenant.Id;
	}

	protected override string CreateEncryptionFileName()
	{
		string text;
		do
		{
			text = string.Format(CultureInfo.InvariantCulture, "{0}_{1}.key", Id, Guid.NewGuid());
		}
		while (KeyFileExists(text));
		return text;
	}
}
