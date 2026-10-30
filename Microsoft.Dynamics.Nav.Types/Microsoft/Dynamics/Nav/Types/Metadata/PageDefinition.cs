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
[XmlRoot(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects", IsNullable = false)]
[DataContract(Name = "PD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class PageDefinition : ConceptDefinition
{
	private PageProperties propertiesField;

	private ContentDefinition contentField;

	private List<ActionContainerDefinition> actionContainersField;

	private List<PageViewDefinition> viewContainersField;

	private List<DataFieldDefinition> expressionsField;

	private List<PageTriggerDefinition> triggersField;

	[DataMember(Name = "PD_7", EmitDefaultValue = false, IsRequired = false)]
	private string metadataVersionField;

	[DataMember(Name = "PD_1", EmitDefaultValue = false, IsRequired = false)]
	public PageProperties Properties
	{
		get
		{
			return propertiesField;
		}
		set
		{
			propertiesField = value;
		}
	}

	[DataMember(Name = "PD_2", EmitDefaultValue = false, IsRequired = false)]
	public ContentDefinition Content
	{
		get
		{
			return contentField;
		}
		set
		{
			contentField = value;
		}
	}

	[XmlElement("ActionContainers")]
	[DataMember(Name = "PD_3", EmitDefaultValue = false, IsRequired = false)]
	public List<ActionContainerDefinition> ActionContainers
	{
		get
		{
			return actionContainersField;
		}
		set
		{
			actionContainersField = value;
		}
	}

	[XmlArrayItem("Views", IsNullable = false)]
	[DataMember(Name = "PD_4", EmitDefaultValue = false, IsRequired = false)]
	public List<PageViewDefinition> ViewContainers
	{
		get
		{
			return viewContainersField;
		}
		set
		{
			viewContainersField = value;
		}
	}

	[XmlArrayItem("Expression", IsNullable = false)]
	[DataMember(Name = "PD_5", EmitDefaultValue = false, IsRequired = false)]
	public List<DataFieldDefinition> Expressions
	{
		get
		{
			return expressionsField;
		}
		set
		{
			expressionsField = value;
		}
	}

	[XmlArrayItem("Trigger", IsNullable = false)]
	[DataMember(Name = "PD_6", EmitDefaultValue = false, IsRequired = false)]
	public List<PageTriggerDefinition> Triggers
	{
		get
		{
			return triggersField;
		}
		set
		{
			triggersField = value;
		}
	}

	[XmlAttribute(DataType = "integer")]
	public string MetadataVersion
	{
		get
		{
			return metadataVersionField;
		}
		set
		{
			metadataVersionField = value;
		}
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (actionContainersField != null)
		{
			for (int i = 0; i < actionContainersField.Count; i++)
			{
				actionContainersField[i].SetLanguage(language, primaryLanguage);
			}
		}
		if (viewContainersField != null)
		{
			for (int i = 0; i < viewContainersField.Count; i++)
			{
				viewContainersField[i].SetLanguage(language, primaryLanguage);
			}
		}
		if (expressionsField != null)
		{
			for (int i = 0; i < expressionsField.Count; i++)
			{
				expressionsField[i].SetLanguage(language, primaryLanguage);
			}
		}
		if (triggersField != null)
		{
			for (int i = 0; i < triggersField.Count; i++)
			{
				triggersField[i].SetLanguage(language, primaryLanguage);
			}
		}
		if (propertiesField != null)
		{
			propertiesField.SetLanguage(language, primaryLanguage);
		}
		if (contentField != null)
		{
			contentField.SetLanguage(language, primaryLanguage);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		if (actionContainersField != null)
		{
			for (int i = 0; i < actionContainersField.Count; i++)
			{
				actionContainersField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
		if (viewContainersField != null)
		{
			for (int i = 0; i < viewContainersField.Count; i++)
			{
				viewContainersField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
		if (expressionsField != null)
		{
			for (int i = 0; i < expressionsField.Count; i++)
			{
				expressionsField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
		if (triggersField != null)
		{
			for (int i = 0; i < triggersField.Count; i++)
			{
				triggersField[i].MergeMultiLanguage(objType, id, languageAppGroupId);
			}
		}
		if (propertiesField != null)
		{
			propertiesField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
		if (contentField != null)
		{
			contentField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
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
		if (propertiesField != null)
		{
			elementDefinition = propertiesField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (contentField != null)
		{
			elementDefinition = contentField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (actionContainersField != null)
		{
			for (int i = 0; i < actionContainersField.Count; i++)
			{
				elementDefinition = actionContainersField[i].Find(match);
				if (elementDefinition != null)
				{
					return elementDefinition;
				}
			}
		}
		if (viewContainersField != null)
		{
			for (int i = 0; i < viewContainersField.Count; i++)
			{
				elementDefinition = viewContainersField[i].Find(match);
				if (elementDefinition != null)
				{
					return elementDefinition;
				}
			}
		}
		if (expressionsField != null)
		{
			for (int i = 0; i < expressionsField.Count; i++)
			{
				elementDefinition = expressionsField[i].Find(match);
				if (elementDefinition != null)
				{
					return elementDefinition;
				}
			}
		}
		if (triggersField != null)
		{
			for (int i = 0; i < triggersField.Count; i++)
			{
				elementDefinition = triggersField[i].Find(match);
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
		if (propertiesField != null)
		{
			propertiesField.FindAllCore(result, match);
		}
		if (contentField != null)
		{
			contentField.FindAllCore(result, match);
		}
		if (actionContainersField != null)
		{
			for (int i = 0; i < actionContainersField.Count; i++)
			{
				actionContainersField[i].FindAllCore(result, match);
			}
		}
		if (viewContainersField != null)
		{
			for (int i = 0; i < viewContainersField.Count; i++)
			{
				viewContainersField[i].FindAllCore(result, match);
			}
		}
		if (expressionsField != null)
		{
			for (int i = 0; i < expressionsField.Count; i++)
			{
				expressionsField[i].FindAllCore(result, match);
			}
		}
		if (triggersField != null)
		{
			for (int i = 0; i < triggersField.Count; i++)
			{
				triggersField[i].FindAllCore(result, match);
			}
		}
	}

	public override object Clone()
	{
		PageDefinition pageDefinition = (PageDefinition)base.Clone();
		if (propertiesField != null)
		{
			pageDefinition.Properties = (PageProperties)propertiesField.Clone();
		}
		if (contentField != null)
		{
			pageDefinition.Content = (ContentDefinition)contentField.Clone();
		}
		if (actionContainersField != null)
		{
			pageDefinition.ActionContainers = new List<ActionContainerDefinition>();
			for (int i = 0; i < actionContainersField.Count; i++)
			{
				pageDefinition.ActionContainers.Add((ActionContainerDefinition)actionContainersField[i].Clone());
			}
		}
		if (viewContainersField != null)
		{
			pageDefinition.ViewContainers = new List<PageViewDefinition>();
			for (int j = 0; j < viewContainersField.Count; j++)
			{
				pageDefinition.ViewContainers.Add((PageViewDefinition)viewContainersField[j].Clone());
			}
		}
		if (expressionsField != null)
		{
			pageDefinition.Expressions = new List<DataFieldDefinition>();
			for (int k = 0; k < expressionsField.Count; k++)
			{
				pageDefinition.Expressions.Add((DataFieldDefinition)expressionsField[k].Clone());
			}
		}
		if (triggersField != null)
		{
			pageDefinition.Triggers = new List<PageTriggerDefinition>();
			for (int l = 0; l < triggersField.Count; l++)
			{
				pageDefinition.Triggers.Add((PageTriggerDefinition)triggersField[l].Clone());
			}
		}
		return pageDefinition;
	}
}
