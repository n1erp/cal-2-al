using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.Office;

public static class OfficeCustomXmlPart
{
	private enum ElementType
	{
		Parameter,
		Column,
		DataItem
	}

	private class FieldInfo
	{
		public int Id { get; set; }

		public NavType Type { get; set; }
	}

	internal static readonly CultureInfo MetadataCulture = CultureInfo.InvariantCulture;

	private static readonly XName DataItemElementXNameExternal = "DataItemElement";

	private static readonly XName DataFieldXNameExternal = "DataField";

	private static readonly XName NameXNameExternal = "Name";

	private static readonly XName DesignIdXNameExternal = "ID";

	private static readonly XName ElementIdXName = "ElementId";

	private static readonly XName ElementTypeXName = "ElementType";

	private static readonly XName DataTypeXName = "DataType";

	private static readonly XName DataItemXName = "DataItem";

	private static readonly XName LablesXName = "Labels";

	private static readonly XName IndentationXName = "Indentation";

	private static readonly XName FieldTypeXName = "FieldType";

	public static XName DataItemElementXName => DataItemElementXNameExternal;

	public static XName DataFieldXName => DataFieldXNameExternal;

	public static XName NameXName => NameXNameExternal;

	public static XName DesignIdXName => DesignIdXNameExternal;

	public static string Generate(MetaReport report, bool writeMergeInformation)
	{
		string text = FormatXmlNamespace(report.ExternalizedName, report.Id);
		Dictionary<string, int> dictionary = null;
		StringBuilder stringBuilder = new StringBuilder();
		using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings
		{
			Encoding = Encoding.Unicode,
			Indent = true
		}))
		{
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("NavWordReportXmlPart", text);
			if (report.Labels.Count > 0)
			{
				xmlWriter.WriteStartElement("Labels");
				foreach (MetaReportLabel item in report.Labels.OrderBy((MetaReportLabel label) => label.Name))
				{
					int num = item.Id;
					if (num == 0)
					{
						if (dictionary == null)
						{
							dictionary = PopulateFieldDictionary(report.RootDataItems);
						}
						num = GetLabelIdFromField(dictionary, item.Name);
					}
					WriteColumnElement(xmlWriter, text, item.Name, ElementType.Parameter, num.ToString(MetadataCulture), NavType.String, writeMergeInformation);
				}
				xmlWriter.WriteEndElement();
			}
			foreach (MetaDataItem rootDataItem in report.RootDataItems)
			{
				WriteDataItem(xmlWriter, text, rootDataItem, writeMergeInformation);
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
		}
		return stringBuilder.ToString();
	}

	private static int GetLabelIdFromField(Dictionary<string, int> labelMap, string label)
	{
		if (labelMap.TryGetValue(label, out var value))
		{
			return value;
		}
		return -1;
	}

	private static Dictionary<string, int> PopulateFieldDictionary(IEnumerable<MetaDataItem> rootDataItems)
	{
		Dictionary<string, int> seed = new Dictionary<string, int>();
		return rootDataItems.Aggregate(seed, PopulateFieldDictionary);
	}

	private static Dictionary<string, int> PopulateFieldDictionary(Dictionary<string, int> labelMap, MetaDataItem dataItem)
	{
		foreach (MetaDataItemColumn item in dataItem.DataItemColumns.Where((MetaDataItemColumn column) => !column.IsDecimalFormatter))
		{
			labelMap.Add(item.Name + "Caption", item.Id);
		}
		return dataItem.ChildDataItems.Aggregate(labelMap, PopulateFieldDictionary);
	}

	public static XDocument Generate(XElement reportDesignMetadata, bool writeMergeInformation)
	{
		if (reportDesignMetadata == null)
		{
			return null;
		}
		IEnumerable<XElement> enumerable = reportDesignMetadata.Descendants(DataItemXName);
		IList<XElement> source = (enumerable as IList<XElement>) ?? enumerable.ToList();
		if (!source.Any())
		{
			return null;
		}
		XElement xElement = source.First();
		if (xElement == null)
		{
			return null;
		}
		XNamespace xNamespace = FormatXmlNamespace(MetaBase.ExternalizeName(GetName(reportDesignMetadata)), GetElementId(reportDesignMetadata));
		XElement xElement2 = new XElement(xNamespace + "NavWordReportXmlPart");
		XDocument result = new XDocument(new XDeclaration("1.0", "utf-16", "true"), xElement2);
		Dictionary<string, FieldInfo> dictionary = new Dictionary<string, FieldInfo>();
		Dictionary<string, FieldInfo> dictionary2 = new Dictionary<string, FieldInfo>();
		int num = 0;
		XElement xElement3 = null;
		bool flag = true;
		foreach (XElement item in xElement.Elements())
		{
			int elementId = GetElementId(item);
			string name = GetName(item);
			int indentation = GetIndentation(item);
			if (item.Name == DataItemElementXName)
			{
				if (xElement3 == null)
				{
					xElement3 = new XElement(xNamespace + MetaBase.ExternalizeName(name));
					SetElementAttributes(xElement3, elementId, ElementType.DataItem, writeMergeInformation);
					xElement2.Add(xElement3);
				}
				if (!flag)
				{
					FeedListToElement(xNamespace, xElement3, dictionary2, ElementType.Column, writeMergeInformation);
					dictionary2.Clear();
					int num2 = num - indentation;
					while (num2-- >= 0 && xElement3.Parent != null)
					{
						xElement3 = xElement3.Parent;
					}
					XElement xElement4 = new XElement(xNamespace + MetaBase.ExternalizeName(name));
					SetElementAttributes(xElement4, elementId, ElementType.DataItem, writeMergeInformation);
					xElement3.Add(xElement4);
					xElement3 = xElement4;
					num = indentation;
				}
				flag = false;
			}
			else
			{
				if (!(item.Name == DataFieldXName))
				{
					continue;
				}
				bool flag2 = FieldIsParameter(item);
				FieldInfo fieldInfo = new FieldInfo
				{
					Id = elementId,
					Type = ParseFieldType(item)
				};
				if (flag2)
				{
					dictionary.Add(name, fieldInfo);
					fieldInfo.Type = NavType.TextConst;
					continue;
				}
				if (elementId > 0 && !dictionary2.ContainsKey(name))
				{
					dictionary2.Add(name, fieldInfo);
				}
				if (FieldIsCaption(item, out var captionName) && !dictionary.ContainsKey(captionName))
				{
					fieldInfo.Type = NavType.TextConst;
					dictionary.Add(captionName, fieldInfo);
				}
			}
		}
		if (dictionary2.Count > 0 && xElement3 != null)
		{
			FeedListToElement(xNamespace, xElement3, dictionary2, ElementType.Column, writeMergeInformation);
		}
		if (dictionary.Count > 0)
		{
			XElement xElement5 = new XElement(xNamespace + LablesXName.ToString());
			FeedListToElement(xNamespace, xElement5, dictionary, ElementType.Parameter, writeMergeInformation);
			xElement2.AddFirst(xElement5);
		}
		return result;
	}

	private static NavType ParseFieldType(XElement oldDataItemElement)
	{
		string fieldType = GetFieldType(oldDataItemElement);
		if (string.IsNullOrEmpty(fieldType))
		{
			return NavType.None;
		}
		return (NavType)Enum.Parse(typeof(NavType), fieldType);
	}

	public static string FormatXmlNamespace(string externalizedName, int reportId)
	{
		return string.Format(MetadataCulture, "{0}{1}/{2}/", "urn:microsoft-dynamics-nav/reports/", externalizedName, reportId);
	}

	private static void SetElementAttributes(XElement dataItemElement, int id, ElementType elementType, bool writeMergeInformation)
	{
		if (writeMergeInformation)
		{
			dataItemElement.SetAttributeValue(ElementIdXName, id.ToString(MetadataCulture));
			dataItemElement.SetAttributeValue(ElementTypeXName, elementType.ToString());
		}
	}

	private static void SetElementAttributes(XElement dataItemElement, FieldInfo info, ElementType elementType, bool writeMergeInformation)
	{
		if (writeMergeInformation)
		{
			dataItemElement.SetAttributeValue(ElementIdXName, info.Id.ToString(MetadataCulture));
			dataItemElement.SetAttributeValue(DataTypeXName, info.Type.ToString());
			dataItemElement.SetAttributeValue(ElementTypeXName, elementType.ToString());
		}
	}

	public static bool FieldIsParameter(XContainer element)
	{
		XName name = "IsParameter";
		if (!bool.TryParse((string)element.Element(name), out var result))
		{
			return false;
		}
		return result;
	}

	private static bool FieldIsCaption(XElement element, out string captionName)
	{
		XName name = "IncludeCaption";
		string text = (string)element.Element(name);
		if (string.IsNullOrEmpty(text) || string.Compare(text, "Included", StringComparison.Ordinal) != 0)
		{
			captionName = string.Empty;
			return false;
		}
		captionName = GetName(element) + "Caption";
		return true;
	}

	private static void FeedListToElement(XNamespace xmlNamespace, XElement element, Dictionary<string, FieldInfo> fieldNames, ElementType elementType, bool writeMergeInformation)
	{
		foreach (KeyValuePair<string, FieldInfo> item in fieldNames.OrderBy((KeyValuePair<string, FieldInfo> fieldName) => fieldName.Key))
		{
			XElement xElement = ((item.Value.Type == NavType.BLOB) ? new XElement(xmlNamespace + item.Key) : new XElement(xmlNamespace + item.Key, item.Key));
			SetElementAttributes(xElement, item.Value, elementType, writeMergeInformation);
			element.Add(xElement);
		}
	}

	private static int GetIntegerElement(XElement element, XName elementName)
	{
		if (!int.TryParse(((string)element.Element(elementName)) ?? "0", out var result))
		{
			return 0;
		}
		return result;
	}

	private static int GetIndentation(XElement element)
	{
		return GetIntegerElement(element, IndentationXName);
	}

	private static int GetElementId(XElement element)
	{
		return GetIntegerElement(element, DesignIdXName);
	}

	private static string GetName(XElement element)
	{
		return (string)element.Element(NameXName);
	}

	private static string GetFieldType(XElement element)
	{
		return (string)element.Element(FieldTypeXName);
	}

	private static void WriteColumnElement(XmlWriter xmlWriter, string xmlNamespace, string name, ElementType elementType, string identifier, NavType navType, bool writeMergeInformation)
	{
		xmlWriter.WriteStartElement(name, xmlNamespace);
		if (writeMergeInformation)
		{
			xmlWriter.WriteAttributeString(ElementTypeXName.LocalName, elementType.ToString());
			xmlWriter.WriteAttributeString(ElementIdXName.LocalName, identifier);
			xmlWriter.WriteAttributeString(DataTypeXName.LocalName, navType.ToString());
		}
		if (navType != NavType.BLOB)
		{
			xmlWriter.WriteValue(name);
		}
		xmlWriter.WriteEndElement();
	}

	private static void WriteDataItem(XmlWriter xmlWriter, string xmlNamespace, MetaDataItem dataItem, bool writeMergeInformation)
	{
		xmlWriter.WriteStartElement(dataItem.ExternalizedName, xmlNamespace);
		if (writeMergeInformation)
		{
			xmlWriter.WriteAttributeString(ElementTypeXName.LocalName, ElementType.DataItem.ToString());
			xmlWriter.WriteAttributeString(ElementIdXName.LocalName, dataItem.Id.ToString(MetadataCulture));
		}
		foreach (MetaDataItemColumn item in dataItem.DataItemColumns.OrderBy((MetaDataItemColumn column) => column.Name))
		{
			if (!item.IsDecimalFormatter)
			{
				WriteColumnElement(xmlWriter, xmlNamespace, item.Name, ElementType.Column, item.Id.ToString(MetadataCulture), item.FieldType, writeMergeInformation);
			}
		}
		foreach (MetaDataItem childDataItem in dataItem.ChildDataItems)
		{
			WriteDataItem(xmlWriter, xmlNamespace, childDataItem, writeMergeInformation);
		}
		xmlWriter.WriteEndElement();
	}
}
