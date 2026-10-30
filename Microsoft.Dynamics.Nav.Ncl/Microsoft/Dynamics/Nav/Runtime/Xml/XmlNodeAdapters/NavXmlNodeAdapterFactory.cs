using System;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Xml.XmlNodeAdapters;

internal static class NavXmlNodeAdapterFactory
{
	public static INavXmlNode CreateAdapter(object rawNode)
	{
		if (rawNode == null)
		{
			return new NavNullNodeAdapter();
		}
		if (rawNode is XDeclaration value)
		{
			return new NavXDeclarationAdapter(value);
		}
		if (rawNode is XAttribute attribute)
		{
			return new NavXAttributeAdapter(attribute);
		}
		if (rawNode is XDocument node)
		{
			return new NavXDocumentAdapter(node);
		}
		if (rawNode is XNode node2)
		{
			return new NavXNodeAdapter<XNode>(node2);
		}
		throw new ArgumentException("An adapter cannot be created from the given value.", "rawNode");
	}
}
