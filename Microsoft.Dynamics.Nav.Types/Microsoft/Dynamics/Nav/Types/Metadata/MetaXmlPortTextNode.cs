using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class MetaXmlPortTextNode : MetaXmlPortNode
{
	private XmlPortNodeTextType textType = XmlPortNodeTextType.Text;

	private string variableName;

	private XmlPortNodeMinOccurs minOccurs = XmlPortNodeMinOccurs.Once;

	private XmlPortNodeMaxOccurs maxOccurs = XmlPortNodeMaxOccurs.Unbounded;

	private XmlPortNodeOccurrence occurrence = XmlPortNodeOccurrence.Required;

	private bool unbound;

	public XmlPortNodeTextType TextType => textType;

	public string VariableName => variableName;

	public override XmlPortNodeMinOccurs MinOccurs => minOccurs;

	public override XmlPortNodeMaxOccurs MaxOccurs => maxOccurs;

	public override XmlPortNodeOccurrence Occurrence => occurrence;

	public override bool Unbound => unbound;

	public MetaXmlPortTextNode()
	{
	}

	public MetaXmlPortTextNode(XmlNode textNode)
	{
		foreach (XmlNode item in textNode)
		{
			switch (item.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "VARIABLENAME":
				if (string.IsNullOrEmpty(item.InnerText))
				{
					variableName = NodeName;
				}
				else
				{
					variableName = item.InnerText;
				}
				break;
			case "TEXTTYPE":
				if (XmlPortNodeTextType.BigText.ToString() == item.InnerText)
				{
					textType = XmlPortNodeTextType.BigText;
				}
				else
				{
					textType = XmlPortNodeTextType.Text;
				}
				break;
			case "ID":
				Id = new Guid(item.InnerText);
				break;
			case "PARENTID":
				ParentId = new Guid(item.InnerText);
				break;
			case "INDENTATION":
				Indentation = int.Parse(item.InnerText, CultureInfo.InvariantCulture);
				break;
			case "WIDTH":
				Width = int.Parse(item.InnerText, CultureInfo.InvariantCulture);
				break;
			case "NODENAME":
				NodeName = item.InnerText;
				break;
			case "NAMESPACEPREFIX":
				NodePrefix = item.InnerText;
				break;
			case "UNBOUND":
				unbound = PropertiesSolveHelper.SolveBoolean(item.InnerText);
				break;
			case "NODETYPE":
				if (XmlPortNodeType.Attribute.ToString() == item.InnerText)
				{
					NodeType = XmlPortNodeType.Attribute;
				}
				else
				{
					NodeType = XmlPortNodeType.Element;
				}
				break;
			case "MINOCCURS":
				if (XmlPortNodeMinOccurs.Once.ToString() == item.InnerText)
				{
					minOccurs = XmlPortNodeMinOccurs.Once;
				}
				else
				{
					minOccurs = XmlPortNodeMinOccurs.Zero;
				}
				break;
			case "MAXOCCURS":
				if (XmlPortNodeMaxOccurs.Once.ToString() == item.InnerText)
				{
					maxOccurs = XmlPortNodeMaxOccurs.Once;
				}
				else
				{
					maxOccurs = XmlPortNodeMaxOccurs.Unbounded;
				}
				break;
			case "OCCURRENCE":
				if (XmlPortNodeOccurrence.Required.ToString() == item.InnerText)
				{
					occurrence = XmlPortNodeOccurrence.Required;
				}
				else
				{
					occurrence = XmlPortNodeOccurrence.Optional;
				}
				break;
			}
		}
	}
}
