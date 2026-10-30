using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "ACD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ActionContainerDefinition : ElementDefinition
{
	private List<ActionBaseDefinition> actionsField;

	private int idField;

	private ActionContainerType actionContainerTypeField;

	[DataMember(Name = "ACD_4", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipField;

	[DataMember(Name = "ACD_5", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipMLField;

	[DataMember(Name = "ACD_6", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipResourceIdentifierField;

	internal PageActionData PageActionData { get; set; }

	[XmlElement("Actions")]
	[DataMember(Name = "ACD_1", EmitDefaultValue = false, IsRequired = false)]
	public List<ActionBaseDefinition> Actions
	{
		get
		{
			return actionsField;
		}
		set
		{
			actionsField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "ACD_2", EmitDefaultValue = false, IsRequired = false)]
	public int ID
	{
		get
		{
			return idField;
		}
		set
		{
			idField = value;
			IDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool IDSpecified
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
	[DataMember(Name = "ACD_3", EmitDefaultValue = false, IsRequired = false)]
	public ActionContainerType ActionContainerType
	{
		get
		{
			return actionContainerTypeField;
		}
		set
		{
			actionContainerTypeField = value;
			ActionContainerTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ActionContainerTypeSpecified
	{
		get
		{
			return GetSpecified(1);
		}
		set
		{
			SetSpecified(1, value);
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

	protected override int MaxSpecifiedBits => 2;

	public override void MergeMenuSuiteMultiLanguage(int languageAppGroupId)
	{
		base.MergeMenuSuiteMultiLanguage(languageAppGroupId);
		if (actionsField != null)
		{
			for (int i = 0; i < actionsField.Count; i++)
			{
				actionsField[i].MergeMenuSuiteMultiLanguage(languageAppGroupId);
			}
		}
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (actionsField != null)
		{
			for (int i = 0; i < actionsField.Count; i++)
			{
				actionsField[i].SetLanguage(language, primaryLanguage);
			}
		}
		if (!string.IsNullOrEmpty(toolTipMLField))
		{
			toolTipField = MultiLanguage.GetLanguageValue(language, primaryLanguage, toolTipMLField);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (actionsField != null)
		{
			for (int i = 0; i < actionsField.Count; i++)
			{
				actionsField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
		toolTipMLField = MetaPage.MultiLanguageTextMerge(toolTipMLField, objType, id, ComponentType(), ComponentId(), 4, languageAppGroupId);
	}

	public override ElementDefinition Find(Predicate<ElementDefinition> match)
	{
		if (match(this))
		{
			return this;
		}
		ElementDefinition elementDefinition = base.Find(match);
		if (elementDefinition != null)
		{
			return elementDefinition;
		}
		if (actionsField != null)
		{
			for (int i = 0; i < actionsField.Count; i++)
			{
				elementDefinition = actionsField[i].Find(match);
				if (elementDefinition != null)
				{
					return elementDefinition;
				}
			}
		}
		return elementDefinition;
	}

	public override void FindAllCore(List<ElementDefinition> result, Predicate<ElementDefinition> match)
	{
		base.FindAllCore(result, match);
		if (actionsField != null)
		{
			for (int i = 0; i < actionsField.Count; i++)
			{
				actionsField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		ActionContainerDefinition actionContainerDefinition = (ActionContainerDefinition)base.Clone();
		if (actionsField != null)
		{
			actionContainerDefinition.Actions = new List<ActionBaseDefinition>();
			for (int i = 0; i < actionsField.Count; i++)
			{
				actionContainerDefinition.Actions.Add((ActionBaseDefinition)actionsField[i].Clone());
			}
		}
		return actionContainerDefinition;
	}
}
