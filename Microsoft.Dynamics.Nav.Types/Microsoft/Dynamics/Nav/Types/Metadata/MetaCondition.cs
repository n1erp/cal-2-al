using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MetaCondition : MetaBase
{
	private const string PropertyFieldId = "FieldID";

	private const string PropertyValue = "ConditionValue";

	private const string PropertyType = "ConditionType";

	private int fieldId;

	private FilterType conditionType;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string conditionValue;

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

	[DefaultValue(0)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public FilterType ConditionType
	{
		get
		{
			return conditionType;
		}
		set
		{
			conditionType = value;
		}
	}

	[DefaultValue("")]
	[XmlAttribute]
	public string ConditionValue
	{
		get
		{
			return conditionValue;
		}
		set
		{
			conditionValue = value;
		}
	}

	public MetaCondition()
	{
	}

	public MetaCondition(int fieldId, FilterType conditionType, string conditionValue)
	{
		this.fieldId = fieldId;
		this.conditionType = conditionType;
		this.conditionValue = conditionValue;
	}

	public MetaCondition(XmlNode root)
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
			case "ConditionValue":
				conditionValue = attribute.Value;
				break;
			case "ConditionType":
				conditionType = (FilterType)Enum.Parse(typeof(FilterType), attribute.Value);
				break;
			}
		}
	}
}
