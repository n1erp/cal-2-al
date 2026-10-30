using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Runtime.Designer;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class ConverterHelper
{
	public static Dictionary<string, int> GetDistinctALIdentifier(Dictionary<string, XmlElement> sourceNodes, Dictionary<string, XmlElement> sourceDefNodes, Dictionary<string, string> alIdentifierMapper, string pageName, bool skipUsingALCompiler = false)
	{
		if (sourceDefNodes == null)
		{
			throw new ArgumentNullException("sourceDefNodes");
		}
		if (sourceNodes == null)
		{
			throw new ArgumentNullException("sourceNodes");
		}
		if (alIdentifierMapper == null)
		{
			throw new ArgumentNullException("alIdentifierMapper");
		}
		if (skipUsingALCompiler)
		{
			return GetNamesFromMetadata(sourceNodes, sourceDefNodes, alIdentifierMapper);
		}
		return GetNamesFromALCompiler(sourceNodes, sourceDefNodes, alIdentifierMapper, pageName);
	}

	private static Dictionary<string, int> GetNamesFromMetadata(Dictionary<string, XmlElement> sourceNodes, Dictionary<string, XmlElement> sourceDefNodes, Dictionary<string, string> alIdentifierMapper)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		foreach (string key in sourceDefNodes.Keys)
		{
			if (!sourceDefNodes.TryGetValue(key, out var value) || !sourceNodes.TryGetValue(key, out var value2))
			{
				continue;
			}
			string elementALNameFromMetadata = GetElementALNameFromMetadata(value2, value);
			string elementPrefix = GetElementPrefix(value2);
			if (elementALNameFromMetadata == null || elementPrefix == null)
			{
				continue;
			}
			string text = elementPrefix + elementALNameFromMetadata;
			bool flag = false;
			if (dictionary.ContainsKey(text))
			{
				string text2 = value2.Attributes?["ID"]?.Value;
				text = ((text2 != null) ? (elementPrefix + text2) : null);
				flag = true;
			}
			if (text != null)
			{
				if (flag)
				{
					alIdentifierMapper.Add(key, text);
				}
				else
				{
					alIdentifierMapper.Add(key, elementALNameFromMetadata);
				}
				dictionary.Add(text, 1);
			}
		}
		return dictionary;
	}

	private static Dictionary<string, int> GetNamesFromALCompiler(Dictionary<string, XmlElement> sourceNodes, Dictionary<string, XmlElement> sourceDefNodes, Dictionary<string, string> alIdentifierMapper, string pageName)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		foreach (string key in sourceDefNodes.Keys)
		{
			if (sourceNodes.TryGetValue(key, out var value) && GetElementPrefix(value) != null)
			{
				if (!TryGetNodeInfo(value, pageName, out var elementName, out var elementId))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The AL name of the XML element with ID '{0}' defined in page '{1}' could not be resolved.", key, pageName));
				}
				if (dictionary.ContainsKey(elementId))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The AL name '{0}' resolved for the XML element with ID '{1}' is already defined in page '{2}'.", elementName, key, pageName));
				}
				alIdentifierMapper.Add(key, elementName);
				dictionary.Add(elementId, 1);
			}
		}
		return dictionary;
	}

	private static bool TryGetNodeInfo(XmlElement sourceNode, string pageName, out string elementName, out string elementId)
	{
		if (sourceNode == null)
		{
			throw new ArgumentNullException("sourceNode");
		}
		elementName = null;
		elementId = sourceNode.Attributes?["ID"]?.Value;
		if (elementId != null)
		{
			switch (GetElementPrefix(sourceNode))
			{
			case "Control":
				elementName = NavDesignerUtil.CompilationForALIdentification.ResolvePageControlName(pageName, int.Parse(elementId));
				return true;
			case "Action":
				elementName = NavDesignerUtil.CompilationForALIdentification.ResolvePageActionName(pageName, int.Parse(elementId));
				return true;
			default:
				return false;
			}
		}
		return false;
	}

	public static string ConvertBooleanValue(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		value = value.ToUpperInvariant();
		switch (value)
		{
		case "TRUE":
		case "YES":
		case "1":
			return "true";
		case "FALSE":
		case "NO":
		case "0":
			return "false";
		default:
			return null;
		}
	}

	public static string GetElementALNameFromMetadata(XmlNode element, XmlNode elementDef)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		string elementPrefix = GetElementPrefix(element);
		string text = element.Attributes?["ID"]?.Value;
		string text2 = ((elementPrefix != null && text != null) ? (elementPrefix + text) : null);
		return elementDef?.Attributes?["Name"]?.Value ?? element.Attributes["Caption"]?.Value ?? element?["ALIdentifier"]?.InnerText ?? text2;
	}

	public static string GetElementPrefix(XmlNode element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		string result = null;
		string text = element.Attributes?["xsi:type"]?.Value;
		if (text != null)
		{
			switch (text)
			{
			case "ActionDefinition":
			case "ActionGroupDefinition":
			case "SeparatorDefinition":
				result = "Action";
				break;
			case "ControlDefinition":
			case "ControlGroupDefinition":
			case "InfopartPageDefinition":
			case "InfopartSystemDefinition":
			case "StackGroupDefinition":
			case "RepeaterDefinition":
			case "GridLayoutDefinition":
			case "ColumnLayoutDefinition":
				result = "Control";
				break;
			default:
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Elements of type '{0}' cannot be resolved to AL names.", text));
			}
		}
		return result;
	}

	public static Dictionary<string, XmlElement> GetControlElements(XmlDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		Dictionary<string, XmlElement> result = new Dictionary<string, XmlElement>(StringComparer.OrdinalIgnoreCase);
		GetControlNodes("ControlGUID", document, result);
		GetControlNodes("ShowFilterPane", document, result);
		return result;
	}

	public static HashSet<int> GetIdSet(XmlDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		HashSet<int> result = new HashSet<int>();
		GetIds("ID", document, result);
		return result;
	}

	private static void GetIds(string identifier, XmlNode node, HashSet<int> result)
	{
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		XmlNodeList xmlNodeList = node.SelectNodes("//*[@" + identifier + "]");
		foreach (XmlElement item in xmlNodeList)
		{
			if (int.TryParse(item.GetAttribute("ID"), out var result2))
			{
				result.Add(result2);
			}
		}
	}

	public static void GetControlNodes(string identifier, XmlNode node, IDictionary<string, XmlElement> result)
	{
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
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

	public static string GetAreaKind(XmlElement element, string dest, IDictionary<string, XmlElement> sourceTempNodes)
	{
		string value = null;
		if ((!Enum.TryParse<ControlContainerType>(element.Attributes?["ContainerType"]?.Value, out var result) || !ObjectMapper.AreaKindMapper.TryGetValue(result, out value)) && (!Enum.TryParse<ActionContainerType>(element.Attributes?["ContainerType"]?.Value, out var result2) || !ObjectMapper.ActionAreaKindMapper.TryGetValue(result2, out value)))
		{
			value = GetActionAreaKind(dest, sourceTempNodes);
		}
		return value;
	}

	public static XmlElement ConvertAddNode(Microsoft.Dynamics.Nav.XmlMetadata.NavXmlNode addNode, XmlDocument document, Dictionary<string, XmlElement> sourceNodes, Dictionary<string, XmlElement> sourceDefNodes, Dictionary<string, int> distinctALIdentifier, Dictionary<string, string> alIdentifierMapper)
	{
		XmlElement xmlElement = document.CreateElement(addNode.Name, addNode.Uri);
		foreach (NavXmlNodeAttribute attribute in addNode.Attributes)
		{
			XmlAttribute xmlAttribute = document.CreateAttribute(attribute.Name, attribute.Uri);
			xmlAttribute.Value = attribute.Value;
			xmlElement.Attributes.Append(xmlAttribute);
		}
		foreach (Microsoft.Dynamics.Nav.XmlMetadata.NavXmlNode childNode in addNode.ChildNodes)
		{
			XmlElement newChild = ConvertAddNode(childNode, document, sourceNodes, sourceDefNodes, distinctALIdentifier, alIdentifierMapper);
			xmlElement.AppendChild(newChild);
		}
		string text = xmlElement.Attributes?["Name"]?.Value;
		string text2 = xmlElement.Attributes?["Caption"]?.Value ?? xmlElement.Attributes?["CaptionML"]?.Value;
		if (text == null && text2 != null)
		{
			XmlAttribute xmlAttribute2 = document.CreateAttribute("Name", addNode.Uri);
			xmlAttribute2.Value = text2;
			xmlElement.Attributes.Append(xmlAttribute2);
		}
		if (text != null && text2 == null)
		{
			XmlAttribute xmlAttribute3 = document.CreateAttribute("Caption", addNode.Uri);
			xmlAttribute3.Value = text;
			xmlElement.Attributes.Append(xmlAttribute3);
		}
		if (text != null)
		{
			string key = "Action" + text;
			if (distinctALIdentifier.TryGetValue(key, out var value))
			{
				xmlElement.Attributes["Name"].Value = text + "_" + value;
				alIdentifierMapper.Add(xmlElement.Attributes["ControlGUID"].Value, text + "_" + value);
				distinctALIdentifier[key] = value + 1;
			}
			else
			{
				distinctALIdentifier.Add(key, 1);
				alIdentifierMapper.Add(xmlElement.Attributes["ControlGUID"].Value, text);
			}
		}
		if (addNode.Id != null && !sourceNodes.ContainsKey(addNode.Id))
		{
			sourceNodes.Add(addNode.Id, xmlElement);
		}
		if (addNode.Id != null && !sourceDefNodes.ContainsKey(addNode.Id))
		{
			sourceDefNodes.Add(addNode.Id, xmlElement);
		}
		return xmlElement;
	}

	private static string GetActionAreaKind(string dest, IDictionary<string, XmlElement> sourceTempNodes)
	{
		string value = null;
		if (sourceTempNodes.TryGetValue(dest, out var value2))
		{
			ActionContainerType result;
			for (int i = 0; i < value2.ChildNodes.Count && (!Enum.TryParse<ActionContainerType>(value2.ChildNodes[i].Attributes?["ContainerType"]?.Value, out result) || !ObjectMapper.ActionAreaKindMapper.TryGetValue(result, out value)); i++)
			{
			}
		}
		return value;
	}

	public static IdentifierEqualsStringSyntax[] GetIdentifierEqualsStringSyntax(string value)
	{
		List<IdentifierEqualsStringSyntax> list = new List<IdentifierEqualsStringSyntax>();
		string[] array = value.Split(';');
		foreach (string text in array)
		{
			string[] array2 = text.Split('=');
			if (array2.Length == 2)
			{
				list.Add(SyntaxFactory.IdentifierEqualsString(array2[0], SyntaxFactory.StringLiteralValue(SyntaxFactory.Literal(array2[1]))));
			}
		}
		return list.ToArray();
	}
}
