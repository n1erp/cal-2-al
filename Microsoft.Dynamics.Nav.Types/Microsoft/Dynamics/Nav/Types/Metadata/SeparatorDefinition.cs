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
[DataContract(Name = "SD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SeparatorDefinition : ActionBaseDefinition
{
	private bool isHeaderField;

	private bool isHeadingDynamicField;

	[XmlAttribute]
	[DataMember(Name = "SD_1", EmitDefaultValue = false, IsRequired = false)]
	public bool IsHeader
	{
		get
		{
			return isHeaderField;
		}
		set
		{
			isHeaderField = value;
			IsHeaderSpecified = true;
		}
	}

	[XmlIgnore]
	public bool IsHeaderSpecified
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
	[DataMember(Name = "SD_2", EmitDefaultValue = false, IsRequired = false)]
	public bool IsHeadingDynamic
	{
		get
		{
			return isHeadingDynamicField;
		}
		set
		{
			isHeadingDynamicField = value;
			IsHeadingDynamicSpecified = true;
		}
	}

	[XmlIgnore]
	public bool IsHeadingDynamicSpecified
	{
		get
		{
			return GetSpecified(11);
		}
		set
		{
			SetSpecified(11, value);
		}
	}

	protected override int MaxSpecifiedBits => 12;

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
		return (SeparatorDefinition)base.Clone();
	}
}
