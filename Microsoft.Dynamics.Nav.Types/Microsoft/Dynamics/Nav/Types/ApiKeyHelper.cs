using System;
using System.Text;

namespace Microsoft.Dynamics.Nav.Types;

public static class ApiKeyHelper
{
	public static string EncodeApiKeyValue(string apiKey)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(FormattableString.Invariant($":{apiKey}")));
	}
}
