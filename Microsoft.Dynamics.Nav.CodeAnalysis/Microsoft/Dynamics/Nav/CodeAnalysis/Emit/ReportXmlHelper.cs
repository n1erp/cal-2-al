using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class ReportXmlHelper
{
	private const string ReportSectionsNodeSelector = "//a:ReportSections";

	private const string ReportParametersNodeSelector = "//a:ReportParameters";

	private const string ReportParametersLayoutNodeSelector = "//a:ReportParametersLayout";

	private const string DataSetsNodeSelector = "//a:DataSets";

	private const string DataSourceNodeSelector = "//a:DataSource";

	private const string DataSourcesNodeSelector = "//a:DataSources";

	[SuppressMessage("Microsoft.Security.Xml", "CA3053:UseXmlSecureResolver")]
	[SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
	public static XmlDocument ApplyReportDataSet(string reportLayout, string newDataSet)
	{
		XmlDocument xmlDocument = new XmlDocument
		{
			XmlResolver = null
		};
		xmlDocument.Load(CreateSafeXmlReader(reportLayout.TrimEnd(default(char))));
		return ApplyReportDataSet(xmlDocument, newDataSet);
	}

	[SuppressMessage("Microsoft.Security.Xml", "CA3053:UseXmlSecureResolver")]
	[SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
	public static XmlDocument ApplyReportDataSet(XmlDocument document, string newDataSet)
	{
		XmlNode xmlNode = FlushNodeInDocument(document, "//a:DataSets");
		if (xmlNode == null)
		{
			xmlNode = document.CreateElement(string.Empty, "DataSets", GetRdlcSchemaName(document));
			document.DocumentElement.AppendChild(xmlNode);
		}
		XmlDocument xmlDocument = new XmlDocument
		{
			XmlResolver = null
		};
		if (newDataSet.Length > 0)
		{
			xmlDocument.Load(CreateSafeXmlReader(newDataSet));
			xmlNode.InnerXml = xmlDocument.DocumentElement.InnerXml;
		}
		XmlNamespaceManager namespaceManager = GetNamespaceManager(document, GetRdlcSchemaName(document));
		XmlNode xmlNode2 = document.SelectSingleNode("//a:DataSources", namespaceManager);
		if (xmlNode2 != null)
		{
			XmlNode xmlNode3 = xmlNode2.SelectSingleNode("//a:DataSource", namespaceManager);
			if (xmlNode3 != null)
			{
				XmlAttributeCollection attributes = xmlNode3.Attributes;
				if (attributes != null)
				{
					foreach (XmlAttribute item in attributes)
					{
						if (item.Name.Equals("Name", StringComparison.Ordinal) && item.Value.Equals("DummyDataSource", StringComparison.Ordinal))
						{
							item.Value = "DataSource";
						}
					}
				}
			}
		}
		return document;
	}

	[SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
	public static XmlDocument ApplyReportParameters(XmlDocument document, IEnumerable<string> reportParameters)
	{
		XmlNode xmlNode = FlushNodeInDocument(document, "//a:ReportParameters");
		if (xmlNode != null)
		{
			xmlNode.ParentNode?.RemoveChild(xmlNode);
			xmlNode = null;
		}
		string rdlcSchemaName = GetRdlcSchemaName(document);
		string[] array = reportParameters.ToArray();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (xmlNode == null)
			{
				xmlNode = document.CreateElement(string.Empty, "ReportParameters", rdlcSchemaName);
				XmlNode xmlNode2 = FindNodeInDocument(document, "//a:ReportSections");
				if (xmlNode2 != null)
				{
					document.DocumentElement?.InsertAfter(xmlNode, xmlNode2);
				}
				else
				{
					document.DocumentElement?.AppendChild(xmlNode);
				}
			}
			XmlElement xmlElement = document.CreateElement(string.Empty, "ReportParameter", rdlcSchemaName);
			xmlElement.SetAttribute("Name", text);
			XmlElement xmlElement2 = document.CreateElement(string.Empty, "DataType", rdlcSchemaName);
			xmlElement2.AppendChild(document.CreateTextNode("String"));
			xmlElement.AppendChild(xmlElement2);
			XmlElement xmlElement3 = document.CreateElement(string.Empty, "DefaultValue", rdlcSchemaName);
			xmlElement.AppendChild(xmlElement3);
			XmlElement xmlElement4 = document.CreateElement(string.Empty, "Values", rdlcSchemaName);
			xmlElement3.AppendChild(xmlElement4);
			XmlElement xmlElement5 = document.CreateElement(string.Empty, "Value", rdlcSchemaName);
			xmlElement5.AppendChild(document.CreateTextNode(text));
			xmlElement4.AppendChild(xmlElement5);
			XmlElement xmlElement6 = document.CreateElement(string.Empty, "Prompt", rdlcSchemaName);
			xmlElement6.AppendChild(document.CreateTextNode(text));
			xmlElement.AppendChild(xmlElement6);
			xmlNode.AppendChild(xmlElement);
		}
		return ApplyReportParametersLayout(document, array);
	}

	[SuppressMessage("Microsoft.Security.Xml", "CA3053:UseXmlSecureResolver")]
	public static XmlReader CreateSafeXmlReader(string text)
	{
		return XmlReader.Create(new StringReader(text), new XmlReaderSettings
		{
			DtdProcessing = DtdProcessing.Prohibit,
			XmlResolver = null
		});
	}

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rdlc")]
	public static string GetRdlcSchemaName(IXPathNavigable docNavigator)
	{
		XPathNavigator xPathNavigator = docNavigator.CreateNavigator();
		xPathNavigator.MoveToFirstChild();
		return xPathNavigator.NamespaceURI;
	}

	[SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
	public static XmlNamespaceManager GetNamespaceManager(XmlDocument document, string schemaName)
	{
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(document.NameTable);
		xmlNamespaceManager.AddNamespace("a", schemaName);
		return xmlNamespaceManager;
	}

	[SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
	private static XmlDocument ApplyReportParametersLayout(XmlDocument document, string[] reportParameterArray)
	{
		XmlNode xmlNode = FlushNodeInDocument(document, "//a:ReportParametersLayout");
		if (xmlNode == null)
		{
			return document;
		}
		xmlNode.ParentNode?.RemoveChild(xmlNode);
		if (reportParameterArray.Length == 0)
		{
			return document;
		}
		string rdlcSchemaName = GetRdlcSchemaName(document);
		xmlNode = document.CreateElement(string.Empty, "ReportParametersLayout", rdlcSchemaName);
		XmlNode xmlNode2 = FindNodeInDocument(document, "//a:ReportParameters");
		if (xmlNode2 != null)
		{
			document.DocumentElement?.InsertAfter(xmlNode, xmlNode2);
		}
		else
		{
			document.DocumentElement?.AppendChild(xmlNode);
		}
		XmlNode xmlNode3 = document.CreateElement(string.Empty, "GridLayoutDefinition", rdlcSchemaName);
		xmlNode.AppendChild(xmlNode3);
		XmlElement xmlElement = document.CreateElement(string.Empty, "NumberOfColumns", rdlcSchemaName);
		xmlElement.AppendChild(document.CreateTextNode("1"));
		xmlNode3.AppendChild(xmlElement);
		XmlElement xmlElement2 = document.CreateElement(string.Empty, "NumberOfRows", rdlcSchemaName);
		xmlElement2.AppendChild(document.CreateTextNode(reportParameterArray.Length.ToString(CultureInfo.InvariantCulture)));
		xmlNode3.AppendChild(xmlElement2);
		XmlNode xmlNode4 = document.CreateElement(string.Empty, "CellDefinitions", rdlcSchemaName);
		xmlNode3.AppendChild(xmlNode4);
		for (int i = 0; i < reportParameterArray.Length; i++)
		{
			XmlElement xmlElement3 = document.CreateElement(string.Empty, "CellDefinition", rdlcSchemaName);
			XmlElement xmlElement4 = document.CreateElement(string.Empty, "ColumnIndex", rdlcSchemaName);
			xmlElement4.AppendChild(document.CreateTextNode("0"));
			xmlElement3.AppendChild(xmlElement4);
			XmlElement xmlElement5 = document.CreateElement(string.Empty, "RowIndex", rdlcSchemaName);
			xmlElement5.AppendChild(document.CreateTextNode(i.ToString(CultureInfo.InvariantCulture)));
			xmlElement3.AppendChild(xmlElement5);
			XmlElement xmlElement6 = document.CreateElement(string.Empty, "ParameterName", rdlcSchemaName);
			xmlElement6.AppendChild(document.CreateTextNode(reportParameterArray[i]));
			xmlElement3.AppendChild(xmlElement6);
			xmlNode4.AppendChild(xmlElement3);
		}
		return document;
	}

	private static XmlNode FlushNodeInDocument(XmlDocument document, string selectionToFlush)
	{
		XmlNode xmlNode = FindNodeInDocument(document, selectionToFlush);
		xmlNode?.RemoveAll();
		return xmlNode;
	}

	private static XmlNode FindNodeInDocument(XmlDocument document, string searchString)
	{
		string rdlcSchemaName = GetRdlcSchemaName(document);
		XmlNamespaceManager namespaceManager = GetNamespaceManager(document, rdlcSchemaName);
		return document.SelectSingleNode(searchString, namespaceManager);
	}
}
