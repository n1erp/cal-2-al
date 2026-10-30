using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlRoot(ElementName = "Controls", Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects", IsNullable = false)]
[XmlInclude(typeof(InfopartBaseDefinition))]
[XmlInclude(typeof(InfopartChartDefinition))]
[XmlInclude(typeof(InfopartSystemDefinition))]
[XmlInclude(typeof(InfopartPageDefinition))]
[XmlInclude(typeof(ControlContainerPlaceHolder))]
[XmlInclude(typeof(ControlGroupBaseDefinition))]
[XmlInclude(typeof(StackGroupDefinition))]
[XmlInclude(typeof(RepeaterDefinition))]
[XmlInclude(typeof(GridLayoutDefinition))]
[XmlInclude(typeof(ColumnLayoutDefinition))]
[XmlInclude(typeof(ControlGroupDefinition))]
[XmlInclude(typeof(ReportDefinition))]
[XmlInclude(typeof(RecordLinksControlDefinition))]
[XmlInclude(typeof(OutlookOverviewDefinition))]
[XmlInclude(typeof(ControlDataboundDefinition))]
[XmlInclude(typeof(FilterControlDefinition))]
[XmlInclude(typeof(ControlDefinition))]
[XmlInclude(typeof(ProgressControlDefinition))]
[XmlInclude(typeof(NotesControlDefinition))]
[XmlInclude(typeof(ActionBaseDefinition))]
[XmlInclude(typeof(ActionDefinition))]
[XmlInclude(typeof(ActionContainerPlaceHolder))]
[XmlInclude(typeof(ActionGroupBaseDefinition))]
[XmlInclude(typeof(NavigationGroupDefinition))]
[XmlInclude(typeof(ActionGroupDefinition))]
[XmlInclude(typeof(SeparatorDefinition))]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[KnownType(typeof(InfopartBaseDefinition))]
[KnownType(typeof(InfopartChartDefinition))]
[KnownType(typeof(InfopartSystemDefinition))]
[KnownType(typeof(InfopartPageDefinition))]
[KnownType(typeof(ControlContainerPlaceHolder))]
[KnownType(typeof(ControlGroupBaseDefinition))]
[KnownType(typeof(StackGroupDefinition))]
[KnownType(typeof(RepeaterDefinition))]
[KnownType(typeof(GridLayoutDefinition))]
[KnownType(typeof(ColumnLayoutDefinition))]
[KnownType(typeof(ControlGroupDefinition))]
[KnownType(typeof(ReportDefinition))]
[KnownType(typeof(RecordLinksControlDefinition))]
[KnownType(typeof(OutlookOverviewDefinition))]
[KnownType(typeof(ControlDataboundDefinition))]
[KnownType(typeof(FilterControlDefinition))]
[KnownType(typeof(ControlDefinition))]
[KnownType(typeof(ProgressControlDefinition))]
[KnownType(typeof(NotesControlDefinition))]
[KnownType(typeof(ActionBaseDefinition))]
[KnownType(typeof(ActionDefinition))]
[KnownType(typeof(ActionContainerPlaceHolder))]
[KnownType(typeof(ActionGroupBaseDefinition))]
[KnownType(typeof(NavigationGroupDefinition))]
[KnownType(typeof(ActionGroupDefinition))]
[KnownType(typeof(SeparatorDefinition))]
[DataContract(Name = "CBD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class ControlBaseDefinition : UIElementDefinition
{
	private List<ActionBaseDefinition> actionsField;

	[DataMember(Name = "CBD_2", EmitDefaultValue = false, IsRequired = false)]
	private string captionColumnNameField;

	[DataMember(Name = "CBD_3", EmitDefaultValue = false, IsRequired = false)]
	private string captionClassField;

	private bool drillDownField;

	private bool lookupField;

	private bool assistEditField;

	private bool dropDownField;

	private bool closingDatesField;

	private bool numericField;

	private bool dateFormulaField;

	private NavAppExtensionType sourceExtensionTypeField;

	[DataMember(Name = "CBD_12", EmitDefaultValue = false, IsRequired = false)]
	private string helpLinkField;

	[DataMember(Name = "CBD_13", EmitDefaultValue = false, IsRequired = false)]
	private string helpBaseLinkField;

	[DataMember(Name = "CBD_14", EmitDefaultValue = false, IsRequired = false)]
	private string supportedLocalesField;

	[XmlElement("Actions")]
	[DataMember(Name = "CBD_1", EmitDefaultValue = false, IsRequired = false)]
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
	public string CaptionColumnName
	{
		get
		{
			return captionColumnNameField;
		}
		set
		{
			captionColumnNameField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string CaptionClass
	{
		get
		{
			if (captionClassField == null)
			{
				return "";
			}
			return captionClassField;
		}
		set
		{
			captionClassField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CBD_4", EmitDefaultValue = false, IsRequired = false)]
	public bool DrillDown
	{
		get
		{
			return drillDownField;
		}
		set
		{
			drillDownField = value;
			DrillDownSpecified = true;
		}
	}

	[XmlIgnore]
	public bool DrillDownSpecified
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
	[DataMember(Name = "CBD_5", EmitDefaultValue = false, IsRequired = false)]
	public bool Lookup
	{
		get
		{
			return lookupField;
		}
		set
		{
			lookupField = value;
			LookupSpecified = true;
		}
	}

	[XmlIgnore]
	public bool LookupSpecified
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
	[DataMember(Name = "CBD_6", EmitDefaultValue = false, IsRequired = false)]
	public bool AssistEdit
	{
		get
		{
			return assistEditField;
		}
		set
		{
			assistEditField = value;
			AssistEditSpecified = true;
		}
	}

	[XmlIgnore]
	public bool AssistEditSpecified
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
	[DataMember(Name = "CBD_7", EmitDefaultValue = false, IsRequired = false)]
	public bool DropDown
	{
		get
		{
			return dropDownField;
		}
		set
		{
			dropDownField = value;
			DropDownSpecified = true;
		}
	}

	[XmlIgnore]
	public bool DropDownSpecified
	{
		get
		{
			return GetSpecified(5);
		}
		set
		{
			SetSpecified(5, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CBD_8", EmitDefaultValue = false, IsRequired = false)]
	public bool ClosingDates
	{
		get
		{
			return closingDatesField;
		}
		set
		{
			closingDatesField = value;
			ClosingDatesSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ClosingDatesSpecified
	{
		get
		{
			return GetSpecified(6);
		}
		set
		{
			SetSpecified(6, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CBD_9", EmitDefaultValue = false, IsRequired = false)]
	public bool Numeric
	{
		get
		{
			return numericField;
		}
		set
		{
			numericField = value;
			NumericSpecified = true;
		}
	}

	[XmlIgnore]
	public bool NumericSpecified
	{
		get
		{
			return GetSpecified(7);
		}
		set
		{
			SetSpecified(7, value);
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "CBD_10", EmitDefaultValue = false, IsRequired = false)]
	public bool DateFormula
	{
		get
		{
			return dateFormulaField;
		}
		set
		{
			dateFormulaField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CBD_11", EmitDefaultValue = false, IsRequired = false)]
	public NavAppExtensionType SourceExtensionType
	{
		get
		{
			return sourceExtensionTypeField;
		}
		set
		{
			sourceExtensionTypeField = value;
			SourceExtensionTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool SourceExtensionTypeSpecified
	{
		get
		{
			return GetSpecified(8);
		}
		set
		{
			SetSpecified(8, value);
		}
	}

	[XmlAttribute]
	public string HelpLink
	{
		get
		{
			return helpLinkField;
		}
		set
		{
			helpLinkField = value;
		}
	}

	[XmlAttribute]
	public string HelpBaseLink
	{
		get
		{
			return helpBaseLinkField;
		}
		set
		{
			helpBaseLinkField = value;
		}
	}

	[XmlAttribute]
	public string SupportedLocales
	{
		get
		{
			return supportedLocalesField;
		}
		set
		{
			supportedLocalesField = value;
		}
	}

	protected override int MaxSpecifiedBits => 9;

	public override void MergeMenuSuiteMultiLanguage(int languageAppGroupId)
	{
		base.MergeMenuSuiteMultiLanguage(languageAppGroupId);
		CaptionML = MetaPage.MultiLanguageTextMerge(CaptionML, ControlGUID, languageAppGroupId);
		if (actionsField != null)
		{
			for (int i = 0; i < actionsField.Count; i++)
			{
				actionsField[i].MergeMenuSuiteMultiLanguage(languageAppGroupId);
			}
		}
	}

	protected internal ControlBaseDefinition()
	{
		dateFormulaField = false;
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
		ControlBaseDefinition controlBaseDefinition = (ControlBaseDefinition)base.Clone();
		if (actionsField != null)
		{
			controlBaseDefinition.Actions = new List<ActionBaseDefinition>();
			for (int i = 0; i < actionsField.Count; i++)
			{
				controlBaseDefinition.Actions.Add((ActionBaseDefinition)actionsField[i].Clone());
			}
		}
		return controlBaseDefinition;
	}

	public override void SolveProperties(MetaField field, MetaTable metaTable, MasterPage masterPage)
	{
		base.SolveProperties(field, metaTable, masterPage);
		PropertiesSolveHelper.SolveCaptionClass(this, field, masterPage);
		PropertiesSolveHelper.SolveDropDown(this, field);
		PropertiesSolveHelper.SolveClosingDates(this, field);
		PropertiesSolveHelper.SolveDateFormula(this, field);
		PropertiesSolveHelper.SolveNumeric(this, field);
	}
}
