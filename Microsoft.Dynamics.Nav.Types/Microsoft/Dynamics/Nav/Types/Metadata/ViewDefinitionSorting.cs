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
[DataContract(Name = "VDS", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ViewDefinitionSorting : ElementDefinition
{
	[DataMember(Name = "VDS_1", EmitDefaultValue = false, IsRequired = false)]
	private string keyFieldsField;

	private bool ascendingField;

	private bool ascendingSetByViewField;

	private bool keyFieldsSetByViewField;

	[XmlAttribute]
	[DefaultValue("")]
	public string KeyFields
	{
		get
		{
			if (keyFieldsField == null)
			{
				return "";
			}
			return keyFieldsField;
		}
		set
		{
			keyFieldsField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(true)]
	[DataMember(Name = "VDS_2", EmitDefaultValue = false, IsRequired = false)]
	public bool Ascending
	{
		get
		{
			return ascendingField;
		}
		set
		{
			ascendingField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "VDS_3", EmitDefaultValue = false, IsRequired = false)]
	public bool AscendingSetByView
	{
		get
		{
			return ascendingSetByViewField;
		}
		set
		{
			ascendingSetByViewField = value;
			AscendingSetByViewSpecified = true;
		}
	}

	[XmlIgnore]
	public bool AscendingSetByViewSpecified
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
	[DataMember(Name = "VDS_4", EmitDefaultValue = false, IsRequired = false)]
	public bool KeyFieldsSetByView
	{
		get
		{
			return keyFieldsSetByViewField;
		}
		set
		{
			keyFieldsSetByViewField = value;
			KeyFieldsSetByViewSpecified = true;
		}
	}

	[XmlIgnore]
	public bool KeyFieldsSetByViewSpecified
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

	protected override int MaxSpecifiedBits => 2;

	public ViewDefinitionSorting()
	{
		ascendingField = true;
	}

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
		return (ViewDefinitionSorting)base.Clone();
	}
}
