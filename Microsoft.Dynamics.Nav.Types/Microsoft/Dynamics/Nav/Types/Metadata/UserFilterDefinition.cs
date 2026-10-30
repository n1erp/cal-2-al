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
[DataContract(Name = "UFD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class UserFilterDefinition : ElementDefinition
{
	[DataMember(Name = "UFD_1", EmitDefaultValue = false, IsRequired = false)]
	private string fieldIdField;

	[DataMember(Name = "UFD_2", EmitDefaultValue = false, IsRequired = false)]
	private string filterValueField;

	[XmlAttribute]
	public string FieldId
	{
		get
		{
			return fieldIdField;
		}
		set
		{
			fieldIdField = value;
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
		return (UserFilterDefinition)base.Clone();
	}
}
