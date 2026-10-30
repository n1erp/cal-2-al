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
[DataContract(Name = "CGD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ControlGroupDefinition : ControlGroupBaseDefinition
{
	private bool showCaptionField;

	[XmlAttribute]
	[DefaultValue(true)]
	[DataMember(Name = "CGD_1", EmitDefaultValue = false, IsRequired = false)]
	public bool ShowCaption
	{
		get
		{
			return showCaptionField;
		}
		set
		{
			showCaptionField = value;
		}
	}

	public ControlGroupDefinition()
	{
		showCaptionField = true;
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
		return (ControlGroupDefinition)base.Clone();
	}
}
