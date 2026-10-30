using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class MetaXmlPortFieldNode : MetaXmlPortNode
{
	private const string TableFieldDelimiter = "::";

	private string sourceFieldName;

	private string sourceTableName;

	private XmlPortFieldNodeFieldValidate fieldValidate = XmlPortFieldNodeFieldValidate.Undefined;

	private bool autoCalcField = true;

	private XmlPortNodeMinOccurs minOccurs = XmlPortNodeMinOccurs.Once;

	private XmlPortNodeMaxOccurs maxOccurs = XmlPortNodeMaxOccurs.Once;

	private XmlPortNodeOccurrence occurrence = XmlPortNodeOccurrence.Required;

	private NavType dataType;

	private bool unbound;

	public string SourceFieldName => sourceFieldName;

	public string SourceTableName => sourceTableName;

	public XmlPortFieldNodeFieldValidate FieldValidate => fieldValidate;

	public bool AutoCalcField => autoCalcField;

	public override XmlPortNodeMinOccurs MinOccurs => minOccurs;

	public override XmlPortNodeMaxOccurs MaxOccurs => maxOccurs;

	public override XmlPortNodeOccurrence Occurrence => occurrence;

	public NavType DataType => dataType;

	public override bool Unbound => unbound;

	public MetaXmlPortFieldNode(XmlNode textNode)
	{
		foreach (XmlNode item in textNode)
		{
			switch (item.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "SOURCEFIELD":
			{
				int num = item.InnerText.IndexOf("::", StringComparison.Ordinal);
				sourceFieldName = item.InnerText.Substring(num + "::".Length, item.InnerText.Length - num - "::".Length);
				sourceTableName = item.InnerText.Substring(0, num);
				break;
			}
			case "FIELDVALIDATE":
			{
				string text = item.InnerText.ToUpper(CultureInfo.InvariantCulture);
				if (string.Compare(text, "Undefined", StringComparison.OrdinalIgnoreCase) == 0)
				{
					fieldValidate = XmlPortFieldNodeFieldValidate.Undefined;
				}
				else
				{
					fieldValidate = (PropertiesSolveHelper.SolveBoolean(text) ? XmlPortFieldNodeFieldValidate.Yes : XmlPortFieldNodeFieldValidate.No);
				}
				break;
			}
			case "DATATYPE":
				dataType = (NavType)Enum.Parse(typeof(NavType), item.InnerText);
				break;
			case "AUTOCALCFIELD":
				autoCalcField = PropertiesSolveHelper.SolveBoolean(item.InnerText);
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
