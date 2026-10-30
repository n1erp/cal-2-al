using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public static class NavXmlDeltaUtilities
{
	private class NodeList : List<XmlNode>
	{
		internal NodeList(int capacity)
			: base(capacity)
		{
		}

		internal int IndexOf(string controlId)
		{
			int result = -1;
			for (int i = 0; i < Count; i++)
			{
				if (base[i] is XmlElement xmlElement)
				{
					string attribute = xmlElement.GetAttribute("ControlGUID");
					if (attribute != null && string.Compare(controlId, attribute, StringComparison.OrdinalIgnoreCase) == 0)
					{
						result = i;
						break;
					}
				}
			}
			return result;
		}
	}

	public const string XPathSeparator = "/";

	public const string XPathSimpleExpressionFormat = "{0}:{1}";

	public const string XPathExpressionInnerElementFormat = "{0}:{1}/{0}:{2}";

	public const string ControlGuid = "ControlGUID";

	public const string ControlGuidXPath = "@ControlGUID";

	public const string ShowFilterPane = "ShowFilterPane";

	internal static bool DeepCompare(XmlNode source, XmlNode target)
	{
		if (source.NodeType != target.NodeType)
		{
			return false;
		}
		if (source.LocalName != target.LocalName)
		{
			return false;
		}
		if (source.NamespaceURI != target.NamespaceURI)
		{
			return false;
		}
		if (source.NodeType == XmlNodeType.Attribute && target.NodeType == XmlNodeType.Attribute)
		{
			return string.Compare(source.Value, target.Value, StringComparison.Ordinal) == 0;
		}
		if (source.ChildNodes.Count != target.ChildNodes.Count)
		{
			return false;
		}
		if (source.Attributes.Count != target.Attributes.Count)
		{
			return false;
		}
		foreach (XmlAttribute attribute in source.Attributes)
		{
			XmlAttribute xmlAttribute2 = target.Attributes[attribute.Name];
			if (xmlAttribute2 == null)
			{
				return false;
			}
			if (string.Compare(attribute.Value, xmlAttribute2.Value, StringComparison.Ordinal) != 0)
			{
				return false;
			}
		}
		if (source.ChildNodes.Count == 0 && target.ChildNodes.Count == 0)
		{
			return true;
		}
		for (int i = 0; i < target.ChildNodes.Count; i++)
		{
			if (!DeepCompare(source.ChildNodes[i], target.ChildNodes[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal static XmlNode FindNodeById(XmlNode elementId, string pathId, XmlNodeList elementList, XmlNamespaceManager namespaceManager)
	{
		if (elementId == null || string.IsNullOrEmpty(pathId))
		{
			return null;
		}
		foreach (XmlNode element in elementList)
		{
			XmlNode target = element.SelectSingleNode(pathId, namespaceManager);
			if (DeepCompare(elementId, target))
			{
				return element;
			}
		}
		return null;
	}

	internal static XmlNode ConvertElementIdToXmlNode(NavXmlNode elementId, XmlDocument document)
	{
		if (elementId.Attributes.Count == 1 && elementId.ChildNodes.Count == 0)
		{
			XmlNode xmlNode = document.CreateAttribute(elementId.Attributes[0].Name);
			xmlNode.Value = elementId.Attributes[0].Value;
			return xmlNode;
		}
		if (elementId.ChildNodes.Count == 1)
		{
			XmlNode xmlNode = ConvertNavXmlNodeToElement(elementId, document);
			return xmlNode.ChildNodes[0];
		}
		return null;
	}

	internal static NavXmlNode ConvertElementIdToNavXmlNode(XmlNode elementId)
	{
		if (elementId == null)
		{
			return null;
		}
		NavXmlNode navXmlNode = new NavXmlNode();
		if (elementId is XmlAttribute)
		{
			NavXmlNodeAttribute navXmlNodeAttribute = new NavXmlNodeAttribute();
			navXmlNodeAttribute.Name = elementId.Name;
			navXmlNodeAttribute.Uri = elementId.NamespaceURI;
			navXmlNodeAttribute.Value = elementId.Value;
			navXmlNode.Attributes.Add(navXmlNodeAttribute);
			return navXmlNode;
		}
		navXmlNode.Name = elementId.Name;
		navXmlNode.Uri = elementId.NamespaceURI;
		foreach (XmlAttribute attribute in elementId.Attributes)
		{
			NavXmlNodeAttribute navXmlNodeAttribute2 = new NavXmlNodeAttribute();
			navXmlNodeAttribute2.Name = attribute.Name;
			navXmlNodeAttribute2.Uri = attribute.NamespaceURI;
			navXmlNodeAttribute2.Value = attribute.Value;
			navXmlNode.Attributes.Add(navXmlNodeAttribute2);
		}
		foreach (XmlNode childNode in elementId.ChildNodes)
		{
			if (childNode.Attributes.Count > 0 || childNode.ChildNodes.Count > 0)
			{
				NavXmlNode item = ConvertElementIdToNavXmlNode(childNode);
				navXmlNode.ChildNodes.Add(item);
			}
		}
		return navXmlNode;
	}

	internal static bool IsValidInnerNode(XmlNode childNode)
	{
		if (string.IsNullOrEmpty(GetControlId(childNode)))
		{
			if (childNode.Attributes == null || childNode.Attributes.Count <= 0)
			{
				return childNode.ChildNodes.Count > 0;
			}
			return true;
		}
		return false;
	}

	private static XmlElement ConvertNavXmlNodeToElement(NavXmlNode navXmlNode, XmlDocument document)
	{
		XmlElement xmlElement = document.CreateElement(navXmlNode.Name, navXmlNode.Uri);
		foreach (NavXmlNodeAttribute attribute in navXmlNode.Attributes)
		{
			XmlAttribute xmlAttribute = document.CreateAttribute(attribute.Name, attribute.Uri);
			xmlAttribute.Value = attribute.Value;
			xmlElement.Attributes.Append(xmlAttribute);
		}
		foreach (NavXmlNode childNode in navXmlNode.ChildNodes)
		{
			XmlElement newChild = ConvertNavXmlNodeToElement(childNode, document);
			xmlElement.AppendChild(newChild);
		}
		return xmlElement;
	}

	internal static NavXmlNode ConvertXmlNodeToNavXmlNode(XmlNode node, IDictionary<string, XmlElement> sourceNodes, IXmlDeltaCalculationRules rules)
	{
		if (rules.IsAllowedAsAddDelta(node.Name))
		{
			return null;
		}
		NavXmlNode navXmlNode = new NavXmlNode();
		navXmlNode.Name = node.Name;
		navXmlNode.Uri = node.NamespaceURI;
		navXmlNode.Id = GetControlId(node);
		foreach (XmlAttribute attribute in node.Attributes)
		{
			NavXmlNodeAttribute navXmlNodeAttribute = new NavXmlNodeAttribute();
			navXmlNodeAttribute.Name = attribute.Name;
			navXmlNodeAttribute.Uri = attribute.NamespaceURI;
			navXmlNodeAttribute.Value = attribute.Value;
			navXmlNode.Attributes.Add(navXmlNodeAttribute);
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (IsNewNode(childNode, sourceNodes) || IsValidInnerNode(childNode))
			{
				NavXmlNode navXmlNode2 = ConvertXmlNodeToNavXmlNode(childNode, sourceNodes, rules);
				if (navXmlNode2 != null)
				{
					navXmlNode.ChildNodes.Add(navXmlNode2);
				}
			}
		}
		return navXmlNode;
	}

	internal static bool IsNewNode(XmlNode node, IDictionary<string, XmlElement> sourceNodes)
	{
		string controlId = GetControlId(node);
		if (!string.IsNullOrEmpty(controlId))
		{
			return !sourceNodes.ContainsKey(controlId);
		}
		return false;
	}

	internal static string GetControlId(XmlNode node)
	{
		if (node == null || node.Attributes == null)
		{
			return null;
		}
		return node.Attributes["ControlGUID"]?.Value;
	}

	internal static IDictionary<string, XmlElement> GetControlElements(XmlDocument document)
	{
		Dictionary<string, XmlElement> result = new Dictionary<string, XmlElement>();
		GetControlNodes("ControlGUID", document, result);
		GetControlNodes("ShowFilterPane", document, result);
		return result;
	}

	private static void GetControlNodes(string identifier, XmlNode node, IDictionary<string, XmlElement> result)
	{
		XmlNodeList xmlNodeList = node.SelectNodes("//*[@" + identifier + "]");
		foreach (XmlElement item in xmlNodeList)
		{
			string attribute = item.GetAttribute("ControlGUID");
			if (!result.ContainsKey(attribute))
			{
				result.Add(attribute, item);
			}
		}
	}

	internal static string[] GetChildNodeOrder(XmlElement element)
	{
		StringCollection stringCollection = new StringCollection();
		for (int i = 0; i < element.ChildNodes.Count; i++)
		{
			if (element.ChildNodes[i] is XmlElement xmlElement)
			{
				string attribute = xmlElement.GetAttribute("ControlGUID");
				if (!string.IsNullOrEmpty(attribute))
				{
					stringCollection.Add(attribute);
				}
			}
		}
		string[] array = new string[stringCollection.Count];
		stringCollection.CopyTo(array, 0);
		return array;
	}

	internal static void SetChildNodeOrder(XmlElement element, string[] order)
	{
		if (order.Length == 0 || element.ChildNodes.Count == 0)
		{
			return;
		}
		NodeList nodeList = new NodeList(element.ChildNodes.Count);
		NodeList nodeList2 = new NodeList(element.ChildNodes.Count);
		for (int i = 0; i < element.ChildNodes.Count; i++)
		{
			nodeList.Add(element.ChildNodes[i]);
		}
		for (int j = 0; j < order.Length; j++)
		{
			int num = nodeList.IndexOf(order[j]);
			if (-1 < num)
			{
				nodeList2.Add(nodeList[num]);
			}
		}
		if (!nodeList2.Contains(nodeList[0]))
		{
			nodeList2.Insert(0, nodeList[0]);
		}
		for (int k = 0; k < nodeList.Count; k++)
		{
			int index = nodeList.Count - 1 - k;
			if (nodeList2.Contains(nodeList[index]))
			{
				break;
			}
			int index2 = nodeList2.Count - k;
			nodeList2.Insert(index2, nodeList[index]);
		}
		for (int l = 1; l < nodeList.Count - 1; l++)
		{
			if (!nodeList2.Contains(nodeList[l]))
			{
				int num2 = nodeList2.IndexOf(nodeList[l - 1]);
				nodeList2.Insert(num2 + 1, nodeList[l]);
			}
		}
		foreach (XmlNode childNode in element.ChildNodes)
		{
			element.RemoveChild(childNode);
		}
		for (int m = 0; m < nodeList2.Count; m++)
		{
			element.AppendChild(nodeList2[m]);
		}
	}

	internal static bool IsOrderChanged(string[] sourceNodeChildIds, string[] targetNodeChildIds)
	{
		bool result = false;
		int i = 0;
		if (1 < targetNodeChildIds.Length)
		{
			if (targetNodeChildIds.Length > sourceNodeChildIds.Length)
			{
				return true;
			}
			foreach (string strB in targetNodeChildIds)
			{
				bool flag = false;
				for (; i < sourceNodeChildIds.Length; i++)
				{
					string strA = sourceNodeChildIds[i];
					if (string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	internal static string ArrayToString(string[] array, string separator)
	{
		string result = string.Empty;
		if (array.Length != 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i]);
				if (i < array.Length - 1)
				{
					stringBuilder.Append(separator);
				}
			}
			result = stringBuilder.ToString();
		}
		return result;
	}

	internal static string[] StringToArray(string text, string separator)
	{
		string[] separator2 = new string[1] { separator };
		return text.Split(separator2, StringSplitOptions.RemoveEmptyEntries);
	}

	internal static XmlElement GetParentControlElement(XmlElement element)
	{
		XmlElement result = null;
		if (element.ParentNode is XmlElement xmlElement && xmlElement.HasAttribute("ControlGUID"))
		{
			result = xmlElement;
		}
		return result;
	}

	internal static string GetParentPath(XmlNode element, string namespacePrefix)
	{
		string text = string.Empty;
		while ((element = element.ParentNode) != null && GetControlId(element) == null)
		{
			if (GetControlId(element.ParentNode) == null)
			{
				text += "/";
			}
			text += string.Format(CultureInfo.InvariantCulture, "{0}:{1}", namespacePrefix, element.LocalName);
		}
		return text;
	}
}
