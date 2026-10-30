using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class CodeAnalysisResources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Microsoft.Dynamics.Nav.CodeAnalysis.CodeAnalysisResources", typeof(CodeAnalysisResources).GetTypeInfo().Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string AnalyzerDriverFailure => ResourceManager.GetString("AnalyzerDriverFailure", resourceCulture);

	internal static string AnalyzerDriverThrows => ResourceManager.GetString("AnalyzerDriverThrows", resourceCulture);

	internal static string AnalyzerDriverThrowsDescription => ResourceManager.GetString("AnalyzerDriverThrowsDescription", resourceCulture);

	internal static string AnalyzerExecutionTimeColumnHeader => ResourceManager.GetString("AnalyzerExecutionTimeColumnHeader", resourceCulture);

	internal static string AnalyzerNameColumnHeader => ResourceManager.GetString("AnalyzerNameColumnHeader", resourceCulture);

	internal static string AnalyzerTotalExecutionTime => ResourceManager.GetString("AnalyzerTotalExecutionTime", resourceCulture);

	internal static string ArgumentCannotBeEmpty => ResourceManager.GetString("ArgumentCannotBeEmpty", resourceCulture);

	internal static string ArgumentElementCannotBeNull => ResourceManager.GetString("ArgumentElementCannotBeNull", resourceCulture);

	internal static string ChangesMustBeOrderedAndNotOverlapping => ResourceManager.GetString("ChangesMustBeOrderedAndNotOverlapping", resourceCulture);

	internal static string CompilationEnded => ResourceManager.GetString("CompilationEnded", resourceCulture);

	internal static string CompilationStarted => ResourceManager.GetString("CompilationStarted", resourceCulture);

	internal static string CompilerAnalyzerFailure => ResourceManager.GetString("CompilerAnalyzerFailure", resourceCulture);

	internal static string CompilerAnalyzerThrows => ResourceManager.GetString("CompilerAnalyzerThrows", resourceCulture);

	internal static string CompilerAnalyzerThrowsDescription => ResourceManager.GetString("CompilerAnalyzerThrowsDescription", resourceCulture);

	internal static string ControlAddInManifestWrongValue => ResourceManager.GetString("ControlAddInManifestWrongValue", resourceCulture);

	internal static string ControlAddInReferences => ResourceManager.GetString("ControlAddInReferences", resourceCulture);

	internal static string DataNotApp => ResourceManager.GetString("DataNotApp", resourceCulture);

	internal static string DependencyPartIsMissing => ResourceManager.GetString("DependencyPartIsMissing", resourceCulture);

	internal static string DiagnosticIdCantBeNullOrWhitespace => ResourceManager.GetString("DiagnosticIdCantBeNullOrWhitespace", resourceCulture);

	internal static string DuplicateAnalyzerInstances => ResourceManager.GetString("DuplicateAnalyzerInstances", resourceCulture);

	internal static string ElementIsExpected => ResourceManager.GetString("ElementIsExpected", resourceCulture);

	internal static string EndMustNotBeLessThanStart => ResourceManager.GetString("EndMustNotBeLessThanStart", resourceCulture);

	internal static string ERR_AccessibilityModifierNotAllowed => ResourceManager.GetString("ERR_AccessibilityModifierNotAllowed", resourceCulture);

	internal static string ERR_ActionAndGroupInSectionsAreaOnSameLevel => ResourceManager.GetString("ERR_ActionAndGroupInSectionsAreaOnSameLevel", resourceCulture);

	internal static string ERR_ActionNotFound => ResourceManager.GetString("ERR_ActionNotFound", resourceCulture);

	internal static string ERR_ActionsAreNotAllowedOnThisControlType => ResourceManager.GetString("ERR_ActionsAreNotAllowedOnThisControlType", resourceCulture);

	internal static string ERR_AddChangeNotSupportedForViews => ResourceManager.GetString("ERR_AddChangeNotSupportedForViews", resourceCulture);

	internal static string ERR_AggregationMethodsIncompatibleWithQueryColumnType => ResourceManager.GetString("ERR_AggregationMethodsIncompatibleWithQueryColumnType", resourceCulture);

	internal static string ERR_AmbigBinaryOps => ResourceManager.GetString("ERR_AmbigBinaryOps", resourceCulture);

	internal static string ERR_AmbigCall => ResourceManager.GetString("ERR_AmbigCall", resourceCulture);

	internal static string ERR_AmbigMemberReference => ResourceManager.GetString("ERR_AmbigMemberReference", resourceCulture);

	internal static string ERR_AmbigReference => ResourceManager.GetString("ERR_AmbigReference", resourceCulture);

	internal static string ERR_AmbigSymbolReference => ResourceManager.GetString("ERR_AmbigSymbolReference", resourceCulture);

	internal static string ERR_AmbigUnaryOp => ResourceManager.GetString("ERR_AmbigUnaryOp", resourceCulture);

	internal static string ERR_AmbiguosOptionAccess => ResourceManager.GetString("ERR_AmbiguosOptionAccess", resourceCulture);

	internal static string ERR_AnchorCannotBeArea => ResourceManager.GetString("ERR_AnchorCannotBeArea", resourceCulture);

	internal static string ERR_AnchorMustBeGroupType => ResourceManager.GetString("ERR_AnchorMustBeGroupType", resourceCulture);

	internal static string ERR_ApiPageAndSubpagePropertyMismatch => ResourceManager.GetString("ERR_ApiPageAndSubpagePropertyMismatch", resourceCulture);

	internal static string ERR_ApiPageCouldOnlyBePartOfAnotherApiPage => ResourceManager.GetString("ERR_ApiPageCouldOnlyBePartOfAnotherApiPage", resourceCulture);

	internal static string ERR_ApiPageFieldsShouldHaveAlphaNumericName => ResourceManager.GetString("ERR_ApiPageFieldsShouldHaveAlphaNumericName", resourceCulture);

	internal static string ERR_ApiPagePartAndSubpagePropertyMismatch => ResourceManager.GetString("ERR_ApiPagePartAndSubpagePropertyMismatch", resourceCulture);

	internal static string ERR_ApiPagePartMustBePartOrApi => ResourceManager.GetString("ERR_ApiPagePartMustBePartOrApi", resourceCulture);

	internal static string ERR_ApiPageShouldHaveDelayedInsert => ResourceManager.GetString("ERR_ApiPageShouldHaveDelayedInsert", resourceCulture);

	internal static string ERR_ApiQueryColumnsShouldHaveAlphaNumericName => ResourceManager.GetString("ERR_ApiQueryColumnsShouldHaveAlphaNumericName", resourceCulture);

	internal static string ERR_AppFileDoesNotExist => ResourceManager.GetString("ERR_AppFileDoesNotExist", resourceCulture);

	internal static string ERR_ApplicationIdRangesAreOverlapping => ResourceManager.GetString("ERR_ApplicationIdRangesAreOverlapping", resourceCulture);

	internal static string ERR_ApplicationObjectIdentifierOutOfBounds => ResourceManager.GetString("ERR_ApplicationObjectIdentifierOutOfBounds", resourceCulture);

	internal static string ERR_AppObjDuplicateId => ResourceManager.GetString("ERR_AppObjDuplicateId", resourceCulture);

	internal static string ERR_AppObjDuplicateName => ResourceManager.GetString("ERR_AppObjDuplicateName", resourceCulture);

	internal static string ERR_AreaNotValidOnPageOfType => ResourceManager.GetString("ERR_AreaNotValidOnPageOfType", resourceCulture);

	internal static string ERR_AreaOnlyValidOnPageOfType => ResourceManager.GetString("ERR_AreaOnlyValidOnPageOfType", resourceCulture);

	internal static string ERR_ArrayDimensionMustBePositiveNumber => ResourceManager.GetString("ERR_ArrayDimensionMustBePositiveNumber", resourceCulture);

	internal static string ERR_ArrayIsTooBig => ResourceManager.GetString("ERR_ArrayIsTooBig", resourceCulture);

	internal static string ERR_ArrayMustHaveAtLeastOneDimension => ResourceManager.GetString("ERR_ArrayMustHaveAtLeastOneDimension", resourceCulture);

	internal static string ERR_ArrayNotValidForAssign => ResourceManager.GetString("ERR_ArrayNotValidForAssign", resourceCulture);

	internal static string ERR_ArrayNotValidForCase => ResourceManager.GetString("ERR_ArrayNotValidForCase", resourceCulture);

	internal static string ERR_ArrayTooManyDimensions => ResourceManager.GetString("ERR_ArrayTooManyDimensions", resourceCulture);

	internal static string ERR_AssgLvalueExpected => ResourceManager.GetString("ERR_AssgLvalueExpected", resourceCulture);

	internal static string ERR_AtLeastOneDimension => ResourceManager.GetString("ERR_AtLeastOneDimension", resourceCulture);

	internal static string ERR_AttributeAllowedOnlyOn => ResourceManager.GetString("ERR_AttributeAllowedOnlyOn", resourceCulture);

	internal static string ERR_AttributeCannotBeUsedOnVariablesOfArrayType => ResourceManager.GetString("ERR_AttributeCannotBeUsedOnVariablesOfArrayType", resourceCulture);

	internal static string ERR_AttributeCanOnlyBeSpecifiedOnGlobalVariables => ResourceManager.GetString("ERR_AttributeCanOnlyBeSpecifiedOnGlobalVariables", resourceCulture);

	internal static string ERR_AttributeCanOnlyBeSpecifiedOnLocalVariables => ResourceManager.GetString("ERR_AttributeCanOnlyBeSpecifiedOnLocalVariables", resourceCulture);

	internal static string ERR_AttributeCanOnlyBeUsedOnProceduresWithAttribute => ResourceManager.GetString("ERR_AttributeCanOnlyBeUsedOnProceduresWithAttribute", resourceCulture);

	internal static string ERR_AttributeContext => ResourceManager.GetString("ERR_AttributeContext", resourceCulture);

	internal static string ERR_AttributeNotOnAValidSymbol => ResourceManager.GetString("ERR_AttributeNotOnAValidSymbol", resourceCulture);

	internal static string ERR_AttributeOnlyAllowedIn => ResourceManager.GetString("ERR_AttributeOnlyAllowedIn", resourceCulture);

	internal static string ERR_AttributesCannotHaveNestedElements => ResourceManager.GetString("ERR_AttributesCannotHaveNestedElements", resourceCulture);

	internal static string ERR_AttributesMustBeNestedInsideElements => ResourceManager.GetString("ERR_AttributesMustBeNestedInsideElements", resourceCulture);

	internal static string ERR_BadAccess => ResourceManager.GetString("ERR_BadAccess", resourceCulture);

	internal static string ERR_BadArgCount => ResourceManager.GetString("ERR_BadArgCount", resourceCulture);

	internal static string ERR_BadArgMember => ResourceManager.GetString("ERR_BadArgMember", resourceCulture);

	internal static string ERR_BadArgType => ResourceManager.GetString("ERR_BadArgType", resourceCulture);

	internal static string ERR_BadArgTypeMustBeSameAsFirstArgument => ResourceManager.GetString("ERR_BadArgTypeMustBeSameAsFirstArgument", resourceCulture);

	internal static string ERR_BadAttributeArgCount => ResourceManager.GetString("ERR_BadAttributeArgCount", resourceCulture);

	internal static string ERR_BadAttributeCodeunitSubtype => ResourceManager.GetString("ERR_BadAttributeCodeunitSubtype", resourceCulture);

	internal static string ERR_BadBinaryOps => ResourceManager.GetString("ERR_BadBinaryOps", resourceCulture);

	internal static string ERR_BadIndexCount => ResourceManager.GetString("ERR_BadIndexCount", resourceCulture);

	internal static string ERR_BadIndexLHS => ResourceManager.GetString("ERR_BadIndexLHS", resourceCulture);

	internal static string ERR_BadSwitch => ResourceManager.GetString("ERR_BadSwitch", resourceCulture);

	internal static string ERR_BadUnaryOp => ResourceManager.GetString("ERR_BadUnaryOp", resourceCulture);

	internal static string ERR_BinaryFile => ResourceManager.GetString("ERR_BinaryFile", resourceCulture);

	internal static string ERR_CalcFormulaConversionError => ResourceManager.GetString("ERR_CalcFormulaConversionError", resourceCulture);

	internal static string ERR_CannotAddMultipleTimes => ResourceManager.GetString("ERR_CannotAddMultipleTimes", resourceCulture);

	internal static string ERR_CannotHaveAddSymbolsOnPageCustomization => ResourceManager.GetString("ERR_CannotHaveAddSymbolsOnPageCustomization", resourceCulture);

	internal static string ERR_CannotHaveMethodsOnPageCustomization => ResourceManager.GetString("ERR_CannotHaveMethodsOnPageCustomization", resourceCulture);

	internal static string ERR_CannotHaveMethodsOnPageViews => ResourceManager.GetString("ERR_CannotHaveMethodsOnPageViews", resourceCulture);

	internal static string ERR_CannotHaveVariablesOnPageCustomization => ResourceManager.GetString("ERR_CannotHaveVariablesOnPageCustomization", resourceCulture);

	internal static string ERR_CannotModifyMultipleTimes => ResourceManager.GetString("ERR_CannotModifyMultipleTimes", resourceCulture);

	internal static string ERR_CannotMoveMultipleTimes => ResourceManager.GetString("ERR_CannotMoveMultipleTimes", resourceCulture);

	internal static string ERR_CannotMoveOrModifyInTheSameExtensionYouAdded => ResourceManager.GetString("ERR_CannotMoveOrModifyInTheSameExtensionYouAdded", resourceCulture);

	internal static string ERR_CannotMoveRelativeToItself => ResourceManager.GetString("ERR_CannotMoveRelativeToItself", resourceCulture);

	internal static string ERR_CannotMoveSymbolsBetweenAreas => ResourceManager.GetString("ERR_CannotMoveSymbolsBetweenAreas", resourceCulture);

	internal static string ERR_CannotSpecifyLabelAndMultilanguageSameTime => ResourceManager.GetString("ERR_CannotSpecifyLabelAndMultilanguageSameTime", resourceCulture);

	internal static string ERR_CharAllowedFormatIsWrong => ResourceManager.GetString("ERR_CharAllowedFormatIsWrong", resourceCulture);

	internal static string ERR_CheckedOverflow => ResourceManager.GetString("ERR_CheckedOverflow", resourceCulture);

	internal static string ERR_ClientSideEventsAreSupportedOnlyOnPages => ResourceManager.GetString("ERR_ClientSideEventsAreSupportedOnlyOnPages", resourceCulture);

	internal static string ERR_ClusteredKeyCanAppearOnce => ResourceManager.GetString("ERR_ClusteredKeyCanAppearOnce", resourceCulture);

	internal static string ERR_ColumnNameFormatClash => ResourceManager.GetString("ERR_ColumnNameFormatClash", resourceCulture);

	internal static string ERR_CompileCanceled => ResourceManager.GetString("ERR_CompileCanceled", resourceCulture);

	internal static string ERR_ConstantStringTooLong => ResourceManager.GetString("ERR_ConstantStringTooLong", resourceCulture);

	internal static string ERR_ConstantValueNotAValidOptionValue => ResourceManager.GetString("ERR_ConstantValueNotAValidOptionValue", resourceCulture);

	internal static string ERR_ConstOutOfRange => ResourceManager.GetString("ERR_ConstOutOfRange", resourceCulture);

	internal static string ERR_ContextSentiveHelpRequiresManifestProperty => ResourceManager.GetString("ERR_ContextSentiveHelpRequiresManifestProperty", resourceCulture);

	internal static string ERR_ContextSentiveHelpWithPlaceholder => ResourceManager.GetString("ERR_ContextSentiveHelpWithPlaceholder", resourceCulture);

	internal static string ERR_ControlAddInNotFound => ResourceManager.GetString("ERR_ControlAddInNotFound", resourceCulture);

	internal static string ERR_ControlAddWrongLinkFormat => ResourceManager.GetString("ERR_ControlAddWrongLinkFormat", resourceCulture);

	internal static string ERR_ControlNotFound => ResourceManager.GetString("ERR_ControlNotFound", resourceCulture);

	internal static string ERR_ControlNotInGroup => ResourceManager.GetString("ERR_ControlNotInGroup", resourceCulture);

	internal static string ERR_ControlTypeNotAllowedInParent => ResourceManager.GetString("ERR_ControlTypeNotAllowedInParent", resourceCulture);

	internal static string ERR_CouldNotDetermineDefaultPrimaryKey => ResourceManager.GetString("ERR_CouldNotDetermineDefaultPrimaryKey", resourceCulture);

	internal static string ERR_CuegroupLayoutMustBeOnCuegroup => ResourceManager.GetString("ERR_CuegroupLayoutMustBeOnCuegroup", resourceCulture);

	internal static string ERR_DataItemLinkMustBeSetForNestedDataItems => ResourceManager.GetString("ERR_DataItemLinkMustBeSetForNestedDataItems", resourceCulture);

	internal static string ERR_DataItemLinkMustReferenceFieldOnAncestorDataItem => ResourceManager.GetString("ERR_DataItemLinkMustReferenceFieldOnAncestorDataItem", resourceCulture);

	internal static string ERR_DateFormulaNumberOutOfRange => ResourceManager.GetString("ERR_DateFormulaNumberOutOfRange", resourceCulture);

	internal static string ERR_DateFormulaShouldIncludeANumber => ResourceManager.GetString("ERR_DateFormulaShouldIncludeANumber", resourceCulture);

	internal static string ERR_DateFormulaShouldIncludeQuantor => ResourceManager.GetString("ERR_DateFormulaShouldIncludeQuantor", resourceCulture);

	internal static string ERR_DateFormulaSignMissing => ResourceManager.GetString("ERR_DateFormulaSignMissing", resourceCulture);

	internal static string ERR_DateFormulaValueTooLong => ResourceManager.GetString("ERR_DateFormulaValueTooLong", resourceCulture);

	internal static string ERR_DateMethodsIncompatibleWithQueryColumnType => ResourceManager.GetString("ERR_DateMethodsIncompatibleWithQueryColumnType", resourceCulture);

	internal static string ERR_DeletedObjectChangeFoundInCompilation => ResourceManager.GetString("ERR_DeletedObjectChangeFoundInCompilation", resourceCulture);

	internal static string ERR_DirectoryNotFound => ResourceManager.GetString("ERR_DirectoryNotFound", resourceCulture);

	internal static string ERR_DivByZero => ResourceManager.GetString("ERR_DivByZero", resourceCulture);

	internal static string ERR_DotNetAssemblyCouldNotBeFound => ResourceManager.GetString("ERR_DotNetAssemblyCouldNotBeFound", resourceCulture);

	internal static string ERR_DotNetTypeCouldNotBeFound => ResourceManager.GetString("ERR_DotNetTypeCouldNotBeFound", resourceCulture);

	internal static string ERR_DotNetTypeDeclarationDoesNotContainIsControlAddInProperty => ResourceManager.GetString("ERR_DotNetTypeDeclarationDoesNotContainIsControlAddInProperty", resourceCulture);

	internal static string ERR_DotNetTypeIsNotControlAddIn => ResourceManager.GetString("ERR_DotNetTypeIsNotControlAddIn", resourceCulture);

	internal static string ERR_DuplicateActionId => ResourceManager.GetString("ERR_DuplicateActionId", resourceCulture);

	internal static string ERR_DuplicateActionName => ResourceManager.GetString("ERR_DuplicateActionName", resourceCulture);

	internal static string ERR_DuplicateApiVersionId => ResourceManager.GetString("ERR_DuplicateApiVersionId", resourceCulture);

	internal static string ERR_DuplicateAreaKind => ResourceManager.GetString("ERR_DuplicateAreaKind", resourceCulture);

	internal static string ERR_DuplicateAttribute => ResourceManager.GetString("ERR_DuplicateAttribute", resourceCulture);

	internal static string ERR_DuplicateCaseStatement => ResourceManager.GetString("ERR_DuplicateCaseStatement", resourceCulture);

	internal static string ERR_DuplicateColumnOrLabelName => ResourceManager.GetString("ERR_DuplicateColumnOrLabelName", resourceCulture);

	internal static string ERR_DuplicateColumnReference => ResourceManager.GetString("ERR_DuplicateColumnReference", resourceCulture);

	internal static string ERR_DuplicateControlId => ResourceManager.GetString("ERR_DuplicateControlId", resourceCulture);

	internal static string ERR_DuplicateControlName => ResourceManager.GetString("ERR_DuplicateControlName", resourceCulture);

	internal static string ERR_DuplicatedKeyFields => ResourceManager.GetString("ERR_DuplicatedKeyFields", resourceCulture);

	internal static string ERR_DuplicateDotNetTypeAlias => ResourceManager.GetString("ERR_DuplicateDotNetTypeAlias", resourceCulture);

	internal static string ERR_DuplicateFieldGroupId => ResourceManager.GetString("ERR_DuplicateFieldGroupId", resourceCulture);

	internal static string ERR_DuplicateFieldGroupName => ResourceManager.GetString("ERR_DuplicateFieldGroupName", resourceCulture);

	internal static string ERR_DuplicateFieldId => ResourceManager.GetString("ERR_DuplicateFieldId", resourceCulture);

	internal static string ERR_DuplicateFieldName => ResourceManager.GetString("ERR_DuplicateFieldName", resourceCulture);

	internal static string ERR_DuplicateHandlerMethodName => ResourceManager.GetString("ERR_DuplicateHandlerMethodName", resourceCulture);

	internal static string ERR_DuplicateIdRange => ResourceManager.GetString("ERR_DuplicateIdRange", resourceCulture);

	internal static string ERR_DuplicateKeyId => ResourceManager.GetString("ERR_DuplicateKeyId", resourceCulture);

	internal static string ERR_DuplicateKeyName => ResourceManager.GetString("ERR_DuplicateKeyName", resourceCulture);

	internal static string ERR_DuplicateLabelProperty => ResourceManager.GetString("ERR_DuplicateLabelProperty", resourceCulture);

	internal static string ERR_DuplicateLanguageTranslation => ResourceManager.GetString("ERR_DuplicateLanguageTranslation", resourceCulture);

	internal static string ERR_DuplicateLegacyEnumId => ResourceManager.GetString("ERR_DuplicateLegacyEnumId", resourceCulture);

	internal static string ERR_DuplicateLegacyEnumName => ResourceManager.GetString("ERR_DuplicateLegacyEnumName", resourceCulture);

	internal static string ERR_DuplicateLegacyEnumValuesDiffer => ResourceManager.GetString("ERR_DuplicateLegacyEnumValuesDiffer", resourceCulture);

	internal static string ERR_DuplicateListMember => ResourceManager.GetString("ERR_DuplicateListMember", resourceCulture);

	internal static string ERR_DuplicateLocale => ResourceManager.GetString("ERR_DuplicateLocale", resourceCulture);

	internal static string ERR_DuplicateMemberId => ResourceManager.GetString("ERR_DuplicateMemberId", resourceCulture);

	internal static string ERR_DuplicateMemberIdFromHash => ResourceManager.GetString("ERR_DuplicateMemberIdFromHash", resourceCulture);

	internal static string ERR_DuplicateMemberName => ResourceManager.GetString("ERR_DuplicateMemberName", resourceCulture);

	internal static string ERR_DuplicateNamespacePrefix => ResourceManager.GetString("ERR_DuplicateNamespacePrefix", resourceCulture);

	internal static string ERR_DuplicateOption => ResourceManager.GetString("ERR_DuplicateOption", resourceCulture);

	internal static string ERR_DuplicateParamName => ResourceManager.GetString("ERR_DuplicateParamName", resourceCulture);

	internal static string ERR_DuplicateProperty => ResourceManager.GetString("ERR_DuplicateProperty", resourceCulture);

	internal static string ERR_DuplicateReturnValueName => ResourceManager.GetString("ERR_DuplicateReturnValueName", resourceCulture);

	internal static string ERR_DuplicateTableFieldReference => ResourceManager.GetString("ERR_DuplicateTableFieldReference", resourceCulture);

	internal static string ERR_DuplicateTrigger => ResourceManager.GetString("ERR_DuplicateTrigger", resourceCulture);

	internal static string ERR_DuplicateVariableName => ResourceManager.GetString("ERR_DuplicateVariableName", resourceCulture);

	internal static string ERR_ElseWithoutIf => ResourceManager.GetString("ERR_ElseWithoutIf", resourceCulture);

	internal static string ERR_EmbeddingAreaShouldOnlyRunListPages => ResourceManager.GetString("ERR_EmbeddingAreaShouldOnlyRunListPages", resourceCulture);

	internal static string ERR_EmptyConstIsNotSupported => ResourceManager.GetString("ERR_EmptyConstIsNotSupported", resourceCulture);

	internal static string ERR_EmptyListMember => ResourceManager.GetString("ERR_EmptyListMember", resourceCulture);

	internal static string ERR_EnumNotExtensible => ResourceManager.GetString("ERR_EnumNotExtensible", resourceCulture);

	internal static string ERR_EnumPropertyValueNotSupportedByRuntime => ResourceManager.GetString("ERR_EnumPropertyValueNotSupportedByRuntime", resourceCulture);

	internal static string ERR_EqualExpected => ResourceManager.GetString("ERR_EqualExpected", resourceCulture);

	internal static string ERR_EventElementNameShouldBeEmpty => ResourceManager.GetString("ERR_EventElementNameShouldBeEmpty", resourceCulture);

	internal static string ERR_EventNotFound => ResourceManager.GetString("ERR_EventNotFound", resourceCulture);

	internal static string ERR_EventParameterByVarIsNotAllowed => ResourceManager.GetString("ERR_EventParameterByVarIsNotAllowed", resourceCulture);

	internal static string ERR_EventParameterByVarMismatch => ResourceManager.GetString("ERR_EventParameterByVarMismatch", resourceCulture);

	internal static string ERR_EventPublisherCannotContainCode => ResourceManager.GetString("ERR_EventPublisherCannotContainCode", resourceCulture);

	internal static string ERR_EventPublisherCannotContainVariables => ResourceManager.GetString("ERR_EventPublisherCannotContainVariables", resourceCulture);

	internal static string ERR_EventPublisherIncludeSenderWithParameterSender => ResourceManager.GetString("ERR_EventPublisherIncludeSenderWithParameterSender", resourceCulture);

	internal static string ERR_EventReturnNotValid => ResourceManager.GetString("ERR_EventReturnNotValid", resourceCulture);

	internal static string ERR_EventSubscriberMissingParameter => ResourceManager.GetString("ERR_EventSubscriberMissingParameter", resourceCulture);

	internal static string ERR_EventSubscriberParameterTypeMismatch => ResourceManager.GetString("ERR_EventSubscriberParameterTypeMismatch", resourceCulture);

	internal static string ERR_EventTriggerEventPageMissingSourceTable => ResourceManager.GetString("ERR_EventTriggerEventPageMissingSourceTable", resourceCulture);

	internal static string ERR_EventTriggerEventPageMissingSourceTableButFilled => ResourceManager.GetString("ERR_EventTriggerEventPageMissingSourceTableButFilled", resourceCulture);

	internal static string ERR_EventTriggerEventSubscriberWrongRecord => ResourceManager.GetString("ERR_EventTriggerEventSubscriberWrongRecord", resourceCulture);

	internal static string ERR_EventTriggerPageParameterMissingSourceTable => ResourceManager.GetString("ERR_EventTriggerPageParameterMissingSourceTable", resourceCulture);

	internal static string ERR_EventTriggerPageParameterMissingSourceTableButFilled => ResourceManager.GetString("ERR_EventTriggerPageParameterMissingSourceTableButFilled", resourceCulture);

	internal static string ERR_EventUnmatchedParameter => ResourceManager.GetString("ERR_EventUnmatchedParameter", resourceCulture);

	internal static string ERR_ExitValueNotAllowed => ResourceManager.GetString("ERR_ExitValueNotAllowed", resourceCulture);

	internal static string ERR_ExpectedApplicationObjectKeyword => ResourceManager.GetString("ERR_ExpectedApplicationObjectKeyword", resourceCulture);

	internal static string ERR_ExpectedAscendingOrDescendingKeyword => ResourceManager.GetString("ERR_ExpectedAscendingOrDescendingKeyword", resourceCulture);

	internal static string ERR_ExpectedBooleanLiteral => ResourceManager.GetString("ERR_ExpectedBooleanLiteral", resourceCulture);

	internal static string ERR_ExpectedDateLiteral => ResourceManager.GetString("ERR_ExpectedDateLiteral", resourceCulture);

	internal static string ERR_ExpectedDateTimeLiteral => ResourceManager.GetString("ERR_ExpectedDateTimeLiteral", resourceCulture);

	internal static string ERR_ExpectedFieldFilterOrConstKeyword => ResourceManager.GetString("ERR_ExpectedFieldFilterOrConstKeyword", resourceCulture);

	internal static string ERR_ExpectedFilterKeyword => ResourceManager.GetString("ERR_ExpectedFilterKeyword", resourceCulture);

	internal static string ERR_ExpectedFilterKeywordOrIdentifier => ResourceManager.GetString("ERR_ExpectedFilterKeywordOrIdentifier", resourceCulture);

	internal static string ERR_ExpectedIdentifierOrLiteral => ResourceManager.GetString("ERR_ExpectedIdentifierOrLiteral", resourceCulture);

	internal static string ERR_ExpectedIdentifierOrMemberAccess => ResourceManager.GetString("ERR_ExpectedIdentifierOrMemberAccess", resourceCulture);

	internal static string ERR_ExpectedIntLiteral => ResourceManager.GetString("ERR_ExpectedIntLiteral", resourceCulture);

	internal static string ERR_ExpectedLiteral => ResourceManager.GetString("ERR_ExpectedLiteral", resourceCulture);

	internal static string ERR_ExpectedNumericLiteral => ResourceManager.GetString("ERR_ExpectedNumericLiteral", resourceCulture);

	internal static string ERR_ExpectedOptionValue => ResourceManager.GetString("ERR_ExpectedOptionValue", resourceCulture);

	internal static string ERR_ExpectedStringLiteral => ResourceManager.GetString("ERR_ExpectedStringLiteral", resourceCulture);

	internal static string ERR_ExpectedTimeLiteral => ResourceManager.GetString("ERR_ExpectedTimeLiteral", resourceCulture);

	internal static string ERR_ExperimentalFeatureDisabled => ResourceManager.GetString("ERR_ExperimentalFeatureDisabled", resourceCulture);

	internal static string ERR_ExpressionCannotBeUsedInPropertyValue => ResourceManager.GetString("ERR_ExpressionCannotBeUsedInPropertyValue", resourceCulture);

	internal static string ERR_ExpressionExpected => ResourceManager.GetString("ERR_ExpressionExpected", resourceCulture);

	internal static string ERR_ExpressionMustBeBooleanType => ResourceManager.GetString("ERR_ExpressionMustBeBooleanType", resourceCulture);

	internal static string ERR_ExpressionMustBeIntegerType => ResourceManager.GetString("ERR_ExpressionMustBeIntegerType", resourceCulture);

	internal static string ERR_ExpressionMustBeTextType => ResourceManager.GetString("ERR_ExpressionMustBeTextType", resourceCulture);

	internal static string ERR_ExpressionNotValidForWith => ResourceManager.GetString("ERR_ExpressionNotValidForWith", resourceCulture);

	internal static string ERR_ExtensionCannotBeDeclared => ResourceManager.GetString("ERR_ExtensionCannotBeDeclared", resourceCulture);

	internal static string ERR_ExtensionTargetNotFound => ResourceManager.GetString("ERR_ExtensionTargetNotFound", resourceCulture);

	internal static string ERR_ExtensionTargetTypeCannotBeExtended => ResourceManager.GetString("ERR_ExtensionTargetTypeCannotBeExtended", resourceCulture);

	internal static string ERR_ExternalResourceNotAllowed => ResourceManager.GetString("ERR_ExternalResourceNotAllowed", resourceCulture);

	internal static string ERR_FactBoxesCanOnlyContainParts => ResourceManager.GetString("ERR_FactBoxesCanOnlyContainParts", resourceCulture);

	internal static string ERR_FactBoxesNotAllowedOnParts => ResourceManager.GetString("ERR_FactBoxesNotAllowedOnParts", resourceCulture);

	internal static string ERR_FailedToResolveAbsolutePathToRuleSet => ResourceManager.GetString("ERR_FailedToResolveAbsolutePathToRuleSet", resourceCulture);

	internal static string ERR_FailedToResolveRelativePathToRuleSet => ResourceManager.GetString("ERR_FailedToResolveRelativePathToRuleSet", resourceCulture);

	internal static string ERR_FeatureNotAvailable => ResourceManager.GetString("ERR_FeatureNotAvailable", resourceCulture);

	internal static string ERR_FeatureNotSupportedOnCrossPlatformBuilds => ResourceManager.GetString("ERR_FeatureNotSupportedOnCrossPlatformBuilds", resourceCulture);

	internal static string ERR_FieldGroupNotFound => ResourceManager.GetString("ERR_FieldGroupNotFound", resourceCulture);

	internal static string ERR_FieldLengthTooBig => ResourceManager.GetString("ERR_FieldLengthTooBig", resourceCulture);

	internal static string ERR_FieldListCannotBeEmpty => ResourceManager.GetString("ERR_FieldListCannotBeEmpty", resourceCulture);

	internal static string ERR_FieldNotFound => ResourceManager.GetString("ERR_FieldNotFound", resourceCulture);

	internal static string ERR_FieldTypeCannotBeUsedInKey => ResourceManager.GetString("ERR_FieldTypeCannotBeUsedInKey", resourceCulture);

	internal static string ERR_FieldTypeMismatch => ResourceManager.GetString("ERR_FieldTypeMismatch", resourceCulture);

	internal static string ERR_FieldWithInvalidFieldClassAsSumIndexField => ResourceManager.GetString("ERR_FieldWithInvalidFieldClassAsSumIndexField", resourceCulture);

	internal static string ERR_FileInUse => ResourceManager.GetString("ERR_FileInUse", resourceCulture);

	internal static string ERR_FileIsReadonly => ResourceManager.GetString("ERR_FileIsReadonly", resourceCulture);

	internal static string ERR_FileNotFound => ResourceManager.GetString("ERR_FileNotFound", resourceCulture);

	internal static string ERR_FlowFieldCannotBePartOfKeys => ResourceManager.GetString("ERR_FlowFieldCannotBePartOfKeys", resourceCulture);

	internal static string ERR_FlowFieldMustBeBoolean => ResourceManager.GetString("ERR_FlowFieldMustBeBoolean", resourceCulture);

	internal static string ERR_FlowFieldMustBeInteger => ResourceManager.GetString("ERR_FlowFieldMustBeInteger", resourceCulture);

	internal static string ERR_ForEachExpressionMustBeEnumerable => ResourceManager.GetString("ERR_ForEachExpressionMustBeEnumerable", resourceCulture);

	internal static string ERR_ForLoopVariableMustBeNumeric => ResourceManager.GetString("ERR_ForLoopVariableMustBeNumeric", resourceCulture);

	internal static string ERR_ForStatementToOrDownToExpected => ResourceManager.GetString("ERR_ForStatementToOrDownToExpected", resourceCulture);

	internal static string ERR_GenericTypeArgumentCountMismatch => ResourceManager.GetString("ERR_GenericTypeArgumentCountMismatch", resourceCulture);

	internal static string ERR_GridLayoutMustBeOnGrid => ResourceManager.GetString("ERR_GridLayoutMustBeOnGrid", resourceCulture);

	internal static string ERR_GroupingOfActionsNotAllowed => ResourceManager.GetString("ERR_GroupingOfActionsNotAllowed", resourceCulture);

	internal static string ERR_HandlerFunctionsListShouldNotHaveSpaces => ResourceManager.GetString("ERR_HandlerFunctionsListShouldNotHaveSpaces", resourceCulture);

	internal static string ERR_HeadlinePartMustUsedInRoleCenter => ResourceManager.GetString("ERR_HeadlinePartMustUsedInRoleCenter", resourceCulture);

	internal static string ERR_HelpUrlWithMissingLocalePlaceHolders => ResourceManager.GetString("ERR_HelpUrlWithMissingLocalePlaceHolders", resourceCulture);

	internal static string ERR_HelpUrlWithTooManyPlaceHolders => ResourceManager.GetString("ERR_HelpUrlWithTooManyPlaceHolders", resourceCulture);

	internal static string ERR_IdentifierExpected => ResourceManager.GetString("ERR_IdentifierExpected", resourceCulture);

	internal static string ERR_IdentifierExpectedKW => ResourceManager.GetString("ERR_IdentifierExpectedKW", resourceCulture);

	internal static string ERR_IdentifierNotProperlyTerminated => ResourceManager.GetString("ERR_IdentifierNotProperlyTerminated", resourceCulture);

	internal static string ERR_IdentifierTooLong => ResourceManager.GetString("ERR_IdentifierTooLong", resourceCulture);

	internal static string ERR_IllegalStatement => ResourceManager.GetString("ERR_IllegalStatement", resourceCulture);

	internal static string ERR_IncludeCaptionCannotBeSetForNonField => ResourceManager.GetString("ERR_IncludeCaptionCannotBeSetForNonField", resourceCulture);

	internal static string ERR_IncompleteDependency => ResourceManager.GetString("ERR_IncompleteDependency", resourceCulture);

	internal static string ERR_IndexersMustHaveAtLeastOneValue => ResourceManager.GetString("ERR_IndexersMustHaveAtLeastOneValue", resourceCulture);

	internal static string ERR_IntegerLiteralExpected => ResourceManager.GetString("ERR_IntegerLiteralExpected", resourceCulture);

	internal static string ERR_InternalError => ResourceManager.GetString("ERR_InternalError", resourceCulture);

	internal static string ERR_InvalidAlphaNumeric => ResourceManager.GetString("ERR_InvalidAlphaNumeric", resourceCulture);

	internal static string ERR_InvalidApiVersionId => ResourceManager.GetString("ERR_InvalidApiVersionId", resourceCulture);

	internal static string ERR_InvalidApplicationIdRanges => ResourceManager.GetString("ERR_InvalidApplicationIdRanges", resourceCulture);

	internal static string ERR_InvalidApplicationObjectIdentifier => ResourceManager.GetString("ERR_InvalidApplicationObjectIdentifier", resourceCulture);

	internal static string ERR_InvalidAreaKind => ResourceManager.GetString("ERR_InvalidAreaKind", resourceCulture);

	internal static string ERR_InvalidAttribute => ResourceManager.GetString("ERR_InvalidAttribute", resourceCulture);

	internal static string ERR_InvalidAttributeArgumentSyntax => ResourceManager.GetString("ERR_InvalidAttributeArgumentSyntax", resourceCulture);

	internal static string ERR_InvalidAttributeForVariableType => ResourceManager.GetString("ERR_InvalidAttributeForVariableType", resourceCulture);

	internal static string ERR_InvalidAttributeValueForVariableType => ResourceManager.GetString("ERR_InvalidAttributeValueForVariableType", resourceCulture);

	internal static string ERR_InvalidCalculationFormulaMethod => ResourceManager.GetString("ERR_InvalidCalculationFormulaMethod", resourceCulture);

	internal static string ERR_InvalidCodeunitSubtypeForTrigger => ResourceManager.GetString("ERR_InvalidCodeunitSubtypeForTrigger", resourceCulture);

	internal static string ERR_InvalidColumnType => ResourceManager.GetString("ERR_InvalidColumnType", resourceCulture);

	internal static string ERR_InvalidConstExpression => ResourceManager.GetString("ERR_InvalidConstExpression", resourceCulture);

	internal static string ERR_InvalidControlAddInNameCharacter => ResourceManager.GetString("ERR_InvalidControlAddInNameCharacter", resourceCulture);

	internal static string ERR_InvalidControlType => ResourceManager.GetString("ERR_InvalidControlType", resourceCulture);

	internal static string ERR_InvalidDateLiteral => ResourceManager.GetString("ERR_InvalidDateLiteral", resourceCulture);

	internal static string ERR_InvalidDateTimeLiteral => ResourceManager.GetString("ERR_InvalidDateTimeLiteral", resourceCulture);

	internal static string ERR_InvalidDecimalLiteral => ResourceManager.GetString("ERR_InvalidDecimalLiteral", resourceCulture);

	internal static string ERR_InvalidDotNetEventPublisher => ResourceManager.GetString("ERR_InvalidDotNetEventPublisher", resourceCulture);

	internal static string ERR_InvalidExtendedDataTypeRatio => ResourceManager.GetString("ERR_InvalidExtendedDataTypeRatio", resourceCulture);

	internal static string ERR_InvalidFieldAccessSyntax => ResourceManager.GetString("ERR_InvalidFieldAccessSyntax", resourceCulture);

	internal static string ERR_InvalidFieldType => ResourceManager.GetString("ERR_InvalidFieldType", resourceCulture);

	internal static string ERR_InvalidFilterExpression => ResourceManager.GetString("ERR_InvalidFilterExpression", resourceCulture);

	internal static string ERR_InvalidHelpLinkProperty => ResourceManager.GetString("ERR_InvalidHelpLinkProperty", resourceCulture);

	internal static string ERR_InvalidInClientExpression => ResourceManager.GetString("ERR_InvalidInClientExpression", resourceCulture);

	internal static string ERR_InvalidInDatasetDatatype => ResourceManager.GetString("ERR_InvalidInDatasetDatatype", resourceCulture);

	internal static string ERR_InvalidInt32Literal => ResourceManager.GetString("ERR_InvalidInt32Literal", resourceCulture);

	internal static string ERR_InvalidInt64Literal => ResourceManager.GetString("ERR_InvalidInt64Literal", resourceCulture);

	internal static string ERR_InvalidLabelProperty => ResourceManager.GetString("ERR_InvalidLabelProperty", resourceCulture);

	internal static string ERR_InvalidLanguageId => ResourceManager.GetString("ERR_InvalidLanguageId", resourceCulture);

	internal static string ERR_InvalidLink => ResourceManager.GetString("ERR_InvalidLink", resourceCulture);

	internal static string ERR_InvalidManifest => ResourceManager.GetString("ERR_InvalidManifest", resourceCulture);

	internal static string ERR_InvalidManifestGuid => ResourceManager.GetString("ERR_InvalidManifestGuid", resourceCulture);

	internal static string ERR_InvalidManifestVersion => ResourceManager.GetString("ERR_InvalidManifestVersion", resourceCulture);

	internal static string ERR_InvalidManifestVersion_Short => ResourceManager.GetString("ERR_InvalidManifestVersion_Short", resourceCulture);

	internal static string ERR_InvalidMultilineComment => ResourceManager.GetString("ERR_InvalidMultilineComment", resourceCulture);

	internal static string ERR_InvalidNumberOfOptionOrdinalValues => ResourceManager.GetString("ERR_InvalidNumberOfOptionOrdinalValues", resourceCulture);

	internal static string ERR_InvalidOptionOrdinalValue => ResourceManager.GetString("ERR_InvalidOptionOrdinalValue", resourceCulture);

	internal static string ERR_InvalidOptionValue => ResourceManager.GetString("ERR_InvalidOptionValue", resourceCulture);

	internal static string ERR_InvalidPackageFile => ResourceManager.GetString("ERR_InvalidPackageFile", resourceCulture);

	internal static string ERR_InvalidParameterType => ResourceManager.GetString("ERR_InvalidParameterType", resourceCulture);

	internal static string ERR_InvalidPermissionValue => ResourceManager.GetString("ERR_InvalidPermissionValue", resourceCulture);

	internal static string ERR_InvalidPropertyExpression => ResourceManager.GetString("ERR_InvalidPropertyExpression", resourceCulture);

	internal static string ERR_InvalidPropertyOptionValue => ResourceManager.GetString("ERR_InvalidPropertyOptionValue", resourceCulture);

	internal static string ERR_InvalidPropertyValue => ResourceManager.GetString("ERR_InvalidPropertyValue", resourceCulture);

	internal static string ERR_InvalidReferenceModule => ResourceManager.GetString("ERR_InvalidReferenceModule", resourceCulture);

	internal static string ERR_InvalidReturnType => ResourceManager.GetString("ERR_InvalidReturnType", resourceCulture);

	internal static string ERR_InvalidRoleCenterPage => ResourceManager.GetString("ERR_InvalidRoleCenterPage", resourceCulture);

	internal static string ERR_InvalidRuleSetInclude => ResourceManager.GetString("ERR_InvalidRuleSetInclude", resourceCulture);

	internal static string ERR_InvalidRunObjectPropertyValue => ResourceManager.GetString("ERR_InvalidRunObjectPropertyValue", resourceCulture);

	internal static string ERR_InvalidStyleExpressionPropertyValue => ResourceManager.GetString("ERR_InvalidStyleExpressionPropertyValue", resourceCulture);

	internal static string ERR_InvalidSumIndexKeyType => ResourceManager.GetString("ERR_InvalidSumIndexKeyType", resourceCulture);

	internal static string ERR_InvalidSymbolId => ResourceManager.GetString("ERR_InvalidSymbolId", resourceCulture);

	internal static string ERR_InvalidSystemObject => ResourceManager.GetString("ERR_InvalidSystemObject", resourceCulture);

	internal static string ERR_InvalidSystemPartKind => ResourceManager.GetString("ERR_InvalidSystemPartKind", resourceCulture);

	internal static string ERR_InvalidTableFilter => ResourceManager.GetString("ERR_InvalidTableFilter", resourceCulture);

	internal static string ERR_InvalidTimeLiteral => ResourceManager.GetString("ERR_InvalidTimeLiteral", resourceCulture);

	internal static string ERR_InvalidTranslationLocaleCulture => ResourceManager.GetString("ERR_InvalidTranslationLocaleCulture", resourceCulture);

	internal static string ERR_InvalidTriggerSignature => ResourceManager.GetString("ERR_InvalidTriggerSignature", resourceCulture);

	internal static string ERR_InvalidTypeArgument => ResourceManager.GetString("ERR_InvalidTypeArgument", resourceCulture);

	internal static string ERR_InvalidTypeForSomething => ResourceManager.GetString("ERR_InvalidTypeForSomething", resourceCulture);

	internal static string ERR_InvalidValueInContext => ResourceManager.GetString("ERR_InvalidValueInContext", resourceCulture);

	internal static string ERR_InvalidVariableType => ResourceManager.GetString("ERR_InvalidVariableType", resourceCulture);

	internal static string ERR_InvalidViewName => ResourceManager.GetString("ERR_InvalidViewName", resourceCulture);

	internal static string ERR_InvalidXmlName => ResourceManager.GetString("ERR_InvalidXmlName", resourceCulture);

	internal static string ERR_IOExceptionCaught => ResourceManager.GetString("ERR_IOExceptionCaught", resourceCulture);

	internal static string ERR_LanguageIdExpected => ResourceManager.GetString("ERR_LanguageIdExpected", resourceCulture);

	internal static string ERR_LanguageSpecifiedMoreThanOnce => ResourceManager.GetString("ERR_LanguageSpecifiedMoreThanOnce", resourceCulture);

	internal static string ERR_LayoutNotSupportedFileFormat => ResourceManager.GetString("ERR_LayoutNotSupportedFileFormat", resourceCulture);

	internal static string ERR_LeftMustBeOption => ResourceManager.GetString("ERR_LeftMustBeOption", resourceCulture);

	internal static string ERR_LinkTableValueMustReferenceTableElement => ResourceManager.GetString("ERR_LinkTableValueMustReferenceTableElement", resourceCulture);

	internal static string ERR_ListCannotEndWithSeparator => ResourceManager.GetString("ERR_ListCannotEndWithSeparator", resourceCulture);

	internal static string ERR_LocalIllegallyOverrides => ResourceManager.GetString("ERR_LocalIllegallyOverrides", resourceCulture);

	internal static string ERR_MalformedReportLayout => ResourceManager.GetString("ERR_MalformedReportLayout", resourceCulture);

	internal static string ERR_MandatoryProperty => ResourceManager.GetString("ERR_MandatoryProperty", resourceCulture);

	internal static string ERR_ManifestDependencyIdNotMatching => ResourceManager.GetString("ERR_ManifestDependencyIdNotMatching", resourceCulture);

	internal static string ERR_ManifestMismatch => ResourceManager.GetString("ERR_ManifestMismatch", resourceCulture);

	internal static string ERR_MemberAlreadyExists => ResourceManager.GetString("ERR_MemberAlreadyExists", resourceCulture);

	internal static string ERR_MemberNotAllowedInContext => ResourceManager.GetString("ERR_MemberNotAllowedInContext", resourceCulture);

	internal static string ERR_MemberNotAnEvent => ResourceManager.GetString("ERR_MemberNotAnEvent", resourceCulture);

	internal static string ERR_MethodBodyMustBeSpecified => ResourceManager.GetString("ERR_MethodBodyMustBeSpecified", resourceCulture);

	internal static string ERR_MethodBodyNotAllowed => ResourceManager.GetString("ERR_MethodBodyNotAllowed", resourceCulture);

	internal static string ERR_MethodNameExpected => ResourceManager.GetString("ERR_MethodNameExpected", resourceCulture);

	internal static string ERR_MissingApplicationObject => ResourceManager.GetString("ERR_MissingApplicationObject", resourceCulture);

	internal static string ERR_MissingAssociatedProperty => ResourceManager.GetString("ERR_MissingAssociatedProperty", resourceCulture);

	internal static string ERR_MissingAssociatedPropertyMultipleValues => ResourceManager.GetString("ERR_MissingAssociatedPropertyMultipleValues", resourceCulture);

	internal static string ERR_MissingAssociatedPropertySingleValue => ResourceManager.GetString("ERR_MissingAssociatedPropertySingleValue", resourceCulture);

	internal static string ERR_MissingColumnReference => ResourceManager.GetString("ERR_MissingColumnReference", resourceCulture);

	internal static string ERR_MissingDocument => ResourceManager.GetString("ERR_MissingDocument", resourceCulture);

	internal static string ERR_MissingHandlerFunction => ResourceManager.GetString("ERR_MissingHandlerFunction", resourceCulture);

	internal static string ERR_MissingObjectProperty => ResourceManager.GetString("ERR_MissingObjectProperty", resourceCulture);

	internal static string ERR_MissingPackageDependency => ResourceManager.GetString("ERR_MissingPackageDependency", resourceCulture);

	internal static string ERR_MissingPropertyForTranslationApp => ResourceManager.GetString("ERR_MissingPropertyForTranslationApp", resourceCulture);

	internal static string ERR_MissingRequiredManifestProperty => ResourceManager.GetString("ERR_MissingRequiredManifestProperty", resourceCulture);

	internal static string ERR_MissingSupportedLocalesManifestProperty => ResourceManager.GetString("ERR_MissingSupportedLocalesManifestProperty", resourceCulture);

	internal static string ERR_MissingTableElement => ResourceManager.GetString("ERR_MissingTableElement", resourceCulture);

	internal static string ERR_MissingTargetsForMove => ResourceManager.GetString("ERR_MissingTargetsForMove", resourceCulture);

	internal static string ERR_MissingTypeReference => ResourceManager.GetString("ERR_MissingTypeReference", resourceCulture);

	internal static string ERR_ModifyContainsNoChange => ResourceManager.GetString("ERR_ModifyContainsNoChange", resourceCulture);

	internal static string ERR_MultiplePageCustomizationsSpecifiedPerProfilePerTargetPage => ResourceManager.GetString("ERR_MultiplePageCustomizationsSpecifiedPerProfilePerTargetPage", resourceCulture);

	internal static string ERR_MutuallyExclusiveAttributes => ResourceManager.GetString("ERR_MutuallyExclusiveAttributes", resourceCulture);

	internal static string ERR_NameIsNotValidClsIdentifier => ResourceManager.GetString("ERR_NameIsNotValidClsIdentifier", resourceCulture);

	internal static string ERR_NameNotInContext => ResourceManager.GetString("ERR_NameNotInContext", resourceCulture);

	internal static string ERR_NoBreak => ResourceManager.GetString("ERR_NoBreak", resourceCulture);

	internal static string ERR_NoCorrespondingArgument => ResourceManager.GetString("ERR_NoCorrespondingArgument", resourceCulture);

	internal static string ERR_NoFileSpec => ResourceManager.GetString("ERR_NoFileSpec", resourceCulture);

	internal static string ERR_NoImplicitConversion => ResourceManager.GetString("ERR_NoImplicitConversion", resourceCulture);

	internal static string ERR_NoMetadataFile => ResourceManager.GetString("ERR_NoMetadataFile", resourceCulture);

	internal static string ERR_NonCustomizableProperty => ResourceManager.GetString("ERR_NonCustomizableProperty", resourceCulture);

	internal static string ERR_NonInvocableMemberCalled => ResourceManager.GetString("ERR_NonInvocableMemberCalled", resourceCulture);

	internal static string ERR_NoRequestPage => ResourceManager.GetString("ERR_NoRequestPage", resourceCulture);

	internal static string ERR_NoSourceFile => ResourceManager.GetString("ERR_NoSourceFile", resourceCulture);

	internal static string ERR_NoSuchMember => ResourceManager.GetString("ERR_NoSuchMember", resourceCulture);

	internal static string ERR_NotAllowedSyntax => ResourceManager.GetString("ERR_NotAllowedSyntax", resourceCulture);

	internal static string ERR_ObjectChangeNotFoundInCompilation => ResourceManager.GetString("ERR_ObjectChangeNotFoundInCompilation", resourceCulture);

	internal static string ERR_ObjectIdentifierTooLong => ResourceManager.GetString("ERR_ObjectIdentifierTooLong", resourceCulture);

	internal static string ERR_ObjectProhibited => ResourceManager.GetString("ERR_ObjectProhibited", resourceCulture);

	internal static string ERR_ObjectRequired => ResourceManager.GetString("ERR_ObjectRequired", resourceCulture);

	internal static string ERR_ObjectTypeExpected => ResourceManager.GetString("ERR_ObjectTypeExpected", resourceCulture);

	internal static string ERR_ObsoleteFieldCannotBeUsedInKey => ResourceManager.GetString("ERR_ObsoleteFieldCannotBeUsedInKey", resourceCulture);

	internal static string ERR_ObsoleteStateObsolete => ResourceManager.GetString("ERR_ObsoleteStateObsolete", resourceCulture);

	internal static string ERR_ObsoleteSymbol => ResourceManager.GetString("ERR_ObsoleteSymbol", resourceCulture);

	internal static string ERR_OnlyGroupsAndPartsAreValidInRoleCenters => ResourceManager.GetString("ERR_OnlyGroupsAndPartsAreValidInRoleCenters", resourceCulture);

	internal static string ERR_OpenResponseFile => ResourceManager.GetString("ERR_OpenResponseFile", resourceCulture);

	internal static string ERR_OptionCannotContainComma => ResourceManager.GetString("ERR_OptionCannotContainComma", resourceCulture);

	internal static string ERR_OptionReferencedAsMember => ResourceManager.GetString("ERR_OptionReferencedAsMember", resourceCulture);

	internal static string ERR_OrphanedElseStatement => ResourceManager.GetString("ERR_OrphanedElseStatement", resourceCulture);

	internal static string ERR_OutputFileNotSpecified => ResourceManager.GetString("ERR_OutputFileNotSpecified", resourceCulture);

	internal static string ERR_OutputWriteFailed => ResourceManager.GetString("ERR_OutputWriteFailed", resourceCulture);

	internal static string ERR_PackageCacheNotFound => ResourceManager.GetString("ERR_PackageCacheNotFound", resourceCulture);

	internal static string ERR_PackageFileNotFound => ResourceManager.GetString("ERR_PackageFileNotFound", resourceCulture);

	internal static string ERR_PackageNotLoaded => ResourceManager.GetString("ERR_PackageNotLoaded", resourceCulture);

	internal static string ERR_PageMustSpecifySourceTable => ResourceManager.GetString("ERR_PageMustSpecifySourceTable", resourceCulture);

	internal static string ERR_PageNotFound => ResourceManager.GetString("ERR_PageNotFound", resourceCulture);

	internal static string ERR_ParameterCountDoesNotMatchAttribute => ResourceManager.GetString("ERR_ParameterCountDoesNotMatchAttribute", resourceCulture);

	internal static string ERR_ParameterTypeDoesNotMatchAttribute => ResourceManager.GetString("ERR_ParameterTypeDoesNotMatchAttribute", resourceCulture);

	internal static string ERR_ParameterTypeNotSerializable => ResourceManager.GetString("ERR_ParameterTypeNotSerializable", resourceCulture);

	internal static string ERR_PartsCannotContainParts => ResourceManager.GetString("ERR_PartsCannotContainParts", resourceCulture);

	internal static string ERR_PartsCanOnlyContainProcessingAreas => ResourceManager.GetString("ERR_PartsCanOnlyContainProcessingAreas", resourceCulture);

	internal static string ERR_PathMustBeRelative => ResourceManager.GetString("ERR_PathMustBeRelative", resourceCulture);

	internal static string ERR_PermissionDuplicatedObject => ResourceManager.GetString("ERR_PermissionDuplicatedObject", resourceCulture);

	internal static string ERR_PrimaryKeyAppendedToSecondaryKey => ResourceManager.GetString("ERR_PrimaryKeyAppendedToSecondaryKey", resourceCulture);

	internal static string ERR_PrimaryKeyMustBeEnabled => ResourceManager.GetString("ERR_PrimaryKeyMustBeEnabled", resourceCulture);

	internal static string ERR_PrimaryKeyMustHaveMaintainSqlIndexEnabled => ResourceManager.GetString("ERR_PrimaryKeyMustHaveMaintainSqlIndexEnabled", resourceCulture);

	internal static string ERR_PrimaryKeyMustNotHaveUnique => ResourceManager.GetString("ERR_PrimaryKeyMustNotHaveUnique", resourceCulture);

	internal static string ERR_PropertyCanBeUsedOnlyOnKeysWithFieldsFromTheSameTable => ResourceManager.GetString("ERR_PropertyCanBeUsedOnlyOnKeysWithFieldsFromTheSameTable", resourceCulture);

	internal static string ERR_PropertyCannotBeBlank => ResourceManager.GetString("ERR_PropertyCannotBeBlank", resourceCulture);

	internal static string ERR_PropertyCustomizationNotSupportedByRuntime => ResourceManager.GetString("ERR_PropertyCustomizationNotSupportedByRuntime", resourceCulture);

	internal static string ERR_PropertyExpressionKindIsNotValidExpectedAllExpressionKind => ResourceManager.GetString("ERR_PropertyExpressionKindIsNotValidExpectedAllExpressionKind", resourceCulture);

	internal static string ERR_PropertyExpressionKindIsNotValidExpectedConstOrFieldOrFilter => ResourceManager.GetString("ERR_PropertyExpressionKindIsNotValidExpectedConstOrFieldOrFilter", resourceCulture);

	internal static string ERR_PropertyExpressionKindIsNotValidExpectedConstOrFilter => ResourceManager.GetString("ERR_PropertyExpressionKindIsNotValidExpectedConstOrFilter", resourceCulture);

	internal static string ERR_PropertyInfoNotAvailable => ResourceManager.GetString("ERR_PropertyInfoNotAvailable", resourceCulture);

	internal static string ERR_PropertyInvalidOnTableExtension => ResourceManager.GetString("ERR_PropertyInvalidOnTableExtension", resourceCulture);

	internal static string ERR_PropertyNotAllowedOnPageWithoutSourceTable => ResourceManager.GetString("ERR_PropertyNotAllowedOnPageWithoutSourceTable", resourceCulture);

	internal static string ERR_PropertyOnlyValidInControlOfType => ResourceManager.GetString("ERR_PropertyOnlyValidInControlOfType", resourceCulture);

	internal static string ERR_PropertyTypeFieldTypeMismatch => ResourceManager.GetString("ERR_PropertyTypeFieldTypeMismatch", resourceCulture);

	internal static string ERR_PropertyUsedAsMethod => ResourceManager.GetString("ERR_PropertyUsedAsMethod", resourceCulture);

	internal static string ERR_PropertyValueCannotBeEmpty => ResourceManager.GetString("ERR_PropertyValueCannotBeEmpty", resourceCulture);

	internal static string ERR_PropertyValueCannotBeUsedInThisContext => ResourceManager.GetString("ERR_PropertyValueCannotBeUsedInThisContext", resourceCulture);

	internal static string ERR_PropertyValueMustBePositive => ResourceManager.GetString("ERR_PropertyValueMustBePositive", resourceCulture);

	internal static string ERR_PropertyValueNotAValidEnumValue => ResourceManager.GetString("ERR_PropertyValueNotAValidEnumValue", resourceCulture);

	internal static string ERR_PropertyValueNotAValidOptionValue => ResourceManager.GetString("ERR_PropertyValueNotAValidOptionValue", resourceCulture);

	internal static string ERR_PropertyValuesAreMutuallyExclusive => ResourceManager.GetString("ERR_PropertyValuesAreMutuallyExclusive", resourceCulture);

	internal static string ERR_QueriesMustDefineAtLeastOneColumn => ResourceManager.GetString("ERR_QueriesMustDefineAtLeastOneColumn", resourceCulture);

	internal static string ERR_QueriesMustHaveTopLevelDataItem => ResourceManager.GetString("ERR_QueriesMustHaveTopLevelDataItem", resourceCulture);

	internal static string ERR_QueryColumnsMustDefineSourceExpressionOrCountMethod => ResourceManager.GetString("ERR_QueryColumnsMustDefineSourceExpressionOrCountMethod", resourceCulture);

	internal static string ERR_QueryElementSourceMustBeFieldOnTable => ResourceManager.GetString("ERR_QueryElementSourceMustBeFieldOnTable", resourceCulture);

	internal static string ERR_RefLvalueExpected => ResourceManager.GetString("ERR_RefLvalueExpected", resourceCulture);

	internal static string ERR_RepeaterCanOnlyBeAddedToPagesThatHaveASourceTable => ResourceManager.GetString("ERR_RepeaterCanOnlyBeAddedToPagesThatHaveASourceTable", resourceCulture);

	internal static string ERR_RequiresApplicationObjectRunObject => ResourceManager.GetString("ERR_RequiresApplicationObjectRunObject", resourceCulture);

	internal static string ERR_RequiresMediaExtendedDataType => ResourceManager.GetString("ERR_RequiresMediaExtendedDataType", resourceCulture);

	internal static string ERR_RequiresTextualExtendedDataType => ResourceManager.GetString("ERR_RequiresTextualExtendedDataType", resourceCulture);

	internal static string ERR_ReturnValueMustBeUsed => ResourceManager.GetString("ERR_ReturnValueMustBeUsed", resourceCulture);

	internal static string ERR_ReturnValueNotAllowed => ResourceManager.GetString("ERR_ReturnValueNotAllowed", resourceCulture);

	internal static string ERR_ReturnValueTypeDoesNotMatchAttribute => ResourceManager.GetString("ERR_ReturnValueTypeDoesNotMatchAttribute", resourceCulture);

	internal static string ERR_RoleCenterShouldNotHaveTriggers => ResourceManager.GetString("ERR_RoleCenterShouldNotHaveTriggers", resourceCulture);

	internal static string ERR_RuleSetHasDuplicateRules => ResourceManager.GetString("ERR_RuleSetHasDuplicateRules", resourceCulture);

	internal static string ERR_RuleSetMissingProperty => ResourceManager.GetString("ERR_RuleSetMissingProperty", resourceCulture);

	internal static string ERR_SelfReferenceDependency => ResourceManager.GetString("ERR_SelfReferenceDependency", resourceCulture);

	internal static string ERR_SemicolonExpected => ResourceManager.GetString("ERR_SemicolonExpected", resourceCulture);

	internal static string ERR_SourceTableUnreachable => ResourceManager.GetString("ERR_SourceTableUnreachable", resourceCulture);

	internal static string ERR_SqlIndexOnPKMustBeIdentical => ResourceManager.GetString("ERR_SqlIndexOnPKMustBeIdentical", resourceCulture);

	internal static string ERR_SqlTimestampFieldCannotBePartOfKeys => ResourceManager.GetString("ERR_SqlTimestampFieldCannotBePartOfKeys", resourceCulture);

	internal static string ERR_SumAverageOnlySupportedForNumericFields => ResourceManager.GetString("ERR_SumAverageOnlySupportedForNumericFields", resourceCulture);

	internal static string ERR_SwitchNeedsNumber => ResourceManager.GetString("ERR_SwitchNeedsNumber", resourceCulture);

	internal static string ERR_SwitchNeedsString => ResourceManager.GetString("ERR_SwitchNeedsString", resourceCulture);

	internal static string ERR_SymbolCannotBeUsedInThisContext => ResourceManager.GetString("ERR_SymbolCannotBeUsedInThisContext", resourceCulture);

	internal static string ERR_SymbolNotFoundInTarget => ResourceManager.GetString("ERR_SymbolNotFoundInTarget", resourceCulture);

	internal static string ERR_SyntaxError => ResourceManager.GetString("ERR_SyntaxError", resourceCulture);

	internal static string ERR_SystemTableCannotBeExtended => ResourceManager.GetString("ERR_SystemTableCannotBeExtended", resourceCulture);

	internal static string ERR_TableHasToHaveAtLeastOneNormalField => ResourceManager.GetString("ERR_TableHasToHaveAtLeastOneNormalField", resourceCulture);

	internal static string ERR_TableKeyTooManyFields => ResourceManager.GetString("ERR_TableKeyTooManyFields", resourceCulture);

	internal static string ERR_TableTooManyKeys => ResourceManager.GetString("ERR_TableTooManyKeys", resourceCulture);

	internal static string ERR_TemporaryMustBeTable => ResourceManager.GetString("ERR_TemporaryMustBeTable", resourceCulture);

	internal static string ERR_TestCodeunitsEventSubscribersMustHaveManualBinding => ResourceManager.GetString("ERR_TestCodeunitsEventSubscribersMustHaveManualBinding", resourceCulture);

	internal static string ERR_TextLiteralNotProperlyTerminated => ResourceManager.GetString("ERR_TextLiteralNotProperlyTerminated", resourceCulture);

	internal static string ERR_ThereMustBeOneRootNode => ResourceManager.GetString("ERR_ThereMustBeOneRootNode", resourceCulture);

	internal static string ERR_TopLevelDataItemCannotHaveLinkProperty => ResourceManager.GetString("ERR_TopLevelDataItemCannotHaveLinkProperty", resourceCulture);

	internal static string ERR_TriggerMissingAssociatedProperty => ResourceManager.GetString("ERR_TriggerMissingAssociatedProperty", resourceCulture);

	internal static string ERR_TriggerMissingAssociatedPropertyMultipleValues => ResourceManager.GetString("ERR_TriggerMissingAssociatedPropertyMultipleValues", resourceCulture);

	internal static string ERR_TriggerMissingAssociatedPropertySingleValue => ResourceManager.GetString("ERR_TriggerMissingAssociatedPropertySingleValue", resourceCulture);

	internal static string ERR_TriggersCannotBeCalledDirectly => ResourceManager.GetString("ERR_TriggersCannotBeCalledDirectly", resourceCulture);

	internal static string ERR_TypeIsNotGeneric => ResourceManager.GetString("ERR_TypeIsNotGeneric", resourceCulture);

	internal static string ERR_TypeLengthTooBig => ResourceManager.GetString("ERR_TypeLengthTooBig", resourceCulture);

	internal static string ERR_UndefinedOptionValue => ResourceManager.GetString("ERR_UndefinedOptionValue", resourceCulture);

	internal static string ERR_UnexpectedCharacter => ResourceManager.GetString("ERR_UnexpectedCharacter", resourceCulture);

	internal static string ERR_UnexpectedToken => ResourceManager.GetString("ERR_UnexpectedToken", resourceCulture);

	internal static string ERR_UnionsAreNotSupportedInQueries => ResourceManager.GetString("ERR_UnionsAreNotSupportedInQueries", resourceCulture);

	internal static string ERR_UnknownDotNetEvent => ResourceManager.GetString("ERR_UnknownDotNetEvent", resourceCulture);

	internal static string ERR_UnknownTrigger => ResourceManager.GetString("ERR_UnknownTrigger", resourceCulture);

	internal static string ERR_UnknownType => ResourceManager.GetString("ERR_UnknownType", resourceCulture);

	internal static string ERR_UnspecifiedPackageCachePath => ResourceManager.GetString("ERR_UnspecifiedPackageCachePath", resourceCulture);

	internal static string ERR_UnsupportedManifestRuntimeVersion => ResourceManager.GetString("ERR_UnsupportedManifestRuntimeVersion", resourceCulture);

	internal static string ERR_UseBeforeDeclaration => ResourceManager.GetString("ERR_UseBeforeDeclaration", resourceCulture);

	internal static string ERR_ValueOutsideValidRange => ResourceManager.GetString("ERR_ValueOutsideValidRange", resourceCulture);

	internal static string ERR_ValueTypeDoesNotMatchFieldType => ResourceManager.GetString("ERR_ValueTypeDoesNotMatchFieldType", resourceCulture);

	internal static string ERR_VarParameterNotAllowed => ResourceManager.GetString("ERR_VarParameterNotAllowed", resourceCulture);

	internal static string ERR_ViewNotFound => ResourceManager.GetString("ERR_ViewNotFound", resourceCulture);

	internal static string ERR_ViewsBooleanExpressionsShouldNotUseVariables => ResourceManager.GetString("ERR_ViewsBooleanExpressionsShouldNotUseVariables", resourceCulture);

	internal static string ERR_ViewsLayoutOnlySupportedOnContent => ResourceManager.GetString("ERR_ViewsLayoutOnlySupportedOnContent", resourceCulture);

	internal static string ERR_ViewsOnlySupportedOnPagesOfType => ResourceManager.GetString("ERR_ViewsOnlySupportedOnPagesOfType", resourceCulture);

	internal static string ERR_ViewsOrderByOnlySupportOneDirection => ResourceManager.GetString("ERR_ViewsOrderByOnlySupportOneDirection", resourceCulture);

	internal static string ERR_VisibilityDoesNotMatchAttribute => ResourceManager.GetString("ERR_VisibilityDoesNotMatchAttribute", resourceCulture);

	internal static string ERR_WordMergeDataItemMustBeTopLevel => ResourceManager.GetString("ERR_WordMergeDataItemMustBeTopLevel", resourceCulture);

	internal static string ERR_WrongLinkFormat => ResourceManager.GetString("ERR_WrongLinkFormat", resourceCulture);

	internal static string ERR_WrongManifestPropertyType => ResourceManager.GetString("ERR_WrongManifestPropertyType", resourceCulture);

	internal static string ERR_XmlNameMustBeSpecified => ResourceManager.GetString("ERR_XmlNameMustBeSpecified", resourceCulture);

	internal static string ExceptionContext => ResourceManager.GetString("ExceptionContext", resourceCulture);

	internal static string Feature_DotNet => ResourceManager.GetString("Feature_DotNet", resourceCulture);

	internal static string Feature_Enum => ResourceManager.GetString("Feature_Enum", resourceCulture);

	internal static string Feature_FieldGroupExtension => ResourceManager.GetString("Feature_FieldGroupExtension", resourceCulture);

	internal static string Feature_RunQueryObject => ResourceManager.GetString("Feature_RunQueryObject", resourceCulture);

	internal static string Feature_Views => ResourceManager.GetString("Feature_Views", resourceCulture);

	internal static string FileNotFoundException => ResourceManager.GetString("FileNotFoundException", resourceCulture);

	internal static string FTL_InputFileNameTooLong => ResourceManager.GetString("FTL_InputFileNameTooLong", resourceCulture);

	internal static string FTL_InvalidTarget => ResourceManager.GetString("FTL_InvalidTarget", resourceCulture);

	internal static string HttpUriLinkSpecification => ResourceManager.GetString("HttpUriLinkSpecification", resourceCulture);

	internal static string IDS_ALCHelp => ResourceManager.GetString("IDS_ALCHelp", resourceCulture);

	internal static string IDS_LogoLine1 => ResourceManager.GetString("IDS_LogoLine1", resourceCulture);

	internal static string IDS_LogoLine2 => ResourceManager.GetString("IDS_LogoLine2", resourceCulture);

	internal static string IDS_Text => ResourceManager.GetString("IDS_Text", resourceCulture);

	internal static string IDS_ToolName => ResourceManager.GetString("IDS_ToolName", resourceCulture);

	internal static string INF_FileMatchesADefinition => ResourceManager.GetString("INF_FileMatchesADefinition", resourceCulture);

	internal static string INF_PERS_ActionNotFound => ResourceManager.GetString("INF_PERS_ActionNotFound", resourceCulture);

	internal static string INF_PERS_AmbigSymbolReference => ResourceManager.GetString("INF_PERS_AmbigSymbolReference", resourceCulture);

	internal static string INF_PERS_ControlNotFound => ResourceManager.GetString("INF_PERS_ControlNotFound", resourceCulture);

	internal static string INF_PERS_MissingTargetsForMove => ResourceManager.GetString("INF_PERS_MissingTargetsForMove", resourceCulture);

	internal static string INF_PERS_ViewNotFound => ResourceManager.GetString("INF_PERS_ViewNotFound", resourceCulture);

	internal static string INF_SuccessfullyIncludedTranslationsForLanguages => ResourceManager.GetString("INF_SuccessfullyIncludedTranslationsForLanguages", resourceCulture);

	internal static string InvalidDiagnosticLocationReported => ResourceManager.GetString("InvalidDiagnosticLocationReported", resourceCulture);

	internal static string InvalidTree => ResourceManager.GetString("InvalidTree", resourceCulture);

	internal static string LclFileGenerationHashCollisionError => ResourceManager.GetString("LclFileGenerationHashCollisionError", resourceCulture);

	internal static string MissingListItem => ResourceManager.GetString("MissingListItem", resourceCulture);

	internal static string MultithreadedAnalyzerExecutionNote => ResourceManager.GetString("MultithreadedAnalyzerExecutionNote", resourceCulture);

	internal static string NameCannotBeEmpty => ResourceManager.GetString("NameCannotBeEmpty", resourceCulture);

	internal static string NameCannotStartWithWhitespace => ResourceManager.GetString("NameCannotStartWithWhitespace", resourceCulture);

	internal static string NameContainsInvalidCharacter => ResourceManager.GetString("NameContainsInvalidCharacter", resourceCulture);

	internal static string NoAnalyzersFound => ResourceManager.GetString("NoAnalyzersFound", resourceCulture);

	internal static string ObjectDisposedError => ResourceManager.GetString("ObjectDisposedError", resourceCulture);

	internal static string PositionIsNotWithinSyntax => ResourceManager.GetString("PositionIsNotWithinSyntax", resourceCulture);

	internal static string PositionNotWithinTree => ResourceManager.GetString("PositionNotWithinTree", resourceCulture);

	internal static string SeparatorIsExpected => ResourceManager.GetString("SeparatorIsExpected", resourceCulture);

	internal static string SeverityError => ResourceManager.GetString("SeverityError", resourceCulture);

	internal static string SeverityHidden => ResourceManager.GetString("SeverityHidden", resourceCulture);

	internal static string SeverityInfo => ResourceManager.GetString("SeverityInfo", resourceCulture);

	internal static string SeverityWarning => ResourceManager.GetString("SeverityWarning", resourceCulture);

	internal static string SpanDoesNotIncludeEndOfLine => ResourceManager.GetString("SpanDoesNotIncludeEndOfLine", resourceCulture);

	internal static string SpanDoesNotIncludeStartOfLine => ResourceManager.GetString("SpanDoesNotIncludeStartOfLine", resourceCulture);

	internal static string StartMustNotBeNegative => ResourceManager.GetString("StartMustNotBeNegative", resourceCulture);

	internal static string StreamMustSupportReadAndSeek => ResourceManager.GetString("StreamMustSupportReadAndSeek", resourceCulture);

	internal static string StreamNotWriteable => ResourceManager.GetString("StreamNotWriteable", resourceCulture);

	internal static string SyntaxTreeAlreadyPresent => ResourceManager.GetString("SyntaxTreeAlreadyPresent", resourceCulture);

	internal static string SyntaxTreeNotFoundTo => ResourceManager.GetString("SyntaxTreeNotFoundTo", resourceCulture);

	internal static string TextSpanNotIncludedInSemanticModel => ResourceManager.GetString("TextSpanNotIncludedInSemanticModel", resourceCulture);

	internal static string TreeMustHaveARootNodeWith => ResourceManager.GetString("TreeMustHaveARootNodeWith", resourceCulture);

	internal static string TreeNotPartOfCompilation => ResourceManager.GetString("TreeNotPartOfCompilation", resourceCulture);

	internal static string Trees0 => ResourceManager.GetString("Trees0", resourceCulture);

	internal static string Unresolved => ResourceManager.GetString("Unresolved", resourceCulture);

	internal static string UnsupportedAnalyzerInstance => ResourceManager.GetString("UnsupportedAnalyzerInstance", resourceCulture);

	internal static string UnsupportedDiagnosticReported => ResourceManager.GetString("UnsupportedDiagnosticReported", resourceCulture);

	internal static string UnsupportedSeekOrigin => ResourceManager.GetString("UnsupportedSeekOrigin", resourceCulture);

	internal static string ValueMayNotBeLargerThanStream => ResourceManager.GetString("ValueMayNotBeLargerThanStream", resourceCulture);

	internal static string ValueMayNotBeLessThanZero => ResourceManager.GetString("ValueMayNotBeLessThanZero", resourceCulture);

	internal static string ValueMustBeGreaterThan => ResourceManager.GetString("ValueMustBeGreaterThan", resourceCulture);

	internal static string ValueMustBeThreeCharacters => ResourceManager.GetString("ValueMustBeThreeCharacters", resourceCulture);

	internal static string ValueMustBeWithinStreamLengthOrMinusOne => ResourceManager.GetString("ValueMustBeWithinStreamLengthOrMinusOne", resourceCulture);

	internal static string VersionFromTo => ResourceManager.GetString("VersionFromTo", resourceCulture);

	internal static string VersionFromToExcluded => ResourceManager.GetString("VersionFromToExcluded", resourceCulture);

	internal static string VersionOrGreater => ResourceManager.GetString("VersionOrGreater", resourceCulture);

	internal static string WRN_AnalyzerCannotBeCreated => ResourceManager.GetString("WRN_AnalyzerCannotBeCreated", resourceCulture);

	internal static string WRN_ConstantValueNotAValidOptionValue => ResourceManager.GetString("WRN_ConstantValueNotAValidOptionValue", resourceCulture);

	internal static string WRN_DeprecatedIdSyntax => ResourceManager.GetString("WRN_DeprecatedIdSyntax", resourceCulture);

	internal static string WRN_DeprecatedSymbol => ResourceManager.GetString("WRN_DeprecatedSymbol", resourceCulture);

	internal static string WRN_DuplicateTranslationItemForSameId => ResourceManager.GetString("WRN_DuplicateTranslationItemForSameId", resourceCulture);

	internal static string WRN_EmptyConstIsObsolete => ResourceManager.GetString("WRN_EmptyConstIsObsolete", resourceCulture);

	internal static string WRN_ERR_ActionInGroupInSectionsAreaShouldOnlyRunListPages => ResourceManager.GetString("WRN_ERR_ActionInGroupInSectionsAreaShouldOnlyRunListPages", resourceCulture);

	internal static string WRN_ERR_DuplicateMemberName => ResourceManager.GetString("WRN_ERR_DuplicateMemberName", resourceCulture);

	internal static string WRN_ERR_FieldImageOnlyOnFieldInCueGroup => ResourceManager.GetString("WRN_ERR_FieldImageOnlyOnFieldInCueGroup", resourceCulture);

	internal static string WRN_ERR_FieldNameIdentifierTooLong => ResourceManager.GetString("WRN_ERR_FieldNameIdentifierTooLong", resourceCulture);

	internal static string WRN_ERR_KeyNameIdentifierTooLong => ResourceManager.GetString("WRN_ERR_KeyNameIdentifierTooLong", resourceCulture);

	internal static string WRN_ERR_MemberNameOnlyAllowedForTriggers => ResourceManager.GetString("WRN_ERR_MemberNameOnlyAllowedForTriggers", resourceCulture);

	internal static string WRN_ERR_PagePartMustBePart => ResourceManager.GetString("WRN_ERR_PagePartMustBePart", resourceCulture);

	internal static string WRN_FileAlreadyIncluded => ResourceManager.GetString("WRN_FileAlreadyIncluded", resourceCulture);

	internal static string WRN_FileDoesNotMatchAnyFormat => ResourceManager.GetString("WRN_FileDoesNotMatchAnyFormat", resourceCulture);

	internal static string WRN_FileIsReadonly => ResourceManager.GetString("WRN_FileIsReadonly", resourceCulture);

	internal static string WRN_ImageNotValid => ResourceManager.GetString("WRN_ImageNotValid", resourceCulture);

	internal static string WRN_ImageOnNestedActionGroupInSectionsArea => ResourceManager.GetString("WRN_ImageOnNestedActionGroupInSectionsArea", resourceCulture);

	internal static string WRN_IncorrectLabelSyntax => ResourceManager.GetString("WRN_IncorrectLabelSyntax", resourceCulture);

	internal static string WRN_MemberAlreadyDeclaredInBase => ResourceManager.GetString("WRN_MemberAlreadyDeclaredInBase", resourceCulture);

	internal static string WRN_MemberAlreadyExists => ResourceManager.GetString("WRN_MemberAlreadyExists", resourceCulture);

	internal static string WRN_MissingProjectManifest => ResourceManager.GetString("WRN_MissingProjectManifest", resourceCulture);

	internal static string WRN_NoAnalyzerInAssembly => ResourceManager.GetString("WRN_NoAnalyzerInAssembly", resourceCulture);

	internal static string WRN_NoConfigNotOnCommandLine => ResourceManager.GetString("WRN_NoConfigNotOnCommandLine", resourceCulture);

	internal static string WRN_ObsoleteMultilanguageSyntax => ResourceManager.GetString("WRN_ObsoleteMultilanguageSyntax", resourceCulture);

	internal static string WRN_ObsoleteStateObsolete => ResourceManager.GetString("WRN_ObsoleteStateObsolete", resourceCulture);

	internal static string WRN_ObsoleteStatePending => ResourceManager.GetString("WRN_ObsoleteStatePending", resourceCulture);

	internal static string WRN_PermissionObjectIsMissing => ResourceManager.GetString("WRN_PermissionObjectIsMissing", resourceCulture);

	internal static string WRN_PropertyIsObsolete => ResourceManager.GetString("WRN_PropertyIsObsolete", resourceCulture);

	internal static string WRN_ReuseOfAreaName => ResourceManager.GetString("WRN_ReuseOfAreaName", resourceCulture);

	internal static string WRN_SortingFieldShouldBePartOfKey => ResourceManager.GetString("WRN_SortingFieldShouldBePartOfKey", resourceCulture);

	internal static string WRN_SortingFieldsShouldBePartOfKey => ResourceManager.GetString("WRN_SortingFieldsShouldBePartOfKey", resourceCulture);

	internal static string WRN_TableUsingReservedFieldId => ResourceManager.GetString("WRN_TableUsingReservedFieldId", resourceCulture);

	internal static string WRN_TableUsingReservedFieldName => ResourceManager.GetString("WRN_TableUsingReservedFieldName", resourceCulture);

	internal static string WRN_TranslationFileTargetLanguageInvalid => ResourceManager.GetString("WRN_TranslationFileTargetLanguageInvalid", resourceCulture);

	internal static string WRN_TranslationFileTargetLanguageMissing => ResourceManager.GetString("WRN_TranslationFileTargetLanguageMissing", resourceCulture);

	internal static string WRN_TranslationItemSourceStringMismatch => ResourceManager.GetString("WRN_TranslationItemSourceStringMismatch", resourceCulture);

	internal static string WRN_TranslationItemTranslatedStringTooLong => ResourceManager.GetString("WRN_TranslationItemTranslatedStringTooLong", resourceCulture);

	internal static string WRN_UnableToLoadAnalyzer => ResourceManager.GetString("WRN_UnableToLoadAnalyzer", resourceCulture);

	internal static string WRN_XmlValidationErrorOccurred => ResourceManager.GetString("WRN_XmlValidationErrorOccurred", resourceCulture);

	internal static string WrongSyntaxKind => ResourceManager.GetString("WrongSyntaxKind", resourceCulture);

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	internal CodeAnalysisResources()
	{
	}
}
