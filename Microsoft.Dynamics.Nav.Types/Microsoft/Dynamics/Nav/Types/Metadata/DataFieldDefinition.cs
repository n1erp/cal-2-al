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
[DataContract(Name = "DFD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[XmlRoot(ElementName = "Expression", Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects", IsNullable = false)]
public class DataFieldDefinition : ElementDefinition
{
	[DataMember(Name = "DFD_1", EmitDefaultValue = false, IsRequired = false)]
	private string nameField;

	private DataType datatypeField;

	private int dataLengthField;

	[DataMember(Name = "DFD_4", EmitDefaultValue = false, IsRequired = false)]
	private string optionStringField;

	private int enumIdField;

	private bool isDataSetVarField;

	private bool expressionIsAssignableField;

	[DataMember(Name = "DFD_8", EmitDefaultValue = false, IsRequired = false)]
	private string sourceExpressionField;

	private bool inRepeaterGroupField;

	private int tableNoField;

	private int fieldNoField;

	private SourceExpressionType expressionTypeField;

	[DataMember(Name = "DFD_13", EmitDefaultValue = false, IsRequired = false)]
	private string subTypeField;

	[DataMember(Name = "DFD_14", EmitDefaultValue = false, IsRequired = false)]
	private string optionCaptionField;

	[DataMember(Name = "DFD_15", EmitDefaultValue = false, IsRequired = false)]
	private string optionValuesField;

	[XmlAttribute]
	public string Name
	{
		get
		{
			return nameField;
		}
		set
		{
			nameField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "DFD_2", EmitDefaultValue = false, IsRequired = false)]
	public DataType Datatype
	{
		get
		{
			return datatypeField;
		}
		set
		{
			datatypeField = value;
			DatatypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool DatatypeSpecified
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
	[DataMember(Name = "DFD_3", EmitDefaultValue = false, IsRequired = false)]
	public int DataLength
	{
		get
		{
			return dataLengthField;
		}
		set
		{
			dataLengthField = value;
			DataLengthSpecified = true;
		}
	}

	[XmlIgnore]
	public bool DataLengthSpecified
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
	public string OptionString
	{
		get
		{
			return optionStringField;
		}
		set
		{
			optionStringField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "DFD_5", EmitDefaultValue = false, IsRequired = false)]
	public int EnumId
	{
		get
		{
			return enumIdField;
		}
		set
		{
			enumIdField = value;
			EnumIdSpecified = true;
		}
	}

	[XmlIgnore]
	public bool EnumIdSpecified
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
	[DataMember(Name = "DFD_6", EmitDefaultValue = false, IsRequired = false)]
	public bool IsDataSetVar
	{
		get
		{
			return isDataSetVarField;
		}
		set
		{
			isDataSetVarField = value;
			IsDataSetVarSpecified = true;
		}
	}

	[XmlIgnore]
	public bool IsDataSetVarSpecified
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
	[DataMember(Name = "DFD_7", EmitDefaultValue = false, IsRequired = false)]
	public bool ExpressionIsAssignable
	{
		get
		{
			return expressionIsAssignableField;
		}
		set
		{
			expressionIsAssignableField = value;
			ExpressionIsAssignableSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ExpressionIsAssignableSpecified
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

	[XmlAttribute]
	public string SourceExpression
	{
		get
		{
			return sourceExpressionField;
		}
		set
		{
			sourceExpressionField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "DFD_9", EmitDefaultValue = false, IsRequired = false)]
	public bool InRepeaterGroup
	{
		get
		{
			return inRepeaterGroupField;
		}
		set
		{
			inRepeaterGroupField = value;
			InRepeaterGroupSpecified = true;
		}
	}

	[XmlIgnore]
	public bool InRepeaterGroupSpecified
	{
		get
		{
			return GetSpecified(5);
		}
		set
		{
			SetSpecified(5, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "DFD_10", EmitDefaultValue = false, IsRequired = false)]
	public int TableNo
	{
		get
		{
			return tableNoField;
		}
		set
		{
			tableNoField = value;
			TableNoSpecified = true;
		}
	}

	[XmlIgnore]
	public bool TableNoSpecified
	{
		get
		{
			return GetSpecified(6);
		}
		set
		{
			SetSpecified(6, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "DFD_11", EmitDefaultValue = false, IsRequired = false)]
	public int FieldNo
	{
		get
		{
			return fieldNoField;
		}
		set
		{
			fieldNoField = value;
			FieldNoSpecified = true;
		}
	}

	[XmlIgnore]
	public bool FieldNoSpecified
	{
		get
		{
			return GetSpecified(7);
		}
		set
		{
			SetSpecified(7, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "DFD_12", EmitDefaultValue = false, IsRequired = false)]
	public SourceExpressionType ExpressionType
	{
		get
		{
			return expressionTypeField;
		}
		set
		{
			expressionTypeField = value;
			ExpressionTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ExpressionTypeSpecified
	{
		get
		{
			return GetSpecified(8);
		}
		set
		{
			SetSpecified(8, value);
		}
	}

	[XmlAttribute]
	public string SubType
	{
		get
		{
			return subTypeField;
		}
		set
		{
			subTypeField = value;
		}
	}

	[XmlAttribute]
	public string OptionCaption
	{
		get
		{
			return optionCaptionField;
		}
		set
		{
			optionCaptionField = value;
		}
	}

	[XmlAttribute]
	public string OptionValues
	{
		get
		{
			return optionValuesField;
		}
		set
		{
			optionValuesField = value;
		}
	}

	protected override int MaxSpecifiedBits => 9;

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
		return (DataFieldDefinition)base.Clone();
	}
}
