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
[DataContract(Name = "AD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[XmlRoot(ElementName = "Actions", Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects", IsNullable = false)]
public class ActionDefinition : ActionBaseDefinition
{
	private LinkDefinition runFormLinkField;

	private ViewDefinition runFormViewField;

	private ActionTriggerDefinition triggerField;

	private PersonalizedFilterControlDefinition personalizedFilterField;

	private bool runPageOnRecField;

	private bool ellipsisField;

	[DataMember(Name = "AD_7", EmitDefaultValue = false, IsRequired = false)]
	private string shortCutKeyField;

	[DataMember(Name = "AD_8", EmitDefaultValue = false, IsRequired = false)]
	private string shortCutKeyMLField;

	[DataMember(Name = "AD_9", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipField;

	[DataMember(Name = "AD_10", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipMLField;

	[DataMember(Name = "AD_11", EmitDefaultValue = false, IsRequired = false)]
	private string additionalSearchTermsField;

	[DataMember(Name = "AD_12", EmitDefaultValue = false, IsRequired = false)]
	private string additionalSearchTermsMLField;

	[DataMember(Name = "AD_13", EmitDefaultValue = false, IsRequired = false)]
	private string toolTipResourceIdentifierField;

	[DataMember(Name = "AD_14", EmitDefaultValue = false, IsRequired = false)]
	private string shortCutKeyResourceIdentifierField;

	private int targetIDField;

	private RunObjectType runObjectTypeField;

	[DataMember(Name = "AD_17", EmitDefaultValue = false, IsRequired = false)]
	private string runObjectViewField;

	private bool isViewAppliedField;

	private bool isViewInUserScopeField;

	private int accessByObjectIDField;

	private AccessByObjectType accessByObjectTypeField;

	private int accessByPermissionMaskField;

	[DataMember(Name = "AD_23", EmitDefaultValue = false, IsRequired = false)]
	private string applicationAreaField;

	private RunPageMode runPageModeField;

	private int runObjectSrcTableField;

	private ViewMode viewModeField;

	private PushActionType pushActionField;

	private MenuActionType menuActionTypeField;

	private bool promotedField;

	private PromotedCategoryType promotedCategoryField;

	[DataMember(Name = "AD_31", EmitDefaultValue = false, IsRequired = false)]
	private string promotedCategoryUserDefinedCaptionField;

	private bool promotedIsBigField;

	private bool promotedOnlyField;

	private ScopeType scopeField;

	private DepartmentCategoryType departmentCategoryField;

	private bool inFooterBarField;

	private bool showCaptionField;

	[DataMember(Name = "AD_38", EmitDefaultValue = false, IsRequired = false)]
	private string defaultNameField;

	private GestureType gestureField;

	public const string ImageNameReport = "Report";

	public const string ImageNameNew = "NewSparkle";

	public const string ImageNameProcess = "Process";

	public const string ImageNameDefault = "Action";

	private const int EllipsisLength = 3;

	private const char Dot = '.';

	[DataMember(Name = "AD_1", EmitDefaultValue = false, IsRequired = false)]
	public LinkDefinition RunFormLink
	{
		get
		{
			return runFormLinkField;
		}
		set
		{
			runFormLinkField = value;
		}
	}

	[DataMember(Name = "AD_2", EmitDefaultValue = false, IsRequired = false)]
	public ViewDefinition RunFormView
	{
		get
		{
			return runFormViewField;
		}
		set
		{
			runFormViewField = value;
		}
	}

	[DataMember(Name = "AD_3", EmitDefaultValue = false, IsRequired = false)]
	public ActionTriggerDefinition Trigger
	{
		get
		{
			return triggerField;
		}
		set
		{
			triggerField = value;
		}
	}

	[DataMember(Name = "AD_4", EmitDefaultValue = false, IsRequired = false)]
	public PersonalizedFilterControlDefinition PersonalizedFilter
	{
		get
		{
			return personalizedFilterField;
		}
		set
		{
			personalizedFilterField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "AD_5", EmitDefaultValue = false, IsRequired = false)]
	public bool RunPageOnRec
	{
		get
		{
			return runPageOnRecField;
		}
		set
		{
			runPageOnRecField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "AD_6", EmitDefaultValue = false, IsRequired = false)]
	public bool Ellipsis
	{
		get
		{
			return ellipsisField;
		}
		set
		{
			ellipsisField = value;
		}
	}

	[XmlAttribute]
	public string ShortCutKey
	{
		get
		{
			return shortCutKeyField;
		}
		set
		{
			shortCutKeyField = value;
		}
	}

	[XmlAttribute]
	public string ShortCutKeyML
	{
		get
		{
			return shortCutKeyMLField;
		}
		set
		{
			shortCutKeyMLField = value;
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
	[DefaultValue("")]
	public string AdditionalSearchTerms
	{
		get
		{
			if (additionalSearchTermsField == null)
			{
				return "";
			}
			return additionalSearchTermsField;
		}
		set
		{
			additionalSearchTermsField = value;
		}
	}

	[XmlAttribute]
	public string AdditionalSearchTermsML
	{
		get
		{
			return additionalSearchTermsMLField;
		}
		set
		{
			additionalSearchTermsMLField = value;
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
	public string ShortCutKeyResourceIdentifier
	{
		get
		{
			return shortCutKeyResourceIdentifierField;
		}
		set
		{
			shortCutKeyResourceIdentifierField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "AD_15", EmitDefaultValue = false, IsRequired = false)]
	public int TargetID
	{
		get
		{
			return targetIDField;
		}
		set
		{
			targetIDField = value;
			TargetIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool TargetIDSpecified
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
	[DataMember(Name = "AD_16", EmitDefaultValue = false, IsRequired = false)]
	public RunObjectType RunObjectType
	{
		get
		{
			return runObjectTypeField;
		}
		set
		{
			runObjectTypeField = value;
			RunObjectTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool RunObjectTypeSpecified
	{
		get
		{
			return GetSpecified(11);
		}
		set
		{
			SetSpecified(11, value);
		}
	}

	[XmlAttribute]
	public string RunObjectView
	{
		get
		{
			return runObjectViewField;
		}
		set
		{
			runObjectViewField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "AD_18", EmitDefaultValue = false, IsRequired = false)]
	public bool IsViewApplied
	{
		get
		{
			return isViewAppliedField;
		}
		set
		{
			isViewAppliedField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "AD_19", EmitDefaultValue = false, IsRequired = false)]
	public bool IsViewInUserScope
	{
		get
		{
			return isViewInUserScopeField;
		}
		set
		{
			isViewInUserScopeField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "AD_20", EmitDefaultValue = false, IsRequired = false)]
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
			return GetSpecified(12);
		}
		set
		{
			SetSpecified(12, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "AD_21", EmitDefaultValue = false, IsRequired = false)]
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
			return GetSpecified(13);
		}
		set
		{
			SetSpecified(13, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "AD_22", EmitDefaultValue = false, IsRequired = false)]
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
			return GetSpecified(14);
		}
		set
		{
			SetSpecified(14, value);
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
	[DataMember(Name = "AD_24", EmitDefaultValue = false, IsRequired = false)]
	public RunPageMode RunPageMode
	{
		get
		{
			return runPageModeField;
		}
		set
		{
			runPageModeField = value;
			RunPageModeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool RunPageModeSpecified
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
	[DataMember(Name = "AD_25", EmitDefaultValue = false, IsRequired = false)]
	public int RunObjectSrcTable
	{
		get
		{
			return runObjectSrcTableField;
		}
		set
		{
			runObjectSrcTableField = value;
			RunObjectSrcTableSpecified = true;
		}
	}

	[XmlIgnore]
	public bool RunObjectSrcTableSpecified
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
	[DataMember(Name = "AD_26", EmitDefaultValue = false, IsRequired = false)]
	public ViewMode ViewMode
	{
		get
		{
			return viewModeField;
		}
		set
		{
			viewModeField = value;
			ViewModeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ViewModeSpecified
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
	[DataMember(Name = "AD_27", EmitDefaultValue = false, IsRequired = false)]
	public PushActionType PushAction
	{
		get
		{
			return pushActionField;
		}
		set
		{
			pushActionField = value;
			PushActionSpecified = true;
		}
	}

	[XmlIgnore]
	public bool PushActionSpecified
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
	[DataMember(Name = "AD_28", EmitDefaultValue = false, IsRequired = false)]
	public MenuActionType MenuActionType
	{
		get
		{
			return menuActionTypeField;
		}
		set
		{
			menuActionTypeField = value;
			MenuActionTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool MenuActionTypeSpecified
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
	[DefaultValue(false)]
	[DataMember(Name = "AD_29", EmitDefaultValue = false, IsRequired = false)]
	public bool Promoted
	{
		get
		{
			return promotedField;
		}
		set
		{
			promotedField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "AD_30", EmitDefaultValue = false, IsRequired = false)]
	public PromotedCategoryType PromotedCategory
	{
		get
		{
			return promotedCategoryField;
		}
		set
		{
			promotedCategoryField = value;
			PromotedCategorySpecified = true;
		}
	}

	[XmlIgnore]
	public bool PromotedCategorySpecified
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
	[DefaultValue("")]
	public string PromotedCategoryUserDefinedCaption
	{
		get
		{
			if (promotedCategoryUserDefinedCaptionField == null)
			{
				return "";
			}
			return promotedCategoryUserDefinedCaptionField;
		}
		set
		{
			promotedCategoryUserDefinedCaptionField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "AD_32", EmitDefaultValue = false, IsRequired = false)]
	public bool PromotedIsBig
	{
		get
		{
			return promotedIsBigField;
		}
		set
		{
			promotedIsBigField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "AD_33", EmitDefaultValue = false, IsRequired = false)]
	public bool PromotedOnly
	{
		get
		{
			return promotedOnlyField;
		}
		set
		{
			promotedOnlyField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "AD_34", EmitDefaultValue = false, IsRequired = false)]
	public ScopeType Scope
	{
		get
		{
			return scopeField;
		}
		set
		{
			scopeField = value;
			ScopeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ScopeSpecified
	{
		get
		{
			return GetSpecified(21);
		}
		set
		{
			SetSpecified(21, value);
		}
	}

	[XmlAttribute]
	[DefaultValue(DepartmentCategoryType.None)]
	[DataMember(Name = "AD_35", EmitDefaultValue = false, IsRequired = false)]
	public DepartmentCategoryType DepartmentCategory
	{
		get
		{
			return departmentCategoryField;
		}
		set
		{
			departmentCategoryField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "AD_36", EmitDefaultValue = false, IsRequired = false)]
	public bool InFooterBar
	{
		get
		{
			return inFooterBarField;
		}
		set
		{
			inFooterBarField = value;
			InFooterBarSpecified = true;
		}
	}

	[XmlIgnore]
	public bool InFooterBarSpecified
	{
		get
		{
			return GetSpecified(22);
		}
		set
		{
			SetSpecified(22, value);
		}
	}

	[XmlAttribute]
	[DefaultValue(true)]
	[DataMember(Name = "AD_37", EmitDefaultValue = false, IsRequired = false)]
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
	public string DefaultName
	{
		get
		{
			return defaultNameField;
		}
		set
		{
			defaultNameField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "AD_39", EmitDefaultValue = false, IsRequired = false)]
	public GestureType Gesture
	{
		get
		{
			return gestureField;
		}
		set
		{
			gestureField = value;
			GestureSpecified = true;
		}
	}

	[XmlIgnore]
	public bool GestureSpecified
	{
		get
		{
			return GetSpecified(23);
		}
		set
		{
			SetSpecified(23, value);
		}
	}

	protected override int MaxSpecifiedBits => 24;

	public ActionDefinition()
	{
		runPageOnRecField = false;
		ellipsisField = false;
		isViewAppliedField = false;
		isViewInUserScopeField = false;
		promotedField = false;
		promotedIsBigField = false;
		promotedOnlyField = false;
		departmentCategoryField = DepartmentCategoryType.None;
		showCaptionField = true;
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (!string.IsNullOrEmpty(shortCutKeyMLField))
		{
			shortCutKeyField = MultiLanguage.GetLanguageValue(language, primaryLanguage, shortCutKeyMLField);
		}
		if (!string.IsNullOrEmpty(toolTipMLField))
		{
			toolTipField = MultiLanguage.GetLanguageValue(language, primaryLanguage, toolTipMLField);
		}
		if (!string.IsNullOrEmpty(additionalSearchTermsMLField))
		{
			additionalSearchTermsField = MultiLanguage.GetLanguageValue(language, primaryLanguage, additionalSearchTermsMLField);
		}
		if (runFormLinkField != null)
		{
			runFormLinkField.SetLanguage(language, primaryLanguage);
		}
		if (runFormViewField != null)
		{
			runFormViewField.SetLanguage(language, primaryLanguage);
		}
		if (triggerField != null)
		{
			triggerField.SetLanguage(language, primaryLanguage);
		}
		if (personalizedFilterField != null)
		{
			personalizedFilterField.SetLanguage(language, primaryLanguage);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		shortCutKeyMLField = MetaPage.MultiLanguageTextMerge(shortCutKeyMLField, objType, id, ComponentType(), ComponentId(), 3, languageAppGroupId);
		toolTipMLField = MetaPage.MultiLanguageTextMerge(toolTipMLField, objType, id, ComponentType(), ComponentId(), 4, languageAppGroupId);
		additionalSearchTermsMLField = MetaPage.MultiLanguageTextMerge(additionalSearchTermsMLField, objType, id, ComponentType(), ComponentId(), 7, languageAppGroupId);
		if (runFormLinkField != null)
		{
			runFormLinkField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
		if (runFormViewField != null)
		{
			runFormViewField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
		if (triggerField != null)
		{
			triggerField.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
		if (personalizedFilterField != null)
		{
			personalizedFilterField.MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (runFormLinkField != null)
		{
			elementDefinition = runFormLinkField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (runFormViewField != null)
		{
			elementDefinition = runFormViewField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (triggerField != null)
		{
			elementDefinition = triggerField.Find(match);
			if (elementDefinition != null)
			{
				return elementDefinition;
			}
		}
		if (personalizedFilterField != null)
		{
			return personalizedFilterField.Find(match);
		}
		return elementDefinition;
	}

	public override void FindAllCore(List<ElementDefinition> result, Predicate<ElementDefinition> match)
	{
		base.FindAllCore(result, match);
		if (runFormLinkField != null)
		{
			runFormLinkField.FindAllCore(result, match);
		}
		if (runFormViewField != null)
		{
			runFormViewField.FindAllCore(result, match);
		}
		if (triggerField != null)
		{
			triggerField.FindAllCore(result, match);
		}
		if (personalizedFilterField != null)
		{
			personalizedFilterField.FindAllCore(result, match);
		}
	}

	public override object Clone()
	{
		ActionDefinition actionDefinition = (ActionDefinition)base.Clone();
		if (runFormLinkField != null)
		{
			actionDefinition.RunFormLink = (LinkDefinition)runFormLinkField.Clone();
		}
		if (runFormViewField != null)
		{
			actionDefinition.RunFormView = (ViewDefinition)runFormViewField.Clone();
		}
		if (triggerField != null)
		{
			actionDefinition.Trigger = (ActionTriggerDefinition)triggerField.Clone();
		}
		if (personalizedFilterField != null)
		{
			actionDefinition.PersonalizedFilter = (PersonalizedFilterControlDefinition)personalizedFilterField.Clone();
		}
		return actionDefinition;
	}

	public override void SolveProperties(MetaField field, MetaTable metaTable, MasterPage masterPage)
	{
		SolveName(field);
		base.SolveProperties(field, metaTable, masterPage);
	}

	public static string AddEllipsisToCaption(string caption)
	{
		if (string.IsNullOrEmpty(caption))
		{
			return new string('.', 3);
		}
		int num = NumberOfDottsAndTheEnd(caption);
		return caption.PadRight(caption.Length + 3 - num, '.');
	}

	private static int NumberOfDottsAndTheEnd(string caption)
	{
		int num = 0;
		int num2 = caption.Length - 1;
		while (num2 >= 0 && caption[num2] == '.')
		{
			num++;
			num2--;
		}
		return num;
	}

	public static string RemoveEllipsisFromCaption(string caption)
	{
		if (string.IsNullOrEmpty(caption))
		{
			return string.Empty;
		}
		if (caption.EndsWith(new string('.', 3)))
		{
			return caption.Remove(caption.Length - 3);
		}
		return caption;
	}
}
