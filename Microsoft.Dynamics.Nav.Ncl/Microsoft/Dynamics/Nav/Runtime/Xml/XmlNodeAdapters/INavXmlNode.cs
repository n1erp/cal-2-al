using System.Xml;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Xml.XmlNodeAdapters;

internal interface INavXmlNode
{
	XmlNodeType NodeType { get; }

	string OuterXml { get; }

	bool WriteTo(DataError errorLevel, ByRef<NavText> data);

	bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, ByRef<NavText> data);

	bool WriteTo(DataError errorLevel, NavOutStream data);

	bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, NavOutStream data);

	bool GetParent(DataError errorLevel, ByRef<NavXmlElement> result);

	bool GetDocument(DataError errorLevel, ByRef<NavXmlDocument> result);

	bool AddAfterSelf(DataError errorLevel, params object[] content);

	bool AddBeforeSelf(DataError errorLevel, params object[] content);

	bool ReplaceWith(DataError errorLevel, params object[] content);

	bool Remove(DataError errorLevel);

	bool SelectSingleNode(DataError errorLevel, string xpath, ByRef<NavXmlNode> result);

	bool SelectSingleNode(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNode> result);

	bool SelectNodes(DataError errorLevel, string xpath, ByRef<NavXmlNodeList> result);

	bool SelectNodes(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNodeList> result);
}
