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
[DataContract(Name = "ICD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class InfopartChartDefinition : InfopartBaseDefinition
{
	[DataMember(Name = "ICD_1", EmitDefaultValue = false, IsRequired = false)]
	private string chartPartIDField;

	private UpdatePropagation updatePropagationField;

	[XmlAttribute]
	public string ChartPartID
	{
		get
		{
			return chartPartIDField;
		}
		set
		{
			chartPartIDField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "ICD_2", EmitDefaultValue = false, IsRequired = false)]
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
			return GetSpecified(9);
		}
		set
		{
			SetSpecified(9, value);
		}
	}

	protected override int MaxSpecifiedBits => 10;

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
		return (InfopartChartDefinition)base.Clone();
	}
}
