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
[DataContract(Name = "CD1", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[XmlRoot(ElementName = "Controls", Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects", IsNullable = false)]
public class ControlDefinition : ControlDataboundDefinition
{
	private ControlAddInManifestDefinition controlAddInManifestField;

	[DataMember(Name = "CD1_2", EmitDefaultValue = false, IsRequired = false)]
	private string formatColumnNameField;

	[DataMember(Name = "CD1_3", EmitDefaultValue = false, IsRequired = false)]
	private string optionCaptionField;

	[DataMember(Name = "CD1_4", EmitDefaultValue = false, IsRequired = false)]
	private string optionCaptionMLField;

	[DataMember(Name = "CD1_5", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipField;

	[DataMember(Name = "CD1_6", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipMLField;

	[DataMember(Name = "CD1_7", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipResourceIdentifierField;

	private bool notBlankField;

	[DataMember(Name = "CD1_9", EmitDefaultValue = false, IsRequired = false)]
	private string charAllowedField;

	[DataMember(Name = "CD1_10", EmitDefaultValue = false, IsRequired = false)]
	private string valuesAllowedField;

	private BlankNumbers blankNumbersField;

	private bool blankZeroField;

	private bool multiLineField;

	private Importance importanceField;

	private int drillDownFormIDField;

	private int lookupFormIDField;

	[DataMember(Name = "CD1_17", EmitDefaultValue = false, IsRequired = false)]
	private string hideValueField;

	[DataMember(Name = "CD1_18", EmitDefaultValue = false, IsRequired = false)]
	private string controlAddInField;

	private int accessByObjectIDField;

	private AccessByObjectType accessByObjectTypeField;

	private int accessByPermissionMaskField;

	[DataMember(Name = "CD1_22", EmitDefaultValue = false, IsRequired = false)]
	private string applicationAreaField;

	[DataMember(Name = "CD1_23", EmitDefaultValue = false, IsRequired = false)]
	private string quickEntryField;

	private bool showCaptionField;

	private int rowSpanField;

	private int columnSpanField;

	[DataMember(Name = "CD1_27", EmitDefaultValue = false, IsRequired = false)]
	private string entitySetNameField;

	[DataMember(Name = "CD1_28", EmitDefaultValue = false, IsRequired = false)]
	private string entityNameField;

	[DataMember(Name = "CD1_1", EmitDefaultValue = false, IsRequired = false)]
	public ControlAddInManifestDefinition ControlAddInManifest
	{
		get
		{
			return controlAddInManifestField;
		}
		set
		{
			controlAddInManifestField = value;
		}
	}

	[XmlAttribute]
	public string FormatColumnName
	{
		get
		{
			return formatColumnNameField;
		}
		set
		{
			formatColumnNameField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string OptionCaption
	{
		get
		{
			if (optionCaptionField == null)
			{
				return "";
			}
			return optionCaptionField;
		}
		set
		{
			optionCaptionField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string OptionCaptionML
	{
		get
		{
			if (optionCaptionMLField == null)
			{
				return "";
			}
			return optionCaptionMLField;
		}
		set
		{
			optionCaptionMLField = value;
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

	[XmlAttribute]
	[DataMember(Name = "CD1_8", EmitDefaultValue = false, IsRequired = false)]
	public bool NotBlank
	{
		get
		{
			return notBlankField;
		}
		set
		{
			notBlankField = value;
			NotBlankSpecified = true;
		}
	}

	[XmlIgnore]
	public bool NotBlankSpecified
	{
		get
		{
			return GetSpecified(12);
		}
		set
		{
			SetSpecified(12, value);
		}
	}

	[XmlAttribute]
	public string CharAllowed
	{
		get
		{
			return charAllowedField;
		}
		set
		{
			charAllowedField = value;
		}
	}

	[XmlAttribute]
	public string ValuesAllowed
	{
		get
		{
			return valuesAllowedField;
		}
		set
		{
			valuesAllowedField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CD1_11", EmitDefaultValue = false, IsRequired = false)]
	public BlankNumbers BlankNumbers
	{
		get
		{
			return blankNumbersField;
		}
		set
		{
			blankNumbersField = value;
			BlankNumbersSpecified = true;
		}
	}

	[XmlIgnore]
	public bool BlankNumbersSpecified
	{
		get
		{
			return GetSpecified(13);
		}
		set
		{
			SetSpecified(13, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CD1_12", EmitDefaultValue = false, IsRequired = false)]
	public bool BlankZero
	{
		get
		{
			return blankZeroField;
		}
		set
		{
			blankZeroField = value;
			BlankZeroSpecified = true;
		}
	}

	[XmlIgnore]
	public bool BlankZeroSpecified
	{
		get
		{
			return GetSpecified(14);
		}
		set
		{
			SetSpecified(14, value);
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "CD1_13", EmitDefaultValue = false, IsRequired = false)]
	public bool MultiLine
	{
		get
		{
			return multiLineField;
		}
		set
		{
			multiLineField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CD1_14", EmitDefaultValue = false, IsRequired = false)]
	public Importance Importance
	{
		get
		{
			return importanceField;
		}
		set
		{
			importanceField = value;
			ImportanceSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ImportanceSpecified
	{
		get
		{
			return GetSpecified(15);
		}
		set
		{
			SetSpecified(15, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CD1_15", EmitDefaultValue = false, IsRequired = false)]
	public int DrillDownFormID
	{
		get
		{
			return drillDownFormIDField;
		}
		set
		{
			drillDownFormIDField = value;
			DrillDownFormIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool DrillDownFormIDSpecified
	{
		get
		{
			return GetSpecified(16);
		}
		set
		{
			SetSpecified(16, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CD1_16", EmitDefaultValue = false, IsRequired = false)]
	public int LookupFormID
	{
		get
		{
			return lookupFormIDField;
		}
		set
		{
			lookupFormIDField = value;
			LookupFormIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool LookupFormIDSpecified
	{
		get
		{
			return GetSpecified(17);
		}
		set
		{
			SetSpecified(17, value);
		}
	}

	[XmlAttribute]
	[DefaultValue("false")]
	public string HideValue
	{
		get
		{
			if (hideValueField == null)
			{
				return "false";
			}
			return hideValueField;
		}
		set
		{
			hideValueField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string ControlAddIn
	{
		get
		{
			if (controlAddInField == null)
			{
				return "";
			}
			return controlAddInField;
		}
		set
		{
			controlAddInField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CD1_19", EmitDefaultValue = false, IsRequired = false)]
	public int AccessByObjectID
	{
		get
		{
			return accessByObjectIDField;
		}
		set
		{
			accessByObjectIDField = value;
			AccessByObjectIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool AccessByObjectIDSpecified
	{
		get
		{
			return GetSpecified(18);
		}
		set
		{
			SetSpecified(18, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CD1_20", EmitDefaultValue = false, IsRequired = false)]
	public AccessByObjectType AccessByObjectType
	{
		get
		{
			return accessByObjectTypeField;
		}
		set
		{
			accessByObjectTypeField = value;
			AccessByObjectTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool AccessByObjectTypeSpecified
	{
		get
		{
			return GetSpecified(19);
		}
		set
		{
			SetSpecified(19, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "CD1_21", EmitDefaultValue = false, IsRequired = false)]
	public int AccessByPermissionMask
	{
		get
		{
			return accessByPermissionMaskField;
		}
		set
		{
			accessByPermissionMaskField = value;
			AccessByPermissionMaskSpecified = true;
		}
	}

	[XmlIgnore]
	public bool AccessByPermissionMaskSpecified
	{
		get
		{
			return GetSpecified(20);
		}
		set
		{
			SetSpecified(20, value);
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

	[XmlAttribute]
	[DefaultValue("true")]
	public string QuickEntry
	{
		get
		{
			if (quickEntryField == null)
			{
				return "true";
			}
			return quickEntryField;
		}
		set
		{
			quickEntryField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(true)]
	[DataMember(Name = "CD1_24", EmitDefaultValue = false, IsRequired = false)]
	public bool ShowCaption
	{
		get
		{
			return showCaptionField;
		}
		set
		{
			showCaptionField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(1)]
	[DataMember(Name = "CD1_25", EmitDefaultValue = false, IsRequired = false)]
	public int RowSpan
	{
		get
		{
			return rowSpanField;
		}
		set
		{
			rowSpanField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(1)]
	[DataMember(Name = "CD1_26", EmitDefaultValue = false, IsRequired = false)]
	public int ColumnSpan
	{
		get
		{
			return columnSpanField;
		}
		set
		{
			columnSpanField = value;
		}
	}

	[XmlAttribute]
	public string EntitySetName
	{
		get
		{
			return entitySetNameField;
		}
		set
		{
			entitySetNameField = value;
		}
	}

	[XmlAttribute]
	public string EntityName
	{
		get
		{
			return entityNameField;
		}
		set
		{
			entityNameField = value;
		}
	}

	protected override int MaxSpecifiedBits => 21;

	public ControlDefinition()
	{
		multiLineField = false;
		showCaptionField = true;
		rowSpanField = 1;
		columnSpanField = 1;
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (!string.IsNullOrEmpty(optionCaptionMLField))
		{
			optionCaptionField = MultiLanguage.GetLanguageValue(language, primaryLanguage, optionCaptionMLField);
		}
		if (!string.IsNullOrEmpty(toolTipMLField))
		{
			toolTipField = MultiLanguage.GetLanguageValue(language, primaryLanguage, toolTipMLField);
		}
		if (controlAddInManifestField != null)
		{
			controlAddInManifestField.SetLanguage(language, primaryLanguage);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		optionCaptionMLField = MetaPage.MultiLanguageTextMerge(optionCaptionMLField, objType, id, ComponentType(), ComponentId(), 2, languageAppGroupId);
		toolTipMLField = MetaPage.MultiLanguageTextMerge(toolTipMLField, objType, id, ComponentType(), ComponentId(), 4, languageAppGroupId);
		if (controlAddInManifestField != null)
		{
			controlAddInManifestField.MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (controlAddInManifestField != null)
		{
			return controlAddInManifestField.Find(match);
		}
		return elementDefinition;
	}

	public override void FindAllCore(List<ElementDefinition> result, Predicate<ElementDefinition> match)
	{
		base.FindAllCore(result, match);
		if (controlAddInManifestField != null)
		{
			controlAddInManifestField.FindAllCore(result, match);
		}
	}

	public override object Clone()
	{
		ControlDefinition controlDefinition = (ControlDefinition)base.Clone();
		if (controlAddInManifestField != null)
		{
			controlDefinition.ControlAddInManifest = (ControlAddInManifestDefinition)controlAddInManifestField.Clone();
		}
		return controlDefinition;
	}

	public override void SolveProperties(MetaField field, MetaTable metaTable, MasterPage masterPage)
	{
		base.SolveProperties(field, metaTable, masterPage);
		PropertiesSolveHelper.SolveCharAllowed(this, field);
		PropertiesSolveHelper.SolveValuesAllowed(this, field);
		PropertiesSolveHelper.SolveBlankNumbers(this, field);
		PropertiesSolveHelper.SolveNotBlank(this, field);
		PropertiesSolveHelper.SolveBlankZero(this, field);
		PropertiesSolveHelper.SolveLookup(this, field);
		PropertiesSolveHelper.SolveDrillDown(this, field);
		PropertiesSolveHelper.SolveMinMax(this, field);
		PropertiesSolveHelper.SolveAssistEdit(this);
		PropertiesSolveHelper.SolveOptionString(this, field);
	}
}
