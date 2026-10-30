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
[DataContract(Name = "AGD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ActionGroupDefinition : ActionGroupBaseDefinition
{
	private bool isDepartmentPageField;

	[DataMember(Name = "AGD_2", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipField;

	[DataMember(Name = "AGD_3", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipMLField;

	[DataMember(Name = "AGD_4", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipResourceIdentifierField;

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "AGD_1", EmitDefaultValue = false, IsRequired = false)]
	public bool IsDepartmentPage
	{
		get
		{
			return isDepartmentPageField;
		}
		set
		{
			isDepartmentPageField = value;
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

	[XmlAttribute]
	public string ToolTipResourceIdentifier
	{
		get
		{
			return toolTipResourceIdentifierField;
		}
		set
		{
			toolTipResourceIdentifierField = value;
		}
	}

	public ActionGroupDefinition()
	{
		isDepartmentPageField = false;
	}

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
		return (ActionGroupDefinition)base.Clone();
	}
}
