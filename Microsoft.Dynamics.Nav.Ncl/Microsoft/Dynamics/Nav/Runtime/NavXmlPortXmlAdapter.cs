using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavXmlPortXmlAdapter : INavXmlPortAdapter
{
	private readonly XmlWriter xmlWriter;

	private readonly Dictionary<string, string> nameSpaces = new Dictionary<string, string>();

	private readonly Dictionary<string, string> definedNameSpaces = new Dictionary<string, string>();

	private bool exceptionIsThrown;

	internal NavXmlPortXmlAdapter(NavXmlPort xmlPortInstance, XmlWriterSettings xmlWriterSettings)
	{
		definedNameSpaces = xmlPortInstance.Metadata.Namespaces;
		xmlWriter = XmlWriter.Create(xmlPortInstance.Destination.InternalStreamCheckedForWrite, xmlWriterSettings);
		NavXmlPort.WriteStartDocument(xmlWriter);
	}

	public void WriteAttributeString(NavXmlPortNode node, string value)
	{
		if (string.IsNullOrEmpty(value) && node.MetaData.Occurrence == XmlPortNodeOccurrence.Optional)
		{
			return;
		}
		string nodeName = node.MetaData.NodeName;
		try
		{
			if (nodeName.Contains(":"))
			{
				string[] array = nodeName.Split(':');
				if (array[0] == "xmlns")
				{
					xmlWriter.WriteAttributeString(array[0], array[1], null, value);
					nameSpaces[array[1]] = value;
				}
				else
				{
					xmlWriter.WriteAttributeString(array[1], GetNamespaceUriString(array[0]), value);
				}
			}
			else
			{
				xmlWriter.WriteAttributeString(nodeName, value);
			}
		}
		catch (XmlException ex)
		{
			throw NavNCLXmlPortNodeProcessException.Create(CultureInfo.CurrentCulture, nodeName, ex.InnerException ?? ex);
		}
		catch (ArgumentException ex2)
		{
			throw NavNCLXmlPortNodeProcessException.Create(CultureInfo.CurrentCulture, nodeName, ex2.InnerException ?? ex2);
		}
	}

	public void WriteNamespaceString(string prefix, string namespaceuri)
	{
		if (string.IsNullOrEmpty(prefix))
		{
			xmlWriter.WriteAttributeString("xmlns", namespaceuri);
		}
		else
		{
			xmlWriter.WriteAttributeString("xmlns", prefix, null, namespaceuri);
		}
	}

	public void WriteEndElement(XmlPortNodeSourceType sourceType)
	{
		if (!exceptionIsThrown && xmlWriter.WriteState != WriteState.Error)
		{
			xmlWriter.WriteEndElement();
		}
	}

	public void WriteRaw(string data)
	{
		xmlWriter.WriteRaw(data);
	}

	public void WriteStartElement(string prefix, string localName, XmlPortNodeSourceType sourceType)
	{
		if (localName.Contains(":"))
		{
			string[] array = localName.Split(':');
			xmlWriter.WriteStartElement(array[1], GetNamespaceUriString(array[0]));
		}
		else if (!string.IsNullOrWhiteSpace(prefix))
		{
			xmlWriter.WriteStartElement(prefix, localName, GetNamespaceUriStringFromDefinedNamespaces(prefix));
		}
		else
		{
			xmlWriter.WriteStartElement(localName);
		}
	}

	public void WriteStartElement(string prefix, string localName, string ns, XmlPortNodeSourceType sourceType)
	{
		if (localName.Contains(":"))
		{
			string[] array = localName.Split(':');
			xmlWriter.WriteStartElement(array[0], array[1], ns);
		}
		else if (!string.IsNullOrWhiteSpace(prefix))
		{
			xmlWriter.WriteStartElement(localName, GetNamespaceUriStringFromDefinedNamespaces(prefix));
		}
		else
		{
			xmlWriter.WriteStartElement(localName, ns);
		}
	}

	public void WriteString(string text, NavXmlPortNode node)
	{
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				xmlWriter.WriteString(text);
			}
			catch (ArgumentException ex)
			{
				exceptionIsThrown = true;
				throw new NavNCLInvalidValueException(string.Format(CultureInfo.CurrentCulture, Lang.XmlIsInvalid, text, ex.Message), ex);
			}
		}
	}

	public void WriteNewLine()
	{
	}

	public void WriteTableSeparator()
	{
	}

	public void Flush()
	{
		xmlWriter.Flush();
	}

	public void Close()
	{
		xmlWriter.Close();
	}

	private string GetNamespaceUriString(string prefix)
	{
		if (!nameSpaces.TryGetValue(prefix, out var value))
		{
			return prefix;
		}
		return value;
	}

	private string GetNamespaceUriStringFromDefinedNamespaces(string prefix)
	{
		if (!definedNameSpaces.TryGetValue(prefix, out var value))
		{
			return prefix;
		}
		return value;
	}
}
