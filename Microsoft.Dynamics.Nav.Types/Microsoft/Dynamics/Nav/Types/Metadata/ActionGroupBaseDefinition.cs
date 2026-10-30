using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlInclude(typeof(NavigationGroupDefinition))]
[XmlInclude(typeof(ActionGroupDefinition))]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[KnownType(typeof(NavigationGroupDefinition))]
[KnownType(typeof(ActionGroupDefinition))]
[DataContract(Name = "AGBD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class ActionGroupBaseDefinition : ActionBaseDefinition
{
	private bool deleteIfEmptyField;

	[XmlAttribute]
	[DefaultValue(true)]
	[DataMember(Name = "AGBD_1", EmitDefaultValue = false, IsRequired = false)]
	public bool DeleteIfEmpty
	{
		get
		{
			return deleteIfEmptyField;
		}
		set
		{
			deleteIfEmptyField = value;
		}
	}

	protected internal ActionGroupBaseDefinition()
	{
		deleteIfEmptyField = true;
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
		return (ActionGroupBaseDefinition)base.Clone();
	}
}
