using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[XmlRoot(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects", IsNullable = false)]
[DataContract(Name = "MP", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MasterPage : ConceptDefinition
{
	public delegate void GetControlsAndActionsOnPageDelegate(int objectId, int appGroupId, out bool onAfterGetCurrentRecordEnabled, out ControlDataboundDefinition[] controls, out ActionDefinition[] actions);

	private CommandBarDefinition commandBarField;

	private ContentAreaDefinition contentAreaField;

	private InfopartsAreaDefinition infopartsAreaField;

	private PageProperties pagePropertiesField;

	private NavigationAreaDefinition navigationAreaField;

	private List<DataFieldDefinition> expressionsField;

	private List<PageTriggerDefinition> triggersField;

	[DataMember(Name = "MP_8", EmitDefaultValue = false, IsRequired = false)]
	private List<string> removedControlsField;

	private long versionNumberField;

	private bool showFilterPaneField;

	private bool showInfoPartPaneField;

	private bool defaultActionIsEditField;

	[DataMember(Name = "MP_14", EmitDefaultValue = false, IsRequired = false)]
	private string defaultFilterColumnNameField;

	public static GetControlsAndActionsOnPageDelegate GetControlsAndActionsOnPage { get; set; }

	public int LanguageAppGroupId { get; set; }

	[DataMember(Name = "MP_1", EmitDefaultValue = false, IsRequired = false)]
	public CommandBarDefinition CommandBar
	{
		get
		{
			return commandBarField;
		}
		set
		{
			commandBarField = value;
		}
	}

	[DataMember(Name = "MP_2", EmitDefaultValue = false, IsRequired = false)]
	public ContentAreaDefinition ContentArea
	{
		get
		{
			return contentAreaField;
		}
		set
		{
			contentAreaField = value;
		}
	}

	[DataMember(Name = "MP_3", EmitDefaultValue = false, IsRequired = false)]
	public InfopartsAreaDefinition InfopartsArea
	{
		get
		{
			return infopartsAreaField;
		}
		set
		{
			infopartsAreaField = value;
		}
	}

	[DataMember(Name = "MP_4", EmitDefaultValue = false, IsRequired = false)]
	public PageProperties PageProperties
	{
		get
		{
			return pagePropertiesField;
		}
		set
		{
			pagePropertiesField = value;
		}
	}

	[DataMember(Name = "MP_5", EmitDefaultValue = false, IsRequired = false)]
	public NavigationAreaDefinition NavigationArea
	{
		get
		{
			return navigationAreaField;
		}
		set
		{
			navigationAreaField = value;
		}
	}

	[XmlArrayItem("Expression", IsNullable = false)]
	[DataMember(Name = "MP_6", EmitDefaultValue = false, IsRequired = false)]
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
	[DataMember(Name = "MP_7", EmitDefaultValue = false, IsRequired = false)]
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

	[XmlElement("RemovedControls")]
	[DataMember(Name = "MP_9", EmitDefaultValue = false, IsRequired = false)]
	public List<string> RemovedControls
	{
		get
		{
			return removedControlsField;
		}
		set
		{
			removedControlsField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "MP_10", EmitDefaultValue = false, IsRequired = false)]
	public long VersionNumber
	{
		get
		{
			return versionNumberField;
		}
		set
		{
			versionNumberField = value;
			VersionNumberSpecified = true;
		}
	}

	[XmlIgnore]
	public bool VersionNumberSpecified
	{
		get
		{
			return GetSpecified(2);
		}
		set
		{
			SetSpecified(2, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "MP_11", EmitDefaultValue = false, IsRequired = false)]
	public bool ShowFilterPane
	{
		get
		{
			return showFilterPaneField;
		}
		set
		{
			showFilterPaneField = value;
			ShowFilterPaneSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ShowFilterPaneSpecified
	{
		get
		{
			return GetSpecified(3);
		}
		set
		{
			SetSpecified(3, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "MP_12", EmitDefaultValue = false, IsRequired = false)]
	public bool ShowInfoPartPane
	{
		get
		{
			return showInfoPartPaneField;
		}
		set
		{
			showInfoPartPaneField = value;
			ShowInfoPartPaneSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ShowInfoPartPaneSpecified
	{
		get
		{
			return GetSpecified(4);
		}
		set
		{
			SetSpecified(4, value);
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "MP_13", EmitDefaultValue = false, IsRequired = false)]
	public bool DefaultActionIsEdit
	{
		get
		{
			return defaultActionIsEditField;
		}
		set
		{
			defaultActionIsEditField = value;
		}
	}

	[XmlAttribute]
	public string DefaultFilterColumnName
	{
		get
		{
			return defaultFilterColumnNameField;
		}
		set
		{
			defaultFilterColumnNameField = value;
		}
	}

	protected override int MaxSpecifiedBits => 5;

	[OnSerializing]
	private void OnSerializing(StreamingContext context)
	{
		if (GetControlsAndActionsOnPage == null || !PageSupportsEvents())
		{
			return;
		}
		ControlDataboundDefinition[] source = FindAll((ElementDefinition e) => e is ControlDataboundDefinition).Cast<ControlDataboundDefinition>().ToArray();
		ActionDefinition[] source2 = FindAll((ElementDefinition e) => e is ActionDefinition).Cast<ActionDefinition>().ToArray();
		GetControlsAndActionsOnPage(ID, LanguageAppGroupId, out var onAfterGetCurrentRecordEnabled, out var controls, out var actions);
		Dictionary<int, ControlDataboundDefinition> masterFieldsDict = controls.ToDictionary((ControlDataboundDefinition x) => x.ID);
		Dictionary<int, ActionDefinition> masterActionsDict = actions.ToDictionary((ActionDefinition x) => x.ID);
		if (PageProperties.OnAfterGetCurrentRecordEnabled != onAfterGetCurrentRecordEnabled)
		{
			PageProperties.OnAfterGetCurrentRecordEnabled = onAfterGetCurrentRecordEnabled;
			PageProperties.OnAfterGetCurrentRecordEnabledSpecified = false;
		}
		foreach (ControlDataboundDefinition item in source.Where((ControlDataboundDefinition field) => masterFieldsDict.ContainsKey(field.ID)))
		{
			item.HasOnValidateEventSubscriber = masterFieldsDict[item.ID].HasOnValidateEventSubscriber;
		}
		foreach (ActionDefinition item2 in source2.Where((ActionDefinition action) => masterActionsDict.ContainsKey(action.ID)))
		{
			if (item2.Trigger == null || masterActionsDict[item2.ID].Trigger != null)
			{
				item2.Trigger = masterActionsDict[item2.ID].Trigger;
			}
		}
	}

	public bool PageSupportsEvents()
	{
		if (PageProperties == null || PageProperties.SourceObject.PageDataSource == DataSourceType.Query)
		{
			return false;
		}
		if (SystemPages.IsServerSystemPageId(ID))
		{
			return false;
		}
		if (string.IsNullOrEmpty(ResourceIdentifier) && PageProperties.PageType != PageType.ReportPreview && PageProperties.PageType != PageType.ReportProcessingOnly && PageProperties.PageType != PageType.ReportViewer && PageProperties.PageType != PageType.FilterPage && PageProperties.PageType != PageType.XmlPort)
		{
			return PageProperties.PageType != PageType.API;
		}
		return false;
	}

	public MasterPage()
	{
		defaultActionIsEditField = false;
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
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
		if (commandBarField != null)
		{
			commandBarField.SetLanguage(language, primaryLanguage);
		}
		if (contentAreaField != null)
		{
			contentAreaField.SetLanguage(language, primaryLanguage);
		}
		if (infopartsAreaField != null)
		{
			infopartsAreaField.SetLanguage(language, primaryLanguage);
		}
		if (pagePropertiesField != null)
		{
			pagePropertiesField.SetLanguage(language, primaryLanguage);
		}
		if (navigationAreaField != null)
		{
			navigationAreaField.SetLanguage(language, primaryLanguage);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (commandBarField != null)
		{
			commandBarField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
		if (contentAreaField != null)
		{
			contentAreaField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
		if (infopartsAreaField != null)
		{
			infopartsAreaField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
		if (pagePropertiesField != null)
		{
			pagePropertiesField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
		if (navigationAreaField != null)
		{
			navigationAreaField.MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (commandBarField != null)
		{
			elementDefinition = commandBarField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (contentAreaField != null)
		{
			elementDefinition = contentAreaField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (infopartsAreaField != null)
		{
			elementDefinition = infopartsAreaField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (pagePropertiesField != null)
		{
			elementDefinition = pagePropertiesField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (navigationAreaField != null)
		{
			elementDefinition = navigationAreaField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
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
		if (commandBarField != null)
		{
			commandBarField.FindAllCore(result, match);
		}
		if (contentAreaField != null)
		{
			contentAreaField.FindAllCore(result, match);
		}
		if (infopartsAreaField != null)
		{
			infopartsAreaField.FindAllCore(result, match);
		}
		if (pagePropertiesField != null)
		{
			pagePropertiesField.FindAllCore(result, match);
		}
		if (navigationAreaField != null)
		{
			navigationAreaField.FindAllCore(result, match);
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
		MasterPage masterPage = (MasterPage)base.Clone();
		if (commandBarField != null)
		{
			masterPage.CommandBar = (CommandBarDefinition)commandBarField.Clone();
		}
		if (contentAreaField != null)
		{
			masterPage.ContentArea = (ContentAreaDefinition)contentAreaField.Clone();
		}
		if (infopartsAreaField != null)
		{
			masterPage.InfopartsArea = (InfopartsAreaDefinition)infopartsAreaField.Clone();
		}
		if (pagePropertiesField != null)
		{
			masterPage.PageProperties = (PageProperties)pagePropertiesField.Clone();
		}
		if (navigationAreaField != null)
		{
			masterPage.NavigationArea = (NavigationAreaDefinition)navigationAreaField.Clone();
		}
		if (expressionsField != null)
		{
			masterPage.Expressions = new List<DataFieldDefinition>();
			for (int i = 0; i < expressionsField.Count; i++)
			{
				masterPage.Expressions.Add((DataFieldDefinition)expressionsField[i].Clone());
			}
		}
		if (triggersField != null)
		{
			masterPage.Triggers = new List<PageTriggerDefinition>();
			for (int j = 0; j < triggersField.Count; j++)
			{
				masterPage.Triggers.Add((PageTriggerDefinition)triggersField[j].Clone());
			}
		}
		if (removedControlsField != null)
		{
			masterPage.RemovedControls = new List<string>();
			for (int k = 0; k < removedControlsField.Count; k++)
			{
				masterPage.RemovedControls.Add((string)removedControlsField[k].Clone());
			}
		}
		return masterPage;
	}
}
