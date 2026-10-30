using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MetaFilter : MetaBase, IFilterDefinition
{
	private const string PropertyFieldId = "FieldID";

	private const string PropertyValue = "FilterValue";

	private const string PropertyType = "FilterType";

	private const string PropertyValueIsFilter = "ValueIsFilter";

	private const string PropertyFilterGroup = "FilterGroup";

	private const string PropertyOnlyMaxLimit = "OnlyMaxLimit";

	private int fieldId;

	private FilterType filterType;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string filterValue;

	private int filterGroup;

	private bool valueIsFilter;

	private bool onlyMaxLimit;

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

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public FilterType FilterType
	{
		get
		{
			return filterType;
		}
		set
		{
			filterType = value;
		}
	}

	[DefaultValue("")]
	[XmlAttribute]
	public string FilterValue
	{
		get
		{
			return filterValue;
		}
		set
		{
			filterValue = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int FilterGroup
	{
		get
		{
			return filterGroup;
		}
		set
		{
			filterGroup = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool ValueIsFilter
	{
		get
		{
			return valueIsFilter;
		}
		set
		{
			valueIsFilter = value;
		}
	}

	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public bool OnlyMaxLimit
	{
		get
		{
			return onlyMaxLimit;
		}
		set
		{
			onlyMaxLimit = value;
		}
	}

	public MetaFilter(XmlNode root)
		: base(root)
	{
		Initialize(root);
	}

	private void Initialize(XmlNode root)
	{
		foreach (XmlAttribute attribute in root.Attributes)
		{
			switch (attribute.Name)
			{
			case "FieldID":
				fieldId = MetaBase.Int32Value(attribute.Value);
				break;
			case "FilterValue":
				filterValue = (string.IsNullOrEmpty(attribute.Value) ? "''" : attribute.Value);
				break;
			case "FilterType":
				filterType = (FilterType)Enum.Parse(typeof(FilterType), attribute.Value);
				break;
			case "FilterGroup":
				filterGroup = MetaBase.Int32Value(attribute.Value);
				break;
			case "ValueIsFilter":
				valueIsFilter = MetaBase.BooleanValue(attribute.Value);
				break;
			case "OnlyMaxLimit":
				onlyMaxLimit = MetaBase.BooleanValue(attribute.Value);
				break;
			}
		}
	}

	public MetaFilter()
	{
	}

	public MetaFilter(int fieldId, FilterType filterType, string filterValue)
	{
		this.fieldId = fieldId;
		this.filterType = filterType;
		this.filterValue = filterValue;
	}
}
