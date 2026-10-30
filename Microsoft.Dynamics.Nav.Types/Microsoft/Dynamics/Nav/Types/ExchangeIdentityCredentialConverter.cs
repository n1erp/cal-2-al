using System;

namespace Microsoft.Dynamics.Nav.Types;

public static class ExchangeIdentityCredentialConverter
{
	public const string IdentityIdentifierUserNamePrefix = "EXCH_ID:";

	public static void ConstructUserNamePasswordFromToken(string token, out string userName, out string password)
	{
		userName = "EXCH_ID:" + Guid.NewGuid().ToString();
		password = token;
	}
}
