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
[DataContract(Name = "FD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public sealed class FilterDefinition : ElementDefinition, IFilterDefinition
{
	private int filterGroupField;

	private int fieldIDField;

	private FilterType filterTypeField;

	[DataMember(Name = "FD_4", EmitDefaultValue = false, IsRequired = false)]
	private string filterValueField;

	private bool valueIsFilterField;

	private bool onlyMaxLimitField;

	[XmlAttribute]
	[DataMember(Name = "FD_1", EmitDefaultValue = false, IsRequired = false)]
	public int FilterGroup
	{
		get
		{
			return filterGroupField;
		}
		set
		{
			filterGroupField = value;
			FilterGroupSpecified = true;
		}
	}

	[XmlIgnore]
	public bool FilterGroupSpecified
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
	[DataMember(Name = "FD_2", EmitDefaultValue = false, IsRequired = false)]
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
			return GetSpecified(1);
		}
		set
		{
			SetSpecified(1, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "FD_3", EmitDefaultValue = false, IsRequired = false)]
	public FilterType FilterType
	{
		get
		{
			return filterTypeField;
		}
		set
		{
			filterTypeField = value;
			FilterTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool FilterTypeSpecified
	{
		get
		{
			return GetSpecified(2);
		}
		set
		{
			SetSpecified(2, value);
		}
	}

	[XmlAttribute]
	public string FilterValue
	{
		get
		{
			return filterValueField;
		}
		set
		{
			filterValueField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "FD_5", EmitDefaultValue = false, IsRequired = false)]
	public bool ValueIsFilter
	{
		get
		{
			return valueIsFilterField;
		}
		set
		{
			valueIsFilterField = value;
			ValueIsFilterSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ValueIsFilterSpecified
	{
		get
		{
			return GetSpecified(3);
		}
		set
		{
			SetSpecified(3, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "FD_6", EmitDefaultValue = false, IsRequired = false)]
	public bool OnlyMaxLimit
	{
		get
		{
			return onlyMaxLimitField;
		}
		set
		{
			onlyMaxLimitField = value;
			OnlyMaxLimitSpecified = true;
		}
	}

	[XmlIgnore]
	public bool OnlyMaxLimitSpecified
	{
		get
		{
			return GetSpecified(4);
		}
		set
		{
			SetSpecified(4, value);
		}
	}

	protected override int MaxSpecifiedBits => 5;

	int IFilterDefinition.FieldId => FieldID;

	FilterType IFilterDefinition.FilterType => FilterType;

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
		return (FilterDefinition)base.Clone();
	}
}
