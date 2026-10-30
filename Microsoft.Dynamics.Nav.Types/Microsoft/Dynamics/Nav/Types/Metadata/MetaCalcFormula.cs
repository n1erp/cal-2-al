using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MetaCalcFormula : MetaBase
{
	private const string PropertyTable = "Table";

	private const string PropertyField = "Field";

	private const string PropertyTableFilters = "TableFilters";

	private const string PropertyFilter = "Filter";

	private const string PropertyFlowFieldType = "FlowType";

	private const string PropertyReverseSign = "ReverseSign";

	private int tableId;

	private int fieldId;

	private string flowFieldType;

	private bool reverseSign;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<MetaFilter> filters = new List<MetaFilter>();

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int TableId
	{
		get
		{
			return tableId;
		}
		set
		{
			tableId = value;
		}
	}

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int FieldId
	{
		get
		{
			return fieldId;
		}
		set
		{
			fieldId = value;
		}
	}

	public List<MetaFilter> Filters
	{
		get
		{
			return filters;
		}
		set
		{
			filters = value;
		}
	}

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public string FlowFieldType
	{
		get
		{
			return flowFieldType;
		}
		set
		{
			flowFieldType = value;
		}
	}

	public bool ReverseSign
	{
		get
		{
			return reverseSign;
		}
		set
		{
			reverseSign = value;
		}
	}

	public MetaCalcFormula()
	{
	}

	public MetaCalcFormula(XmlNode root)
		: base(root)
	{
		Initialize(root);
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(root.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("a", root.NamespaceURI);
		XmlNode xmlNode = root.SelectSingleNode("a:TableFilters", xmlNamespaceManager);
		if (xmlNode == null)
		{
			return;
		}
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			if (childNode.Name == "Filter")
			{
				filters.Add(new MetaFilter(childNode));
			}
		}
	}

	public override object Clone()
	{
		MetaCalcFormula metaCalcFormula = (MetaCalcFormula)base.Clone();
		if (filters != null)
		{
			metaCalcFormula.filters = new List<MetaFilter>();
			foreach (MetaFilter filter in Filters)
			{
				metaCalcFormula.Filters.Add((MetaFilter)filter.Clone());
			}
		}
		return metaCalcFormula;
	}

	private void Initialize(XmlNode root)
	{
		foreach (XmlAttribute attribute in root.Attributes)
		{
			switch (attribute.Name)
			{
			case "Table":
				tableId = MetaBase.Int32Value(attribute.Value);
				break;
			case "Field":
				fieldId = MetaBase.Int32Value(attribute.Value);
				break;
			case "FlowType":
				flowFieldType = attribute.Value;
				break;
			case "ReverseSign":
				reverseSign = MetaBase.BooleanValue(attribute.Value);
				break;
			}
		}
	}
}
