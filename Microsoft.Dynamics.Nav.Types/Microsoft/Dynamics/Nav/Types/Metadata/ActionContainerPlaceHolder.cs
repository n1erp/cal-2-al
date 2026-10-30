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
[DataContract(Name = "ACPH", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ActionContainerPlaceHolder : ActionBaseDefinition
{
	private ActionContainerType containerTypeField;

	[DataMember(Name = "ACPH_2", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipField;

	[DataMember(Name = "ACPH_3", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipMLField;

	[XmlAttribute]
	[DataMember(Name = "ACPH_1", EmitDefaultValue = false, IsRequired = false)]
	public ActionContainerType ContainerType
	{
		get
		{
			return containerTypeField;
		}
		set
		{
			containerTypeField = value;
			ContainerTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ContainerTypeSpecified
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
	public string ToolTip
	{
		get
		{
			return toolTipField;
		}
		set
		{
			toolTipField = value;
		}
	}

	[XmlAttribute]
	public string ToolTipML
	{
		get
		{
			return toolTipMLField;
		}
		set
		{
			toolTipMLField = value;
		}
	}

	protected override int MaxSpecifiedBits => 11;

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (!string.IsNullOrEmpty(toolTipMLField))
		{
			toolTipField = MultiLanguage.GetLanguageValue(language, primaryLanguage, toolTipMLField);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		toolTipMLField = MetaPage.MultiLanguageTextMerge(toolTipMLField, objType, id, ComponentType(), ComponentId(), 4, languageAppGroupId);
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
		return (ActionContainerPlaceHolder)base.Clone();
	}
}
