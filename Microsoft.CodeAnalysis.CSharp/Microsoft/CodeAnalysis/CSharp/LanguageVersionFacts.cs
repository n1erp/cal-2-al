using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public static class LanguageVersionFacts
{
	public static string ToDisplayString(this LanguageVersion version)
	{
		return version switch
		{
			LanguageVersion.CSharp1 => "1", 
			LanguageVersion.CSharp2 => "2", 
			LanguageVersion.CSharp3 => "3", 
			LanguageVersion.CSharp4 => "4", 
			LanguageVersion.CSharp5 => "5", 
			LanguageVersion.CSharp6 => "6", 
			LanguageVersion.CSharp7 => "7.0", 
			LanguageVersion.CSharp7_1 => "7.1", 
			LanguageVersion.CSharp7_2 => "7.2", 
			LanguageVersion.CSharp7_3 => "7.3", 
			LanguageVersion.Default => "default", 
			LanguageVersion.Latest => "latest", 
			_ => throw ExceptionUtilities.UnexpectedValue(version), 
		};
	}

	public static bool TryParse(string version, out LanguageVersion result)
	{
		if (version == null)
		{
			result = LanguageVersion.Default;
			return true;
		}
		switch (CaseInsensitiveComparison.ToLower(version))
		{
		case "default":
			result = LanguageVersion.Default;
			return true;
		case "latest":
			result = LanguageVersion.Latest;
			return true;
		case "1":
		case "1.0":
		case "iso-1":
			result = LanguageVersion.CSharp1;
			return true;
		case "2":
		case "2.0":
		case "iso-2":
			result = LanguageVersion.CSharp2;
			return true;
		case "3":
		case "3.0":
			result = LanguageVersion.CSharp3;
			return true;
		case "4":
		case "4.0":
			result = LanguageVersion.CSharp4;
			return true;
		case "5":
		case "5.0":
			result = LanguageVersion.CSharp5;
			return true;
		case "6":
		case "6.0":
			result = LanguageVersion.CSharp6;
			return true;
		case "7":
		case "7.0":
			result = LanguageVersion.CSharp7;
			return true;
		case "7.1":
			result = LanguageVersion.CSharp7_1;
			return true;
		case "7.2":
			result = LanguageVersion.CSharp7_2;
			return true;
		case "7.3":
			result = LanguageVersion.CSharp7_3;
			return true;
		default:
			result = LanguageVersion.Default;
			return false;
		}
	}

	public static LanguageVersion MapSpecifiedToEffectiveVersion(this LanguageVersion version)
	{
		return version switch
		{
			LanguageVersion.Latest => LanguageVersion.CSharp7_3, 
			LanguageVersion.Default => LanguageVersion.CSharp7, 
			_ => version, 
		};
	}

	internal static bool DisallowInferredTupleElementNames(this LanguageVersion self)
	{
		return self < MessageID.IDS_FeatureInferredTupleNames.RequiredVersion();
	}

	internal static bool AllowNonTrailingNamedArguments(this LanguageVersion self)
	{
		return self >= MessageID.IDS_FeatureNonTrailingNamedArguments.RequiredVersion();
	}

	internal static bool AllowAttributesOnBackingFields(this LanguageVersion self)
	{
		return self >= MessageID.IDS_FeatureAttributesOnBackingFields.RequiredVersion();
	}

	internal static bool AllowImprovedOverloadCandidates(this LanguageVersion self)
	{
		return self >= MessageID.IDS_FeatureImprovedOverloadCandidates.RequiredVersion();
	}
}
