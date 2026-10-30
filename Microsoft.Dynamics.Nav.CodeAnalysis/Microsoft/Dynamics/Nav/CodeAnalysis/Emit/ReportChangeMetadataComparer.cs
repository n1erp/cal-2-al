using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class ReportChangeMetadataComparer
{
	private class ElementInfo
	{
		public int Id { get; }

		public string Name { get; }

		public string ParentPath { get; }

		public string DataType { get; }

		public ReportChangeOptions.ElementType ItemType { get; }

		public XElement DataElement { get; }

		public ElementInfo(int id, string name, ReportChangeOptions.ElementType type, XElement dataElement, string parentPath, string dataType)
		{
			Id = id;
			Name = name;
			ItemType = type;
			DataElement = dataElement;
			ParentPath = parentPath;
			DataType = dataType;
		}
	}

	private enum ElementType
	{
		Parameter,
		DataItem,
		Column
	}

	private static readonly XName ElementIdXname = "ElementId";

	private static readonly XName ElementDataTypeXname = "DataType";

	private static readonly XName ElementTypeXname = "ElementType";

	public static Dictionary<string, ReportChange> CompareReportMetadata(XElement oldXml, XElement newXml, string customXmlRootName)
	{
		Dictionary<string, ReportChange> dictionary = new Dictionary<string, ReportChange>();
		if (oldXml == null)
		{
			return dictionary;
		}
		PooledDictionary<ulong, ElementInfo> instance = PooledDictionary<ulong, ElementInfo>.GetInstance();
		PooledDictionary<ulong, ElementInfo> instance2 = PooledDictionary<ulong, ElementInfo>.GetInstance();
		try
		{
			MapElementsToDictionary(instance, oldXml, string.Empty);
			MapElementsToDictionary(instance2, newXml, string.Empty);
			foreach (KeyValuePair<ulong, ElementInfo> item in instance)
			{
				if (instance2.ContainsKey(item.Key))
				{
					RenameItemIfNeeded(item.Value, instance2[item.Key], dictionary);
				}
				else
				{
					AddDeletedItem(item.Value, dictionary, customXmlRootName);
				}
			}
			return dictionary;
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private static ElementInfo CalculateElementInfo(XElement element, string parentPath)
	{
		string localName = element.Name.LocalName;
		XName xName = element.Name.Namespace + "Labels";
		if (element.Name == xName)
		{
			return null;
		}
		int id = int.Parse((element.Attribute(ElementIdXname) ?? throw new InvalidOperationException("Invalid Custom Xml part format")).Value, CultureInfo.InvariantCulture);
		ReportChangeOptions.ElementType type = ParseElementType(element);
		return new ElementInfo(id, localName, type, element, string.IsNullOrEmpty(parentPath) ? "/" : parentPath, ParseDataType(element));
	}

	private static string ParseDataType(XElement element)
	{
		XAttribute xAttribute = element.Attribute(ElementDataTypeXname);
		if (xAttribute != null)
		{
			return xAttribute.Value;
		}
		return "None";
	}

	private static ReportChangeOptions.ElementType ParseElementType(XElement element)
	{
		XAttribute xAttribute = element.Attribute(ElementTypeXname);
		if (xAttribute == null)
		{
			XName obj = element.GetDefaultNamespace() + "Labels";
			if (element.Parent != null && element.Parent.Name.Equals(obj))
			{
				return ReportChangeOptions.ElementType.Label;
			}
			return ReportChangeOptions.ElementType.Field;
		}
		if (!Enum.TryParse<ElementType>(xAttribute.Value, out var result))
		{
			throw new NotSupportedException();
		}
		return result switch
		{
			ElementType.DataItem => ReportChangeOptions.ElementType.DataItem, 
			ElementType.Column => ReportChangeOptions.ElementType.Field, 
			ElementType.Parameter => ReportChangeOptions.ElementType.Label, 
			_ => throw new NotSupportedException(), 
		};
	}

	private static void MapElementsToDictionary(Dictionary<ulong, ElementInfo> oldElements, XElement oldXml, string parentPath)
	{
		foreach (XElement item in oldXml.Elements())
		{
			ElementInfo elementInfo = CalculateElementInfo(item, parentPath);
			if (elementInfo != null)
			{
				ulong key = CalculateKey(elementInfo);
				oldElements.Add(key, elementInfo);
			}
			if (item.HasElements)
			{
				MapElementsToDictionary(oldElements, item, parentPath + "/" + item.Name.LocalName);
			}
		}
	}

	private static ulong CalculateKey(ElementInfo elementInfo)
	{
		return (ulong)(((long)(elementInfo.ItemType + 1) << 32) + elementInfo.Id);
	}

	private static bool RenameItemIfNeeded(ElementInfo oldElement, ElementInfo newElement, Dictionary<string, ReportChange> renameEntries)
	{
		string parentPath = oldElement.ParentPath;
		string parentPath2 = newElement.ParentPath;
		if (oldElement.Name != newElement.Name || parentPath != parentPath2 || oldElement.ItemType != newElement.ItemType)
		{
			ReportChange reportChange = new ReportChange(oldElement.Id.ToString(CultureInfo.CurrentCulture), oldElement.Name, newElement.Name, parentPath, parentPath2, oldElement.ItemType, newElement.DataType);
			string key = GetKey(reportChange);
			if (!renameEntries.ContainsKey(key))
			{
				reportChange.ActionOnChange = ReportChangeOptions.ChangeAction.Rename;
				renameEntries.Add(key, reportChange);
			}
			return true;
		}
		return false;
	}

	private static void AddDeletedItem(ElementInfo oldElement, Dictionary<string, ReportChange> renameEntries, string customXmlRootName)
	{
		string text = oldElement.ParentPath;
		if (string.IsNullOrEmpty(text))
		{
			List<string> list = new List<string>();
			XElement xElement = oldElement.DataElement;
			while ((xElement = xElement.Ancestors().FirstOrDefault()) != null && xElement.Name.LocalName != customXmlRootName)
			{
				list.Insert(0, xElement.Name.LocalName);
			}
			text = "/" + string.Join("/", list);
		}
		ReportChange reportChange = new ReportChange(oldElement.Id.ToString(CultureInfo.CurrentCulture), oldElement.Name, string.Empty, text, string.Empty, oldElement.ItemType);
		reportChange.ActionOnChange = ReportChangeOptions.ChangeAction.Delete;
		reportChange.ActionOnDelete = ReportChangeOptions.DeleteAction.None;
		string key = GetKey(reportChange);
		if (!renameEntries.ContainsKey(key))
		{
			renameEntries.Add(key, reportChange);
		}
	}

	private static string GetKey(ReportChange entry)
	{
		return entry.SourceParentPath + entry.SourceName;
	}
}
