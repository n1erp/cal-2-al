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
[DataContract(Name = "CAIRD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ControlAddInResourceDefinition : ElementDefinition
{
	private byte[] resourceValueField;

	[DataMember(Name = "CAIRD_2", EmitDefaultValue = false, IsRequired = false)]
	private string resourceNameField;

	private ControlAddInResourceType resourceTypeField;

	[XmlElement(DataType = "base64Binary")]
	[DataMember(Name = "CAIRD_1", EmitDefaultValue = false, IsRequired = false)]
	public byte[] ResourceValue
	{
		get
		{
			return resourceValueField;
		}
		set
		{
			resourceValueField = value;
		}
	}

	[XmlAttribute]
	public string ResourceName
	{
		get
		{
			return resourceNameField;
		}
		set
		{
			resourceNameField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CAIRD_3", EmitDefaultValue = false, IsRequired = false)]
	public ControlAddInResourceType ResourceType
	{
		get
		{
			return resourceTypeField;
		}
		set
		{
			resourceTypeField = value;
			ResourceTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ResourceTypeSpecified
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
		return (ControlAddInResourceDefinition)base.Clone();
	}
}
