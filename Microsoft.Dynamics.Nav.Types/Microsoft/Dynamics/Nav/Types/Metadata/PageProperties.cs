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
[DataContract(Name = "PP", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class PageProperties : ElementDefinition
{
	private SourceObjectDefinition sourceObjectField;

	[DataMember(Name = "PP_2", EmitDefaultValue = false, IsRequired = false)]
	private string additionalSearchTermsMLField;

	private PageType pageTypeField;

	[DataMember(Name = "PP_4", EmitDefaultValue = false, IsRequired = false)]
	private string viewNameField;

	private bool isSystemPageField;

	private bool onAfterGetCurrentRecordEnabledField;

	private bool editableField;

	private int cardFormIDField;

	[DataMember(Name = "PP_9", EmitDefaultValue = false, IsRequired = false)]
	private string dataCaptionExprField;

	private bool refreshOnActivateField;

	private int reportIDField;

	private int xMLportIDField;

	[DataMember(Name = "PP_13", EmitDefaultValue = false, IsRequired = false)]
	private string controlGUIDField;

	[DataMember(Name = "PP_14", EmitDefaultValue = false, IsRequired = false)]
	private string instructionalTextField;

	[DataMember(Name = "PP_15", EmitDefaultValue = false, IsRequired = false)]
	private string instructionalTextMLField;

	[DataMember(Name = "PP_16", EmitDefaultValue = false, IsRequired = false)]
	private string promotedActionCategoriesOrderField;

	[DataMember(Name = "PP_17", EmitDefaultValue = false, IsRequired = false)]
	private string promotedActionCategoriesField;

	[DataMember(Name = "PP_18", EmitDefaultValue = false, IsRequired = false)]
	private string promotedActionCategoriesMLField;

	[DataMember(Name = "PP_19", EmitDefaultValue = false, IsRequired = false)]
	private string helpRelativeUrlField;

	[DataMember(Name = "PP_20", EmitDefaultValue = false, IsRequired = false)]
	private string helpLinkField;

	[DataMember(Name = "PP_21", EmitDefaultValue = false, IsRequired = false)]
	private string supportedLocalesField;

	[DataMember(Name = "PP_22", EmitDefaultValue = false, IsRequired = false)]
	private string helpBaseLinkField;

	private CustomizationType configurationTypeAppliedField;

	private CustomizationType personalizationTypeAppliedField;

	private NavAppExtensionType sourceExtensionTypeField;

	[DataMember(Name = "PP_26", EmitDefaultValue = false, IsRequired = false)]
	private string aPIPublisherField;

	[DataMember(Name = "PP_27", EmitDefaultValue = false, IsRequired = false)]
	private string aPIGroupField;

	[DataMember(Name = "PP_28", EmitDefaultValue = false, IsRequired = false)]
	private string aPIVersionField;

	[DataMember(Name = "PP_29", EmitDefaultValue = false, IsRequired = false)]
	private string entitySetNameField;

	[DataMember(Name = "PP_30", EmitDefaultValue = false, IsRequired = false)]
	private string entityNameField;

	private bool changeTrackingAllowedField;

	[DataMember(Name = "PP_32", EmitDefaultValue = false, IsRequired = false)]
	private string applicationAreaField;

	private int accessByObjectIDField;

	private AccessByObjectType accessByObjectTypeField;

	private int accessByPermissionMaskField;

	private UsageCategory usageCategoryField;

	[DataMember(Name = "PP_37", EmitDefaultValue = false, IsRequired = false)]
	private string queryCategoryField;

	[XmlAttribute]
	[DataMember(Name = "PP_PCA", EmitDefaultValue = false, IsRequired = false)]
	public DesignerPersonalizationTypes PersonalizationChangesApplied { get; set; }

	[DataMember(Name = "PP_1", EmitDefaultValue = false, IsRequired = false)]
	public SourceObjectDefinition SourceObject
	{
		get
		{
			return sourceObjectField;
		}
		set
		{
			sourceObjectField = value;
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
	[DataMember(Name = "PP_3", EmitDefaultValue = false, IsRequired = false)]
	public PageType PageType
	{
		get
		{
			return pageTypeField;
		}
		set
		{
			pageTypeField = value;
			PageTypeSpecified = true;
		}
	}

	[XmlIgnore]
	public bool PageTypeSpecified
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
	public string ViewName
	{
		get
		{
			return viewNameField;
		}
		set
		{
			viewNameField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "PP_5", EmitDefaultValue = false, IsRequired = false)]
	public bool IsSystemPage
	{
		get
		{
			return isSystemPageField;
		}
		set
		{
			isSystemPageField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "PP_6", EmitDefaultValue = false, IsRequired = false)]
	public bool OnAfterGetCurrentRecordEnabled
	{
		get
		{
			return onAfterGetCurrentRecordEnabledField;
		}
		set
		{
			onAfterGetCurrentRecordEnabledField = value;
			OnAfterGetCurrentRecordEnabledSpecified = true;
		}
	}

	[XmlIgnore]
	public bool OnAfterGetCurrentRecordEnabledSpecified
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
	[DefaultValue(true)]
	[DataMember(Name = "PP_7", EmitDefaultValue = false, IsRequired = false)]
	public bool Editable
	{
		get
		{
			return editableField;
		}
		set
		{
			editableField = value;
		}
	}

	[XmlAttribute]
	[DataMember(Name = "PP_8", EmitDefaultValue = false, IsRequired = false)]
	public int CardFormID
	{
		get
		{
			return cardFormIDField;
		}
		set
		{
			cardFormIDField = value;
			CardFormIDSpecified = true;
		}
	}

	[XmlIgnore]
	public bool CardFormIDSpecified
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
	public string DataCaptionExpr
	{
		get
		{
			return dataCaptionExprField;
		}
		set
		{
			dataCaptionExprField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "PP_10", EmitDefaultValue = false, IsRequired = false)]
	public bool RefreshOnActivate
	{
		get
		{
			return refreshOnActivateField;
		}
		set
		{
			refreshOnActivateField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(0)]
	[DataMember(Name = "PP_11", EmitDefaultValue = false, IsRequired = false)]
	public int ReportID
	{
		get
		{
			return reportIDField;
		}
		set
		{
			reportIDField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue(0)]
	[DataMember(Name = "PP_12", EmitDefaultValue = false, IsRequired = false)]
	public int XMLportID
	{
		get
		{
			return xMLportIDField;
		}
		set
		{
			xMLportIDField = value;
		}
	}

	[XmlAttribute]
	public string ControlGUID
	{
		get
		{
			return controlGUIDField;
		}
		set
		{
			controlGUIDField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string InstructionalText
	{
		get
		{
			if (instructionalTextField == null)
			{
				return "";
			}
			return instructionalTextField;
		}
		set
		{
			instructionalTextField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string InstructionalTextML
	{
		get
		{
			if (instructionalTextMLField == null)
			{
				return "";
			}
			return instructionalTextMLField;
		}
		set
		{
			instructionalTextMLField = value;
		}
	}

	[XmlAttribute]
	public string PromotedActionCategoriesOrder
	{
		get
		{
			return promotedActionCategoriesOrderField;
		}
		set
		{
			promotedActionCategoriesOrderField = value;
		}
	}

	[XmlAttribute]
	public string PromotedActionCategories
	{
		get
		{
			return promotedActionCategoriesField;
		}
		set
		{
			promotedActionCategoriesField = value;
		}
	}

	[XmlAttribute]
	public string PromotedActionCategoriesML
	{
		get
		{
			return promotedActionCategoriesMLField;
		}
		set
		{
			promotedActionCategoriesMLField = value;
		}
	}

	[XmlAttribute]
	public string HelpRelativeUrl
	{
		get
		{
			return helpRelativeUrlField;
		}
		set
		{
			helpRelativeUrlField = value;
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
	[DataMember(Name = "PP_23", EmitDefaultValue = false, IsRequired = false)]
	public CustomizationType ConfigurationTypeApplied
	{
		get
		{
			return configurationTypeAppliedField;
		}
		set
		{
			configurationTypeAppliedField = value;
			ConfigurationTypeAppliedSpecified = true;
		}
	}

	[XmlIgnore]
	public bool ConfigurationTypeAppliedSpecified
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
	[DataMember(Name = "PP_24", EmitDefaultValue = false, IsRequired = false)]
	public CustomizationType PersonalizationTypeApplied
	{
		get
		{
			return personalizationTypeAppliedField;
		}
		set
		{
			personalizationTypeAppliedField = value;
			PersonalizationTypeAppliedSpecified = true;
		}
	}

	[XmlIgnore]
	public bool PersonalizationTypeAppliedSpecified
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
	[DataMember(Name = "PP_25", EmitDefaultValue = false, IsRequired = false)]
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
			return GetSpecified(5);
		}
		set
		{
			SetSpecified(5, value);
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string APIPublisher
	{
		get
		{
			if (aPIPublisherField == null)
			{
				return "";
			}
			return aPIPublisherField;
		}
		set
		{
			aPIPublisherField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("")]
	public string APIGroup
	{
		get
		{
			if (aPIGroupField == null)
			{
				return "";
			}
			return aPIGroupField;
		}
		set
		{
			aPIGroupField = value;
		}
	}

	[XmlAttribute]
	[DefaultValue("beta")]
	public string APIVersion
	{
		get
		{
			if (aPIVersionField == null)
			{
				return "beta";
			}
			return aPIVersionField;
		}
		set
		{
			aPIVersionField = value;
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

	[XmlAttribute]
	[DefaultValue(false)]
	[DataMember(Name = "PP_31", EmitDefaultValue = false, IsRequired = false)]
	public bool ChangeTrackingAllowed
	{
		get
		{
			return changeTrackingAllowedField;
		}
		set
		{
			changeTrackingAllowedField = value;
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
	[DataMember(Name = "PP_33", EmitDefaultValue = false, IsRequired = false)]
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
			return GetSpecified(6);
		}
		set
		{
			SetSpecified(6, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "PP_34", EmitDefaultValue = false, IsRequired = false)]
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
			return GetSpecified(7);
		}
		set
		{
			SetSpecified(7, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "PP_35", EmitDefaultValue = false, IsRequired = false)]
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
			return GetSpecified(8);
		}
		set
		{
			SetSpecified(8, value);
		}
	}

	[XmlAttribute]
	[DataMember(Name = "PP_36", EmitDefaultValue = false, IsRequired = false)]
	public UsageCategory UsageCategory
	{
		get
		{
			return usageCategoryField;
		}
		set
		{
			usageCategoryField = value;
			UsageCategorySpecified = true;
		}
	}

	[XmlIgnore]
	public bool UsageCategorySpecified
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
	[DefaultValue("")]
	public string QueryCategory
	{
		get
		{
			if (queryCategoryField == null)
			{
				return "";
			}
			return queryCategoryField;
		}
		set
		{
			queryCategoryField = value;
		}
	}

	protected override int MaxSpecifiedBits => 10;

	public bool IsViewSupported()
	{
		if (PageType == PageType.List)
		{
			return true;
		}
		return false;
	}

	public PageProperties()
	{
		isSystemPageField = false;
		editableField = true;
		refreshOnActivateField = false;
		reportIDField = 0;
		xMLportIDField = 0;
		changeTrackingAllowedField = false;
	}

	public override void SetLanguage(string language, string primaryLanguage)
	{
		base.SetLanguage(language, primaryLanguage);
		if (!string.IsNullOrEmpty(instructionalTextMLField))
		{
			instructionalTextField = MultiLanguage.GetLanguageValue(language, primaryLanguage, instructionalTextMLField);
		}
		if (!string.IsNullOrEmpty(promotedActionCategoriesMLField))
		{
			promotedActionCategoriesField = MultiLanguage.GetLanguageValue(language, primaryLanguage, promotedActionCategoriesMLField);
		}
		if (sourceObjectField != null)
		{
			sourceObjectField.SetLanguage(language, primaryLanguage);
		}
	}

	public override void MergeMultiLanguage(ObjectType objType, int id, int languageAppGroupId)
	{
		base.MergeMultiLanguage(objType, id, languageAppGroupId);
		additionalSearchTermsMLField = MetaPage.MultiLanguageTextMerge(additionalSearchTermsMLField, objType, id, ComponentType(), ComponentId(), 7, languageAppGroupId);
		instructionalTextMLField = MetaPage.MultiLanguageTextMerge(instructionalTextMLField, objType, id, ComponentType(), ComponentId(), 1, languageAppGroupId);
		promotedActionCategoriesMLField = MetaPage.MultiLanguageTextMerge(promotedActionCategoriesMLField, objType, id, ComponentType(), ComponentId(), 5, languageAppGroupId);
		if (sourceObjectField != null)
		{
			sourceObjectField.MergeMultiLanguage(objType, id, languageAppGroupId);
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
		if (sourceObjectField != null)
		{
			return sourceObjectField.Find(match);
		}
		return elementDefinition;
	}

	public override void FindAllCore(List<ElementDefinition> result, Predicate<ElementDefinition> match)
	{
		base.FindAllCore(result, match);
		if (sourceObjectField != null)
		{
			sourceObjectField.FindAllCore(result, match);
		}
	}

	public override object Clone()
	{
		PageProperties pageProperties = (PageProperties)base.Clone();
		if (sourceObjectField != null)
		{
			pageProperties.SourceObject = (SourceObjectDefinition)sourceObjectField.Clone();
		}
		return pageProperties;
	}
}
