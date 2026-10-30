using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class ErrorFacts
{
	private const string s_titleSuffix = "_Title";

	private const string s_descriptionSuffix = "_Description";

	private static readonly Lazy<ImmutableDictionary<ErrorCode, string>> s_helpLinksMap = new Lazy<ImmutableDictionary<ErrorCode, string>>(CreateHelpLinks);

	private static readonly Lazy<ImmutableDictionary<ErrorCode, string>> s_categoriesMap = new Lazy<ImmutableDictionary<ErrorCode, string>>(CreateCategoriesMap);

	private static ResourceManager s_resourceManager;

	private static ResourceManager ResourceManager
	{
		get
		{
			if (s_resourceManager == null)
			{
				s_resourceManager = new ResourceManager("Microsoft.CodeAnalysis.CSharp.CSharpResources", typeof(ErrorCode).GetTypeInfo().Assembly);
			}
			return s_resourceManager;
		}
	}

	private static ImmutableDictionary<ErrorCode, string> CreateHelpLinks()
	{
		return new Dictionary<ErrorCode, string>().ToImmutableDictionary();
	}

	private static ImmutableDictionary<ErrorCode, string> CreateCategoriesMap()
	{
		return new Dictionary<ErrorCode, string>().ToImmutableDictionary();
	}

	internal static DiagnosticSeverity GetSeverity(ErrorCode code)
	{
		switch (code)
		{
		case ErrorCode.Void:
			return (DiagnosticSeverity)(-2);
		case ErrorCode.Unknown:
			return (DiagnosticSeverity)(-1);
		default:
			if (IsWarning(code))
			{
				return DiagnosticSeverity.Warning;
			}
			if (IsInfo(code))
			{
				return DiagnosticSeverity.Info;
			}
			if (IsHidden(code))
			{
				return DiagnosticSeverity.Hidden;
			}
			return DiagnosticSeverity.Error;
		}
	}

	public static string GetMessage(MessageID code, CultureInfo culture)
	{
		return ResourceManager.GetString(code.ToString(), culture);
	}

	public static string GetMessage(ErrorCode code, CultureInfo culture)
	{
		return ResourceManager.GetString(code.ToString(), culture);
	}

	public static LocalizableResourceString GetMessageFormat(ErrorCode code)
	{
		return new LocalizableResourceString(code.ToString(), ResourceManager, typeof(ErrorFacts));
	}

	public static LocalizableResourceString GetTitle(ErrorCode code)
	{
		return new LocalizableResourceString(code.ToString() + "_Title", ResourceManager, typeof(ErrorFacts));
	}

	public static LocalizableResourceString GetDescription(ErrorCode code)
	{
		return new LocalizableResourceString(code.ToString() + "_Description", ResourceManager, typeof(ErrorFacts));
	}

	public static string GetHelpLink(ErrorCode code)
	{
		if (s_helpLinksMap.Value.TryGetValue(code, out var value))
		{
			return value;
		}
		return string.Empty;
	}

	public static string GetCategory(ErrorCode code)
	{
		if (s_categoriesMap.Value.TryGetValue(code, out var value))
		{
			return value;
		}
		return "Compiler";
	}

	public static string GetMessage(XmlParseErrorCode id, CultureInfo culture)
	{
		return ResourceManager.GetString(id.ToString(), culture);
	}

	internal static int GetWarningLevel(ErrorCode code)
	{
		if (IsInfo(code) || IsHidden(code))
		{
			return 4;
		}
		switch (code)
		{
		case ErrorCode.WRN_InvalidMainSig:
		case ErrorCode.WRN_LowercaseEllSuffix:
		case ErrorCode.WRN_NewNotRequired:
		case ErrorCode.WRN_MainCantBeGeneric:
		case ErrorCode.WRN_ProtectedInSealed:
		case ErrorCode.WRN_UnassignedInternalField:
		case ErrorCode.WRN_MissingParamTag:
		case ErrorCode.WRN_MissingXMLComment:
		case ErrorCode.WRN_MissingTypeParamTag:
		case ErrorCode.WRN_InvalidVersionFormat:
			return 4;
		case ErrorCode.WRN_UnreferencedEvent:
		case ErrorCode.WRN_DuplicateUsing:
		case ErrorCode.WRN_UnreferencedVar:
		case ErrorCode.WRN_UnreferencedField:
		case ErrorCode.WRN_UnreferencedVarAssg:
		case ErrorCode.WRN_SequentialOnPartialClass:
		case ErrorCode.WRN_UnreferencedFieldAssg:
		case ErrorCode.WRN_AmbiguousXMLReference:
		case ErrorCode.WRN_PossibleMistakenNullStatement:
		case ErrorCode.WRN_EqualsWithoutGetHashCode:
		case ErrorCode.WRN_EqualityOpWithoutEquals:
		case ErrorCode.WRN_EqualityOpWithoutGetHashCode:
		case ErrorCode.WRN_IncorrectBooleanAssg:
		case ErrorCode.WRN_BitwiseOrSignExtend:
		case ErrorCode.WRN_TypeParameterSameAsOuterTypeParameter:
		case ErrorCode.WRN_DebugFullNameTooLong:
		case ErrorCode.WRN_InvalidAssemblyName:
		case ErrorCode.WRN_UnifyReferenceBldRev:
		case ErrorCode.WRN_AssignmentToSelf:
		case ErrorCode.WRN_ComparisonToSelf:
		case ErrorCode.WRN_IsDynamicIsConfusing:
		case ErrorCode.WRN_PdbLocalNameTooLong:
		case ErrorCode.WRN_UnreferencedLocalFunction:
			return 3;
		case ErrorCode.WRN_NewRequired:
		case ErrorCode.WRN_NewOrOverrideExpected:
		case ErrorCode.WRN_UnreachableCode:
		case ErrorCode.WRN_UnreferencedLabel:
		case ErrorCode.WRN_NegativeArrayIndex:
		case ErrorCode.WRN_BadRefCompareLeft:
		case ErrorCode.WRN_BadRefCompareRight:
		case ErrorCode.WRN_PatternIsAmbiguous:
		case ErrorCode.WRN_PatternStaticOrInaccessible:
		case ErrorCode.WRN_PatternBadSignature:
		case ErrorCode.WRN_SameFullNameThisNsAgg:
		case ErrorCode.WRN_SameFullNameThisAggAgg:
		case ErrorCode.WRN_SameFullNameThisAggNs:
		case ErrorCode.WRN_GlobalAliasDefn:
		case ErrorCode.WRN_AlwaysNull:
		case ErrorCode.WRN_CmpAlwaysFalse:
		case ErrorCode.WRN_GotoCaseShouldConvert:
		case ErrorCode.WRN_NubExprIsConstBool:
		case ErrorCode.WRN_ExplicitImplCollision:
		case ErrorCode.WRN_DeprecatedSymbolStr:
		case ErrorCode.WRN_VacuousIntegralComp:
		case ErrorCode.WRN_AssignmentToLockOrDispose:
		case ErrorCode.WRN_DeprecatedCollectionInitAddStr:
		case ErrorCode.WRN_DeprecatedCollectionInitAdd:
		case ErrorCode.WRN_DuplicateParamTag:
		case ErrorCode.WRN_UnmatchedParamTag:
		case ErrorCode.WRN_UnprocessedXMLComment:
		case ErrorCode.WRN_InvalidSearchPathDir:
		case ErrorCode.WRN_UnifyReferenceMajMin:
		case ErrorCode.WRN_DuplicateTypeParamTag:
		case ErrorCode.WRN_UnmatchedTypeParamTag:
		case ErrorCode.WRN_UnmatchedParamRefTag:
		case ErrorCode.WRN_UnmatchedTypeParamRefTag:
		case ErrorCode.WRN_CantHaveManifestForModule:
		case ErrorCode.WRN_DynamicDispatchToConditionalMethod:
		case ErrorCode.WRN_NoSources:
		case ErrorCode.WRN_CLS_MeaninglessOnPrivateType:
		case ErrorCode.WRN_CLS_AssemblyNotCLS2:
		case ErrorCode.WRN_MainIgnored:
		case ErrorCode.WRN_UnqualifiedNestedTypeInCref:
		case ErrorCode.WRN_NoRuntimeMetadataVersion:
		case ErrorCode.WRN_NubExprIsConstBool2:
			return 2;
		case ErrorCode.WRN_IsAlwaysTrue:
		case ErrorCode.WRN_IsAlwaysFalse:
		case ErrorCode.WRN_ByRefNonAgileField:
		case ErrorCode.WRN_VolatileByRef:
		case ErrorCode.WRN_FinalizeMethod:
		case ErrorCode.WRN_DeprecatedSymbol:
		case ErrorCode.WRN_ExternMethodNoImplementation:
		case ErrorCode.WRN_AttributeLocationOnBadDeclaration:
		case ErrorCode.WRN_InvalidAttributeLocation:
		case ErrorCode.WRN_NonObsoleteOverridingObsolete:
		case ErrorCode.WRN_CoClassWithoutComImport:
		case ErrorCode.WRN_ObsoleteOverridingNonObsolete:
		case ErrorCode.WRN_ExternCtorNoImplementation:
		case ErrorCode.WRN_WarningDirective:
		case ErrorCode.WRN_UnreachableGeneralCatch:
		case ErrorCode.WRN_DefaultValueForUnconsumedLocation:
		case ErrorCode.WRN_IdentifierOrNumericLiteralExpected:
		case ErrorCode.WRN_EmptySwitch:
		case ErrorCode.WRN_XMLParseError:
		case ErrorCode.WRN_BadXMLRef:
		case ErrorCode.WRN_BadXMLRefParamType:
		case ErrorCode.WRN_BadXMLRefReturnType:
		case ErrorCode.WRN_BadXMLRefSyntax:
		case ErrorCode.WRN_FailedInclude:
		case ErrorCode.WRN_InvalidInclude:
		case ErrorCode.WRN_XMLParseIncludeError:
		case ErrorCode.WRN_ALinkWarn:
		case ErrorCode.WRN_CmdOptionConflictsSource:
		case ErrorCode.WRN_IllegalPragma:
		case ErrorCode.WRN_IllegalPPWarning:
		case ErrorCode.WRN_BadRestoreNumber:
		case ErrorCode.WRN_NonECMAFeature:
		case ErrorCode.WRN_ErrorOverride:
		case ErrorCode.WRN_MultiplePredefTypes:
		case ErrorCode.WRN_TooManyLinesForDebugger:
		case ErrorCode.WRN_CallOnNonAgileField:
		case ErrorCode.WRN_InvalidNumber:
		case ErrorCode.WRN_IllegalPPChecksum:
		case ErrorCode.WRN_EndOfPPLineExpected:
		case ErrorCode.WRN_ConflictingChecksum:
		case ErrorCode.WRN_DotOnDefault:
		case ErrorCode.WRN_BadXMLRefTypeVar:
		case ErrorCode.WRN_ReferencedAssemblyReferencesLinkedPIA:
		case ErrorCode.WRN_MultipleRuntimeImplementationMatches:
		case ErrorCode.WRN_MultipleRuntimeOverrideMatches:
		case ErrorCode.WRN_AsyncLacksAwaits:
		case ErrorCode.WRN_FileAlreadyIncluded:
		case ErrorCode.WRN_NoConfigNotOnCommandLine:
		case ErrorCode.WRN_DefineIdentifierRequired:
		case ErrorCode.WRN_BadUILang:
		case ErrorCode.WRN_CLS_NoVarArgs:
		case ErrorCode.WRN_CLS_BadArgType:
		case ErrorCode.WRN_CLS_BadReturnType:
		case ErrorCode.WRN_CLS_BadFieldPropType:
		case ErrorCode.WRN_CLS_BadIdentifierCase:
		case ErrorCode.WRN_CLS_OverloadRefOut:
		case ErrorCode.WRN_CLS_OverloadUnnamed:
		case ErrorCode.WRN_CLS_BadIdentifier:
		case ErrorCode.WRN_CLS_BadBase:
		case ErrorCode.WRN_CLS_BadInterfaceMember:
		case ErrorCode.WRN_CLS_NoAbstractMembers:
		case ErrorCode.WRN_CLS_NotOnModules:
		case ErrorCode.WRN_CLS_ModuleMissingCLS:
		case ErrorCode.WRN_CLS_AssemblyNotCLS:
		case ErrorCode.WRN_CLS_BadAttributeType:
		case ErrorCode.WRN_CLS_ArrayArgumentToAttribute:
		case ErrorCode.WRN_CLS_NotOnModules2:
		case ErrorCode.WRN_CLS_IllegalTrueInFalse:
		case ErrorCode.WRN_CLS_MeaninglessOnParam:
		case ErrorCode.WRN_CLS_MeaninglessOnReturn:
		case ErrorCode.WRN_CLS_BadTypeVar:
		case ErrorCode.WRN_CLS_VolatileField:
		case ErrorCode.WRN_CLS_BadInterface:
		case ErrorCode.WRN_UnobservedAwaitableExpression:
		case ErrorCode.WRN_CallerLineNumberParamForUnconsumedLocation:
		case ErrorCode.WRN_CallerFilePathParamForUnconsumedLocation:
		case ErrorCode.WRN_CallerMemberNameParamForUnconsumedLocation:
		case ErrorCode.WRN_DelaySignButNoKey:
		case ErrorCode.WRN_CallerFilePathPreferredOverCallerMemberName:
		case ErrorCode.WRN_CallerLineNumberPreferredOverCallerMemberName:
		case ErrorCode.WRN_CallerLineNumberPreferredOverCallerFilePath:
		case ErrorCode.WRN_AssemblyAttributeFromModuleIsOverridden:
		case ErrorCode.WRN_FilterIsConstantTrue:
		case ErrorCode.WRN_UnimplementedCommandLineSwitch:
		case ErrorCode.WRN_ReferencedAssemblyDoesNotHaveStrongName:
		case ErrorCode.WRN_RefCultureMismatch:
		case ErrorCode.WRN_ConflictingMachineAssembly:
		case ErrorCode.WRN_AnalyzerCannotBeCreated:
		case ErrorCode.WRN_NoAnalyzerInAssembly:
		case ErrorCode.WRN_UnableToLoadAnalyzer:
		case ErrorCode.WRN_AlignmentMagnitude:
		case ErrorCode.WRN_AttributeIgnoredWhenPublicSigning:
		case ErrorCode.WRN_TupleLiteralNameMismatch:
		case ErrorCode.WRN_Experimental:
		case ErrorCode.WRN_FilterIsConstantFalse:
		case ErrorCode.WRN_FilterIsConstantFalseRedundantTryCatch:
		case ErrorCode.WRN_AttributesOnBackingFieldsNotAvailable:
		case ErrorCode.WRN_TupleBinopLiteralNameMismatch:
		case ErrorCode.WRN_TypeParameterSameAsOuterMethodTypeParameter:
			return 1;
		default:
			return 0;
		}
	}

	internal static bool PreventsSuccessfulDelegateConversion(ErrorCode code)
	{
		if (code == ErrorCode.Void || code == ErrorCode.Unknown)
		{
			return false;
		}
		if (IsWarning(code))
		{
			return false;
		}
		switch (code)
		{
		case ErrorCode.ERR_DuplicateParamName:
		case ErrorCode.ERR_LocalDuplicate:
		case ErrorCode.ERR_LocalIllegallyOverrides:
		case ErrorCode.ERR_LocalSameNameAsTypeParam:
		case ErrorCode.ERR_DeprecatedSymbolStr:
		case ErrorCode.ERR_DeprecatedCollectionInitAddStr:
		case ErrorCode.ERR_QueryRangeVariableOverrides:
		case ErrorCode.ERR_QueryRangeVariableSameAsTypeParam:
			return false;
		default:
			return true;
		}
	}

	internal static bool PreventsSuccessfulDelegateConversion(DiagnosticBag diagnostics)
	{
		foreach (Diagnostic item in diagnostics.AsEnumerable())
		{
			if (PreventsSuccessfulDelegateConversion((ErrorCode)item.Code))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool PreventsSuccessfulDelegateConversion(ImmutableArray<Diagnostic> diagnostics)
	{
		foreach (Diagnostic item in diagnostics)
		{
			if (PreventsSuccessfulDelegateConversion((ErrorCode)item.Code))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsWarning(ErrorCode code)
	{
		if (code <= ErrorCode.WRN_ALinkWarn)
		{
			switch (code)
			{
			case ErrorCode.WRN_InvalidMainSig:
			case ErrorCode.WRN_UnreferencedEvent:
			case ErrorCode.WRN_LowercaseEllSuffix:
			case ErrorCode.WRN_DuplicateUsing:
			case ErrorCode.WRN_NewRequired:
			case ErrorCode.WRN_NewNotRequired:
			case ErrorCode.WRN_NewOrOverrideExpected:
			case ErrorCode.WRN_UnreachableCode:
			case ErrorCode.WRN_UnreferencedLabel:
			case ErrorCode.WRN_UnreferencedVar:
			case ErrorCode.WRN_UnreferencedField:
			case ErrorCode.WRN_IsAlwaysTrue:
			case ErrorCode.WRN_IsAlwaysFalse:
			case ErrorCode.WRN_ByRefNonAgileField:
			case ErrorCode.WRN_UnreferencedVarAssg:
			case ErrorCode.WRN_NegativeArrayIndex:
			case ErrorCode.WRN_BadRefCompareLeft:
			case ErrorCode.WRN_BadRefCompareRight:
			case ErrorCode.WRN_PatternIsAmbiguous:
			case ErrorCode.WRN_PatternStaticOrInaccessible:
			case ErrorCode.WRN_PatternBadSignature:
			case ErrorCode.WRN_SequentialOnPartialClass:
			case ErrorCode.WRN_MainCantBeGeneric:
			case ErrorCode.WRN_UnreferencedFieldAssg:
			case ErrorCode.WRN_AmbiguousXMLReference:
			case ErrorCode.WRN_VolatileByRef:
			case ErrorCode.WRN_SameFullNameThisNsAgg:
			case ErrorCode.WRN_SameFullNameThisAggAgg:
			case ErrorCode.WRN_SameFullNameThisAggNs:
			case ErrorCode.WRN_GlobalAliasDefn:
			case ErrorCode.WRN_AlwaysNull:
			case ErrorCode.WRN_CmpAlwaysFalse:
			case ErrorCode.WRN_FinalizeMethod:
			case ErrorCode.WRN_GotoCaseShouldConvert:
			case ErrorCode.WRN_NubExprIsConstBool:
			case ErrorCode.WRN_ExplicitImplCollision:
			case ErrorCode.WRN_DeprecatedSymbol:
			case ErrorCode.WRN_DeprecatedSymbolStr:
			case ErrorCode.WRN_ExternMethodNoImplementation:
			case ErrorCode.WRN_ProtectedInSealed:
			case ErrorCode.WRN_PossibleMistakenNullStatement:
			case ErrorCode.WRN_UnassignedInternalField:
			case ErrorCode.WRN_VacuousIntegralComp:
			case ErrorCode.WRN_AttributeLocationOnBadDeclaration:
			case ErrorCode.WRN_InvalidAttributeLocation:
			case ErrorCode.WRN_EqualsWithoutGetHashCode:
			case ErrorCode.WRN_EqualityOpWithoutEquals:
			case ErrorCode.WRN_EqualityOpWithoutGetHashCode:
			case ErrorCode.WRN_IncorrectBooleanAssg:
			case ErrorCode.WRN_NonObsoleteOverridingObsolete:
			case ErrorCode.WRN_BitwiseOrSignExtend:
			case ErrorCode.WRN_CoClassWithoutComImport:
			case ErrorCode.WRN_TypeParameterSameAsOuterTypeParameter:
			case ErrorCode.WRN_AssignmentToLockOrDispose:
			case ErrorCode.WRN_ObsoleteOverridingNonObsolete:
			case ErrorCode.WRN_DebugFullNameTooLong:
			case ErrorCode.WRN_ExternCtorNoImplementation:
			case ErrorCode.WRN_WarningDirective:
			case ErrorCode.WRN_UnreachableGeneralCatch:
			case ErrorCode.WRN_DeprecatedCollectionInitAddStr:
			case ErrorCode.WRN_DeprecatedCollectionInitAdd:
			case ErrorCode.WRN_DefaultValueForUnconsumedLocation:
			case ErrorCode.WRN_IdentifierOrNumericLiteralExpected:
			case ErrorCode.WRN_EmptySwitch:
			case ErrorCode.WRN_XMLParseError:
			case ErrorCode.WRN_DuplicateParamTag:
			case ErrorCode.WRN_UnmatchedParamTag:
			case ErrorCode.WRN_MissingParamTag:
			case ErrorCode.WRN_BadXMLRef:
			case ErrorCode.WRN_BadXMLRefParamType:
			case ErrorCode.WRN_BadXMLRefReturnType:
			case ErrorCode.WRN_BadXMLRefSyntax:
			case ErrorCode.WRN_UnprocessedXMLComment:
			case ErrorCode.WRN_FailedInclude:
			case ErrorCode.WRN_InvalidInclude:
			case ErrorCode.WRN_MissingXMLComment:
			case ErrorCode.WRN_XMLParseIncludeError:
			case ErrorCode.WRN_ALinkWarn:
				break;
			default:
				goto IL_070c;
			}
		}
		else
		{
			switch (code)
			{
			case ErrorCode.WRN_CmdOptionConflictsSource:
			case ErrorCode.WRN_IllegalPragma:
			case ErrorCode.WRN_IllegalPPWarning:
			case ErrorCode.WRN_BadRestoreNumber:
			case ErrorCode.WRN_NonECMAFeature:
			case ErrorCode.WRN_ErrorOverride:
			case ErrorCode.WRN_InvalidSearchPathDir:
			case ErrorCode.WRN_MultiplePredefTypes:
			case ErrorCode.WRN_TooManyLinesForDebugger:
			case ErrorCode.WRN_CallOnNonAgileField:
			case ErrorCode.WRN_InvalidNumber:
			case ErrorCode.WRN_IllegalPPChecksum:
			case ErrorCode.WRN_EndOfPPLineExpected:
			case ErrorCode.WRN_ConflictingChecksum:
			case ErrorCode.WRN_InvalidAssemblyName:
			case ErrorCode.WRN_UnifyReferenceMajMin:
			case ErrorCode.WRN_UnifyReferenceBldRev:
			case ErrorCode.WRN_DuplicateTypeParamTag:
			case ErrorCode.WRN_UnmatchedTypeParamTag:
			case ErrorCode.WRN_MissingTypeParamTag:
			case ErrorCode.WRN_AssignmentToSelf:
			case ErrorCode.WRN_ComparisonToSelf:
			case ErrorCode.WRN_DotOnDefault:
			case ErrorCode.WRN_BadXMLRefTypeVar:
			case ErrorCode.WRN_UnmatchedParamRefTag:
			case ErrorCode.WRN_UnmatchedTypeParamRefTag:
			case ErrorCode.WRN_ReferencedAssemblyReferencesLinkedPIA:
			case ErrorCode.WRN_CantHaveManifestForModule:
			case ErrorCode.WRN_MultipleRuntimeImplementationMatches:
			case ErrorCode.WRN_MultipleRuntimeOverrideMatches:
			case ErrorCode.WRN_DynamicDispatchToConditionalMethod:
			case ErrorCode.WRN_IsDynamicIsConfusing:
			case ErrorCode.WRN_AsyncLacksAwaits:
			case ErrorCode.WRN_FileAlreadyIncluded:
			case ErrorCode.WRN_NoSources:
			case ErrorCode.WRN_NoConfigNotOnCommandLine:
			case ErrorCode.WRN_DefineIdentifierRequired:
			case ErrorCode.WRN_BadUILang:
			case ErrorCode.WRN_CLS_NoVarArgs:
			case ErrorCode.WRN_CLS_BadArgType:
			case ErrorCode.WRN_CLS_BadReturnType:
			case ErrorCode.WRN_CLS_BadFieldPropType:
			case ErrorCode.WRN_CLS_BadIdentifierCase:
			case ErrorCode.WRN_CLS_OverloadRefOut:
			case ErrorCode.WRN_CLS_OverloadUnnamed:
			case ErrorCode.WRN_CLS_BadIdentifier:
			case ErrorCode.WRN_CLS_BadBase:
			case ErrorCode.WRN_CLS_BadInterfaceMember:
			case ErrorCode.WRN_CLS_NoAbstractMembers:
			case ErrorCode.WRN_CLS_NotOnModules:
			case ErrorCode.WRN_CLS_ModuleMissingCLS:
			case ErrorCode.WRN_CLS_AssemblyNotCLS:
			case ErrorCode.WRN_CLS_BadAttributeType:
			case ErrorCode.WRN_CLS_ArrayArgumentToAttribute:
			case ErrorCode.WRN_CLS_NotOnModules2:
			case ErrorCode.WRN_CLS_IllegalTrueInFalse:
			case ErrorCode.WRN_CLS_MeaninglessOnPrivateType:
			case ErrorCode.WRN_CLS_AssemblyNotCLS2:
			case ErrorCode.WRN_CLS_MeaninglessOnParam:
			case ErrorCode.WRN_CLS_MeaninglessOnReturn:
			case ErrorCode.WRN_CLS_BadTypeVar:
			case ErrorCode.WRN_CLS_VolatileField:
			case ErrorCode.WRN_CLS_BadInterface:
			case ErrorCode.WRN_UnobservedAwaitableExpression:
			case ErrorCode.WRN_CallerLineNumberParamForUnconsumedLocation:
			case ErrorCode.WRN_CallerFilePathParamForUnconsumedLocation:
			case ErrorCode.WRN_CallerMemberNameParamForUnconsumedLocation:
			case ErrorCode.WRN_MainIgnored:
			case ErrorCode.WRN_DelaySignButNoKey:
			case ErrorCode.WRN_InvalidVersionFormat:
			case ErrorCode.WRN_CallerFilePathPreferredOverCallerMemberName:
			case ErrorCode.WRN_CallerLineNumberPreferredOverCallerMemberName:
			case ErrorCode.WRN_CallerLineNumberPreferredOverCallerFilePath:
			case ErrorCode.WRN_AssemblyAttributeFromModuleIsOverridden:
			case ErrorCode.WRN_FilterIsConstantTrue:
			case ErrorCode.WRN_UnimplementedCommandLineSwitch:
			case ErrorCode.WRN_ReferencedAssemblyDoesNotHaveStrongName:
			case ErrorCode.WRN_RefCultureMismatch:
			case ErrorCode.WRN_ConflictingMachineAssembly:
			case ErrorCode.WRN_UnqualifiedNestedTypeInCref:
			case ErrorCode.WRN_NoRuntimeMetadataVersion:
			case ErrorCode.WRN_PdbLocalNameTooLong:
			case ErrorCode.WRN_AnalyzerCannotBeCreated:
			case ErrorCode.WRN_NoAnalyzerInAssembly:
			case ErrorCode.WRN_UnableToLoadAnalyzer:
			case ErrorCode.WRN_NubExprIsConstBool2:
			case ErrorCode.WRN_AlignmentMagnitude:
			case ErrorCode.WRN_AttributeIgnoredWhenPublicSigning:
			case ErrorCode.WRN_TupleLiteralNameMismatch:
			case ErrorCode.WRN_Experimental:
			case ErrorCode.WRN_UnreferencedLocalFunction:
			case ErrorCode.WRN_FilterIsConstantFalse:
			case ErrorCode.WRN_FilterIsConstantFalseRedundantTryCatch:
			case ErrorCode.WRN_AttributesOnBackingFieldsNotAvailable:
			case ErrorCode.WRN_TupleBinopLiteralNameMismatch:
			case ErrorCode.WRN_TypeParameterSameAsOuterMethodTypeParameter:
				break;
			default:
				goto IL_070c;
			}
		}
		return true;
		IL_070c:
		return false;
	}

	public static bool IsFatal(ErrorCode code)
	{
		switch (code)
		{
		case ErrorCode.FTL_MetadataCantOpenFile:
		case ErrorCode.FTL_DebugEmitFailure:
		case ErrorCode.FTL_BadCodepage:
		case ErrorCode.FTL_InvalidTarget:
		case ErrorCode.FTL_InvalidInputFileName:
		case ErrorCode.FTL_OutputFileExists:
		case ErrorCode.FTL_BadChecksumAlgorithm:
			return true;
		default:
			return false;
		}
	}

	public static bool IsInfo(ErrorCode code)
	{
		if (code == ErrorCode.INF_UnableToLoadSomeTypesInAnalyzer)
		{
			return true;
		}
		return false;
	}

	public static bool IsHidden(ErrorCode code)
	{
		if ((uint)(code - 8019) <= 1u)
		{
			return true;
		}
		return false;
	}
}
