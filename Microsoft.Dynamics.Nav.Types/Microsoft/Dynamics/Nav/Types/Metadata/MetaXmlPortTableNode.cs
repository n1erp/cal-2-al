using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class MetaXmlPortTableNode : MetaXmlPortNode
{
	private int sourceTable;

	private string variableName;

	private string sourceTableView;

	private int[] calcFields;

	private string linkTable;

	private bool linkTableForceInsert = true;

	private string linkFields;

	private bool temporary;

	private bool autoSave;

	private bool autoUpdate;

	private bool autoReplace;

	private XmlPortNodeMinOccurs minOccurs = XmlPortNodeMinOccurs.Once;

	private XmlPortNodeMaxOccurs maxOccurs = XmlPortNodeMaxOccurs.Unbounded;

	private XmlPortNodeOccurrence occurrence = XmlPortNodeOccurrence.Required;

	public int SourceTable => sourceTable;

	public string VariableName => variableName;

	public string SourceTableView => sourceTableView;

	public string LinkTable => linkTable;

	public bool LinkTableForceInsert => linkTableForceInsert;

	public string LinkFields => linkFields;

	public bool Temporary => temporary;

	public bool AutoSave => autoSave;

	public bool AutoUpdate => autoUpdate;

	public bool AutoReplace => autoReplace;

	public override XmlPortNodeMinOccurs MinOccurs => minOccurs;

	public override XmlPortNodeMaxOccurs MaxOccurs => maxOccurs;

	public override XmlPortNodeOccurrence Occurrence => occurrence;

	public override bool Unbound => false;

	public MetaXmlPortTableNode(XmlNode textNode)
	{
		autoSave = true;
		foreach (XmlNode item in textNode)
		{
			switch (item.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "SOURCETABLE":
				sourceTable = int.Parse(item.InnerText, CultureInfo.InvariantCulture);
				break;
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
			case "SOURCETABLEVIEW":
				sourceTableView = item.InnerText;
				break;
			case "CALCFIELDS":
				if (!string.IsNullOrEmpty(item.InnerText))
				{
					string[] array = item.InnerText.Split(',');
					calcFields = new int[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						int num = int.Parse(array[i].Substring(5, array[i].Length - 5), CultureInfo.InvariantCulture);
						calcFields[i] = num;
					}
				}
				break;
			case "LINKTABLE":
				linkTable = item.InnerText;
				break;
			case "LINKTABLEFORCEINSERT":
				linkTableForceInsert = PropertiesSolveHelper.SolveBoolean(item.InnerText);
				break;
			case "LINKFIELDS":
				linkFields = item.InnerText;
				break;
			case "TEMPORARY":
				temporary = PropertiesSolveHelper.SolveBoolean(item.InnerText);
				break;
			case "AUTOSAVE":
				autoSave = PropertiesSolveHelper.SolveBoolean(item.InnerText);
				break;
			case "AUTOUPDATE":
				autoUpdate = PropertiesSolveHelper.SolveBoolean(item.InnerText);
				break;
			case "AUTOREPLACE":
				autoReplace = PropertiesSolveHelper.SolveBoolean(item.InnerText);
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

	public int[] CalcFields()
	{
		return calcFields;
	}
}
