using System;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class XObjectExtensions
{
	public static string GetPrefix(this XAttribute attribute)
	{
		if (string.IsNullOrEmpty(attribute.Name.NamespaceName))
		{
			return string.Empty;
		}
		if (attribute.Parent != null)
		{
			return attribute.Parent.GetPrefixOfNamespace(attribute.Name.NamespaceName) ?? string.Empty;
		}
		string value = ":" + attribute.Name.LocalName + "=\"";
		string text = attribute.ToString();
		int length = text.LastIndexOf(value, StringComparison.Ordinal);
		return text.Substring(0, length);
	}
}
