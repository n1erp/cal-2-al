using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class WordDocumentXmlHelper
{
	public static string NormalizeName(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return name.Replace("ns0:", string.Empty).Replace("[1]", string.Empty);
		}
		return name;
	}

	internal static string GetNamespaceFromPrefixMap(string prefixMappings)
	{
		if (string.IsNullOrEmpty(prefixMappings))
		{
			return string.Empty;
		}
		string result = prefixMappings;
		int num = prefixMappings.IndexOf('\'', 0);
		if (num >= 0)
		{
			int num2 = prefixMappings.IndexOf('\'', num + 1);
			num2 = ((num2 >= 0) ? (num2 - 1) : (prefixMappings.Length - 1));
			result = prefixMappings.Substring(num + 1, num2 - num);
		}
		return result;
	}

	internal static bool IsNavNameSpace(string prefixMappings, string customXmlNamespaceStart)
	{
		if (string.IsNullOrEmpty(prefixMappings))
		{
			return false;
		}
		return GetNamespaceFromPrefixMap(prefixMappings).StartsWith(customXmlNamespaceStart, StringComparison.Ordinal);
	}
}
