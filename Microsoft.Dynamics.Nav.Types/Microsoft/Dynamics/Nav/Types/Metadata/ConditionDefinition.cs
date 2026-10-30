using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "CD0", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ConditionDefinition : ElementDefinition
{
	private int fieldIDField;

	private FilterType conditionTypeField;

	[DataMember(Name = "CD0_3", EmitDefaultValue = false, IsRequired = false)]
	private string conditionValueField;

	[XmlAttribute]
	[DataMember(Name = "CD0_1", EmitDefaultValue = false, IsRequired = false)]
	public int FieldID
	{
		get
		{
			return fieldIDField;
		}
		set
		{
			fieldIDField = value;
			FieldIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool FieldIDSpecified
	{
		get
		{
			return GetSpecified(0);
		}
		set
		{
			SetSpecified(0, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CD0_2", EmitDefaultValue = false, IsRequired = false)]
	public FilterType ConditionType
	{
		get
		{
			return conditionTypeField;
		}
		set
		{
			conditionTypeField = value;
			ConditionTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ConditionTypeSpecified
	{
		get
		{
			return GetSpecified(1);
		}
		set
		{
			SetSpecified(1, value);
		}
	}

	[XmlAttribute]
	public string ConditionValue
	{
		get
		{
			return conditionValueField;
		}
		set
		{
			conditionValueField = value;
		}
	}

	protected override int MaxSpecifiedBits => 2;

	public override ElementDefinition Find(Predicate<ElementDefinition> match)
	{
		if (match(this))
		{
			return this;
		}
		return base.Find(match);
	}

	public override object Clone()
	{
		return (ConditionDefinition)base.Clone();
	}
}
