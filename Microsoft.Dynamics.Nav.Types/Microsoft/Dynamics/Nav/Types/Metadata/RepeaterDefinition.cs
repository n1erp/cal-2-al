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
[DataContract(Name = "RD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RepeaterDefinition : ControlGroupBaseDefinition
{
	private bool showAsTreeField;

	private int freezeColumnIDField;

	[DataMember(Name = "RD_3", EmitDefaultValue = false, IsRequired = false)]
	private string indentationColumnNameField;

	[DataMember(Name = "RD_4", EmitDefaultValue = false, IsRequired = false)]
	private string indentationControlsField;

	[XmlAttribute]
	[DataMember(Name = "RD_1", EmitDefaultValue = false, IsRequired = false)]
	public bool ShowAsTree
	{
		get
		{
			return showAsTreeField;
		}
		set
		{
			showAsTreeField = value;
			ShowAsTreeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ShowAsTreeSpecified
	{
		get
		{
			return GetSpecified(9);
		}
		set
		{
			SetSpecified(9, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "RD_2", EmitDefaultValue = false, IsRequired = false)]
	public int FreezeColumnID
	{
		get
		{
			return freezeColumnIDField;
		}
		set
		{
			freezeColumnIDField = value;
			FreezeColumnIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool FreezeColumnIDSpecified
	{
		get
		{
			return GetSpecified(10);
		}
		set
		{
			SetSpecified(10, value);
		}
	}

	[XmlAttribute]
	public string IndentationColumnName
	{
		get
		{
			return indentationColumnNameField;
		}
		set
		{
			indentationColumnNameField = value;
		}
	}

	[XmlAttribute]
	public string IndentationControls
	{
		get
		{
			return indentationControlsField;
		}
		set
		{
			indentationControlsField = value;
		}
	}

	protected override int MaxSpecifiedBits => 11;

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
		return (RepeaterDefinition)base.Clone();
	}
}
