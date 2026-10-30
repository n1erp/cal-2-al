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
[DataContract(Name = "ISD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class InfopartSystemDefinition : InfopartBaseDefinition
{
	private SystemPartTypes systemPartIDField;

	private UpdatePropagation updatePropagationField;

	[DataMember(Name = "ISD_3", EmitDefaultValue = false, IsRequired = false)]
	private string applicationAreaField;

	[XmlAttribute]
	[DataMember(Name = "ISD_1", EmitDefaultValue = false, IsRequired = false)]
	public SystemPartTypes SystemPartID
	{
		get
		{
			return systemPartIDField;
		}
		set
		{
			systemPartIDField = value;
			SystemPartIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool SystemPartIDSpecified
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
	[DataMember(Name = "ISD_2", EmitDefaultValue = false, IsRequired = false)]
	public UpdatePropagation UpdatePropagation
	{
		get
		{
			return updatePropagationField;
		}
		set
		{
			updatePropagationField = value;
			UpdatePropagationSpecified = true;
		}
	}

	[XmlIgnore]
	public bool UpdatePropagationSpecified
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
	public string ApplicationArea
	{
		get
		{
			return applicationAreaField;
		}
		set
		{
			applicationAreaField = value;
		}
	}

	protected override int MaxSpecifiedBits => 11;

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
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
		return (InfopartSystemDefinition)base.Clone();
	}
}
