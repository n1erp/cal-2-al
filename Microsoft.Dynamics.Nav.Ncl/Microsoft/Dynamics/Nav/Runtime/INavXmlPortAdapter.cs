using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface INavXmlPortAdapter
{
	void WriteAttributeString(NavXmlPortNode node, string value);

	void WriteNamespaceString(string prefix, string namespaceuri);

	void WriteEndElement(XmlPortNodeSourceType sourceType);

	void WriteRaw(string data);

	void WriteStartElement(string prefix, string localName, XmlPortNodeSourceType sourceType);

	void WriteStartElement(string prefix, string localName, string ns, XmlPortNodeSourceType sourceType);

	void WriteString(string text, NavXmlPortNode node);

	void WriteNewLine();

	void WriteTableSeparator();

	void Flush();

	void Close();
}
