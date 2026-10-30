using System.Xml;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Xml.XmlNodeAdapters;

internal class NavNullNodeAdapter : INavXmlNode
{
	public XmlNodeType NodeType => XmlNodeType.None;

	public string OuterXml
	{
		get
		{
			throw new NavNCLNotInitializedException(typeof(NavXmlNode));
		}
	}

	public bool AddAfterSelf(DataError errorLevel, params object[] content)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool AddBeforeSelf(DataError errorLevel, params object[] content)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool GetDocument(DataError errorLevel, ByRef<NavXmlDocument> result)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool GetParent(DataError errorLevel, ByRef<NavXmlElement> result)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool Remove(DataError errorLevel)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool ReplaceWith(DataError errorLevel, params object[] content)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool SelectNodes(DataError errorLevel, string xpath, ByRef<NavXmlNodeList> result)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool SelectNodes(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNodeList> result)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool SelectSingleNode(DataError errorLevel, string xpath, ByRef<NavXmlNode> result)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool SelectSingleNode(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNode> result)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, ByRef<NavText> data)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, NavOutStream data)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool WriteTo(DataError errorLevel, NavOutStream data)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}

	public bool WriteTo(DataError errorLevel, ByRef<NavText> data)
	{
		throw new NavNCLNotInitializedException(typeof(NavXmlNode));
	}
}
