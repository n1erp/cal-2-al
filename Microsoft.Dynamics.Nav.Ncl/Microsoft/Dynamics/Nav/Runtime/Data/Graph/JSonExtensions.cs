using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Data.Graph;

internal static class JSonExtensions
{
	public static bool IsNull(this JToken token)
	{
		if (token == null)
		{
			return true;
		}
		return token.Type == JTokenType.Null;
	}
}
