using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class ErrorFacts
{
	public static string WarningFutureErrorMessage = "This warning will become an error in a future release.";

	private const string TitleSuffix = "_Title";

	private const string DescriptionSuffix = "_Description";

	private static readonly Lazy<Dictionary<ErrorCode, string>> HelpLinksMap = new Lazy<Dictionary<ErrorCode, string>>(CreateHelpLinks);

	private static readonly Lazy<Dictionary<ErrorCode, string>> CategoriesMap = new Lazy<Dictionary<ErrorCode, string>>(CreateCategoriesMap);

	private static ResourceManager resourceManager;

	private static ResourceManager ResourceManager
	{
		get
		{
			if (resourceManager == null)
			{
				resourceManager = CodeAnalysisResources.ResourceManager;
			}
			return resourceManager;
		}
	}

	public static bool IsWarning(ErrorCode code)
	{
		switch (code)
		{
		case ErrorCode.WRN_DeprecatedSymbol:
		case ErrorCode.WRN_PropertyIsObsolete:
		case ErrorCode.WRN_EmptyConstIsObsolete:
		case ErrorCode.WRN_PermissionObjectIsMissing:
		case ErrorCode.WRN_SortingFieldsShouldBePartOfKey:
		case ErrorCode.WRN_SortingFieldShouldBePartOfKey:
		case ErrorCode.WRN_ERR_PagePartMustBePart:
		case ErrorCode.WRN_ReuseOfAreaName:
		case ErrorCode.WRN_ERR_MemberNameOnlyAllowedForTriggers:
		case ErrorCode.WRN_DeprecatedIdSyntax:
		case ErrorCode.WRN_ObsoleteMultilanguageSyntax:
		case ErrorCode.WRN_ObsoleteStatePending:
		case ErrorCode.WRN_IncorrectLabelSyntax:
		case ErrorCode.WRN_FileIsReadonly:
		case ErrorCode.WRN_ERR_FieldNameIdentifierTooLong:
		case ErrorCode.WRN_TranslationItemSourceStringMismatch:
		case ErrorCode.WRN_TranslationItemTranslatedStringTooLong:
		case ErrorCode.WRN_DuplicateTranslationItemForSameId:
		case ErrorCode.WRN_ERR_FieldImageOnlyOnFieldInCueGroup:
		case ErrorCode.WRN_ImageNotValid:
		case ErrorCode.WRN_ImageOnNestedActionGroupInSectionsArea:
		case ErrorCode.WRN_ERR_DuplicateMemberName:
		case ErrorCode.WRN_ERR_ActionInGroupInSectionsAreaShouldOnlyRunListPages:
		case ErrorCode.WRN_ConstantValueNotAValidOptionValue:
		case ErrorCode.WRN_ObsoleteStateObsolete:
		case ErrorCode.WRN_MemberAlreadyExists:
		case ErrorCode.WRN_MemberAlreadyDeclaredInBase:
		case ErrorCode.WRN_ERR_KeyNameIdentifierTooLong:
		case ErrorCode.WRN_ERR_AreaNotValidOnPageOfType:
		case ErrorCode.WRN_ERR_InvalidInClientExpression:
		case ErrorCode.WRN_TableUsingReservedFieldId:
		case ErrorCode.WRN_TableUsingReservedFieldName:
		case ErrorCode.WRN_NoConfigNotOnCommandLine:
		case ErrorCode.WRN_AnalyzerCannotBeCreated:
		case ErrorCode.WRN_NoAnalyzerInAssembly:
		case ErrorCode.WRN_UnableToLoadAnalyzer:
		case ErrorCode.WRN_FileAlreadyIncluded:
		case ErrorCode.WRN_FileDoesNotMatchAnyFormat:
		case ErrorCode.WRN_XmlValidationErrorOccurred:
		case ErrorCode.WRN_TranslationFileTargetLanguageInvalid:
		case ErrorCode.WRN_TranslationFileTargetLanguageMissing:
			return true;
		default:
			return false;
		}
	}

	public static bool IsWarningFutureError(ErrorCode code)
	{
		switch (code)
		{
		case ErrorCode.WRN_ERR_PagePartMustBePart:
		case ErrorCode.WRN_ERR_MemberNameOnlyAllowedForTriggers:
		case ErrorCode.WRN_ERR_FieldNameIdentifierTooLong:
		case ErrorCode.WRN_ERR_FieldImageOnlyOnFieldInCueGroup:
		case ErrorCode.WRN_ERR_DuplicateMemberName:
		case ErrorCode.WRN_ERR_ActionInGroupInSectionsAreaShouldOnlyRunListPages:
		case ErrorCode.WRN_ERR_KeyNameIdentifierTooLong:
		case ErrorCode.WRN_ERR_AreaNotValidOnPageOfType:
		case ErrorCode.WRN_ERR_InvalidInClientExpression:
			return true;
		default:
			return false;
		}
	}

	public static bool IsFatal(ErrorCode code)
	{
		if ((uint)(code - 1100) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsInfo(ErrorCode code)
	{
		if (code == ErrorCode.INF_FileMatchesADefinition || code == ErrorCode.INF_SuccessfullyIncludedTranslationsForLanguages || (uint)(code - 1403) <= 4u)
		{
			return true;
		}
		return false;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801: ReviewUnusedParameters", MessageId = "code")]
	public static bool IsHidden(ErrorCode code)
	{
		return false;
	}

	private static Dictionary<ErrorCode, string> CreateHelpLinks()
	{
		return new Dictionary<ErrorCode, string>();
	}

	private static Dictionary<ErrorCode, string> CreateCategoriesMap()
	{
		return new Dictionary<ErrorCode, string>();
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
		string text = ResourceManager.GetString(code.ToString(), culture);
		DebugAssertHelper.Assert(!string.IsNullOrEmpty(text));
		return text;
	}

	public static string GetMessage(Feature feature, CultureInfo culture)
	{
		string text = ResourceManager.GetString("Feature_" + feature, culture);
		DebugAssertHelper.Assert(!string.IsNullOrEmpty(text));
		return text;
	}

	public static string GetMessage(ErrorCode code, CultureInfo culture)
	{
		string text = ResourceManager.GetString(code.ToString(), culture);
		if (IsWarningFutureError(code))
		{
			text = text + " " + WarningFutureErrorMessage;
		}
		return text;
	}

	public static LocalizableResourceString GetMessageFormat(ErrorCode code)
	{
		return new LocalizableResourceString(code.ToString(), ResourceManager, typeof(ErrorFacts));
	}

	public static LocalizableResourceString GetTitle(ErrorCode code)
	{
		return new LocalizableResourceString(string.Concat(code, "_Title"), ResourceManager, typeof(ErrorFacts));
	}

	public static LocalizableResourceString GetDescription(ErrorCode code)
	{
		return new LocalizableResourceString(string.Concat(code, "_Description"), ResourceManager, typeof(ErrorFacts));
	}

	public static string GetHelpLink(ErrorCode code)
	{
		if (HelpLinksMap.Value.TryGetValue(code, out var value))
		{
			return value;
		}
		return string.Empty;
	}

	public static string GetCategory(ErrorCode code)
	{
		if (CategoriesMap.Value.TryGetValue(code, out var value))
		{
			return value;
		}
		return "Compiler";
	}

	internal static int GetWarningLevel(ErrorCode code)
	{
		if (!IsInfo(code))
		{
			IsHidden(code);
		}
		return 4;
	}
}
