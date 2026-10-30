using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class XmlNodeMapper
{
	public static object[] UnwrapXmlNodes(object[] content)
	{
		if (content == null)
		{
			return content;
		}
		for (int i = 0; i < content.Length; i++)
		{
			content[i] = UnwrapALValue(content[i]);
		}
		return content;
	}

	public static NavXmlNodeList CreateXmlNodeList(IEnumerable<XObject> values)
	{
		List<NavXmlNode> list = new List<NavXmlNode>();
		foreach (XObject value in values)
		{
			list.Add(CreateNavXmlNode(value));
		}
		return new NavXmlNodeList(list);
	}

	public static NavXmlNode SelectFirstOrDefaultXmlNode(object xpathResults)
	{
		if (xpathResults is object[] possibleResults)
		{
			return SelectFirstOrDefaultXmlNode(possibleResults);
		}
		if (xpathResults is IEnumerable<object> possibleResults2)
		{
			return SelectFirstOrDefaultXmlNode(possibleResults2);
		}
		return GetXmlNodeOrDefault(xpathResults);
	}

	public static NavXmlNodeList SelectXmlNodes(object value)
	{
		List<NavXmlNode> elements = SelectXmlNodesFromObject(value);
		return new NavXmlNodeList(elements);
	}

	private static List<NavXmlNode> SelectXmlNodesFromObject(object value)
	{
		List<NavXmlNode> list = new List<NavXmlNode>();
		if (value is object[] array)
		{
			object[] array2 = array;
			foreach (object value2 in array2)
			{
				list.AddRange(SelectXmlNodesFromObject(value2));
			}
			return list;
		}
		if (value is IEnumerable<object> enumerable)
		{
			{
				foreach (object item in enumerable)
				{
					list.AddRange(SelectXmlNodesFromObject(item));
				}
				return list;
			}
		}
		NavXmlNode xmlNodeOrDefault = GetXmlNodeOrDefault(value);
		if (xmlNodeOrDefault != null)
		{
			list.Add(xmlNodeOrDefault);
		}
		return list;
	}

	private static object UnwrapALValue(object content)
	{
		if (content == null)
		{
			return content;
		}
		if (content is IArrayAccess)
		{
			throw new NavNCLArgumentException();
		}
		if (content is IXmlValueContainer<object> xmlValueContainer)
		{
			if (xmlValueContainer.HasValue)
			{
				return xmlValueContainer.Value;
			}
			throw new NavNCLNotInitializedException(content.GetType());
		}
		if (content is NavValue value)
		{
			return NavFormatEvaluateHelper.Format(value, 0, 9);
		}
		return content.ToString();
	}

	private static NavXmlNode GetXmlNodeOrDefault(object value)
	{
		if (value is XObject value2)
		{
			return CreateNavXmlNode(value2);
		}
		if (value is XDeclaration declaration)
		{
			return CreateNavXmlNode(declaration);
		}
		return null;
	}

	private static NavXmlNode CreateNavXmlNode(XObject value)
	{
		return value.NodeType switch
		{
			XmlNodeType.Element => (NavXmlNode)new NavXmlElement((XElement)value), 
			XmlNodeType.Attribute => new NavXmlAttribute((XAttribute)value), 
			XmlNodeType.DocumentType => new NavXmlDocumentType((XDocumentType)value), 
			XmlNodeType.Document => new NavXmlDocument((XDocument)value), 
			XmlNodeType.Text => new NavXmlText((XText)value), 
			XmlNodeType.CDATA => new NavXmlCData((XCData)value), 
			XmlNodeType.ProcessingInstruction => new NavXmlProcessingInstruction((XProcessingInstruction)value), 
			XmlNodeType.Comment => new NavXmlComment((XComment)value), 
			_ => null, 
		};
	}

	private static NavXmlNode CreateNavXmlNode(XDeclaration declaration)
	{
		return new NavXmlDeclaration(declaration);
	}

	private static NavXmlNode SelectFirstOrDefaultXmlNode(IEnumerable<object> possibleResults)
	{
		if (possibleResults != null)
		{
			foreach (object possibleResult in possibleResults)
			{
				NavXmlNode navXmlNode = SelectFirstOrDefaultXmlNode(possibleResult);
				if (navXmlNode != null)
				{
					return navXmlNode;
				}
			}
		}
		return null;
	}
}
