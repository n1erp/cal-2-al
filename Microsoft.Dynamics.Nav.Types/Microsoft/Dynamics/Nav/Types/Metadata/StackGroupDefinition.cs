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
[DataContract(Name = "SGD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class StackGroupDefinition : ControlGroupBaseDefinition
{
	private LayoutType layoutField;

	[XmlAttribute]
	[DataMember(Name = "SGD_1", EmitDefaultValue = false, IsRequired = false)]
	public LayoutType Layout
	{
		get
		{
			return layoutField;
		}
		set
		{
			layoutField = value;
			LayoutSpecified = true;
		}
	}

	[XmlIgnore]
	public bool LayoutSpecified
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

	protected override int MaxSpecifiedBits => 10;

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
		return (StackGroupDefinition)base.Clone();
	}
}
