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
[DataContract(Name = "CTD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ControlTriggerDefinition : ElementDefinition
{
	private ControlTriggerType triggerTypeField;

	[DataMember(Name = "CTD_2", EmitDefaultValue = false, IsRequired = false)]
	private string nameField;

	[XmlAttribute]
	[DataMember(Name = "CTD_1", EmitDefaultValue = false, IsRequired = false)]
	public ControlTriggerType TriggerType
	{
		get
		{
			return triggerTypeField;
		}
		set
		{
			triggerTypeField = value;
			TriggerTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool TriggerTypeSpecified
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

	protected override int MaxSpecifiedBits => 1;

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
		return (ControlTriggerDefinition)base.Clone();
	}
}
