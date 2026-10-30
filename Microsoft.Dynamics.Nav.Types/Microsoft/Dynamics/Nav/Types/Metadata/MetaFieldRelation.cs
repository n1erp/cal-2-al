using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MetaFieldRelation : MetaBase
{
	private const string PropertyTableID = "TableID";

	private const string PropertyTableName = "TableName";

	private const string PropertyFieldNo = "FieldID";

	private const string XpathGetConditions = "{0}Conditions";

	private const string XpathGetFilters = "{0}Filters";

	private int tableId;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string tableName;

	private int fieldId;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<MetaFilter> filters = new List<MetaFilter>();

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private List<MetaCondition> conditions = new List<MetaCondition>();

	[DefaultValue(0)]
	[XmlAttribute]
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

	[DefaultValue("")]
	[XmlAttribute]
	public string TableName
	{
		get
		{
			return tableName;
		}
		set
		{
			tableName = value;
		}
	}

	[DefaultValue(0)]
	[XmlAttribute]
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

	public List<MetaCondition> Conditions
	{
		get
		{
			return conditions;
		}
		set
		{
			conditions = value;
		}
	}

	public MetaFieldRelation()
	{
	}

	public override object Clone()
	{
		MetaFieldRelation metaFieldRelation = (MetaFieldRelation)base.Clone();
		if (conditions != null)
		{
			metaFieldRelation.conditions = new List<MetaCondition>();
			foreach (MetaCondition condition in Conditions)
			{
				metaFieldRelation.Conditions.Add((MetaCondition)condition.Clone());
			}
		}
		if (filters != null)
		{
			metaFieldRelation.filters = new List<MetaFilter>();
			foreach (MetaFilter filter in Filters)
			{
				metaFieldRelation.Filters.Add((MetaFilter)filter.Clone());
			}
		}
		return metaFieldRelation;
	}

	public MetaFieldRelation(int tableId, string tableName, int fieldId)
	{
		this.tableId = tableId;
		this.tableName = tableName;
		this.fieldId = fieldId;
	}

	public MetaFieldRelation(XmlNode root)
		: base(root)
	{
		Initialize(root);
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(root.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("a", root.NamespaceURI);
		XmlNodeList xmlNodeList = root.SelectNodes(string.Format(CultureInfo.InvariantCulture, "{0}Filters", "a:"), xmlNamespaceManager);
		if (xmlNodeList != null)
		{
			foreach (XmlNode item in xmlNodeList)
			{
				filters.Add(new MetaFilter(item));
			}
		}
		XmlNodeList xmlNodeList2 = root.SelectNodes(string.Format(CultureInfo.InvariantCulture, "{0}Conditions", "a:"), xmlNamespaceManager);
		if (xmlNodeList2 == null)
		{
			return;
		}
		foreach (XmlNode item2 in xmlNodeList2)
		{
			conditions.Add(new MetaCondition(item2));
		}
	}

	private void Initialize(XmlNode root)
	{
		foreach (XmlAttribute attribute in root.Attributes)
		{
			switch (attribute.Name)
			{
			case "TableID":
				tableId = MetaBase.Int32Value(attribute.Value);
				break;
			case "TableName":
				tableName = attribute.Value;
				break;
			case "FieldID":
				fieldId = MetaBase.Int32Value(attribute.Value);
				break;
			}
		}
	}
}
