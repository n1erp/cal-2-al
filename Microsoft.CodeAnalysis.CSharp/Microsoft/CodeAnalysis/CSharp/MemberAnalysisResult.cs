using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct MemberAnalysisResult
{
	public readonly ImmutableArray<Conversion> ConversionsOpt;

	public readonly ImmutableArray<int> BadArgumentsOpt;

	public readonly ImmutableArray<int> ArgsToParamsOpt;

	public readonly ImmutableArray<TypeParameterDiagnosticInfo> ConstraintFailureDiagnostics;

	public readonly int BadParameter;

	public readonly MemberResolutionKind Kind;

	public readonly bool HasAnyRefOmittedArgument;

	public bool IsApplicable
	{
		get
		{
			MemberResolutionKind kind = Kind;
			if (kind - 1 <= MemberResolutionKind.ApplicableInNormalForm || kind - 21 <= MemberResolutionKind.ApplicableInNormalForm)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsValid
	{
		get
		{
			MemberResolutionKind kind = Kind;
			if (kind - 1 <= MemberResolutionKind.ApplicableInNormalForm)
			{
				return true;
			}
			return false;
		}
	}

	private MemberAnalysisResult(MemberResolutionKind kind, ImmutableArray<int> badArgumentsOpt = default(ImmutableArray<int>), ImmutableArray<int> argsToParamsOpt = default(ImmutableArray<int>), ImmutableArray<Conversion> conversionsOpt = default(ImmutableArray<Conversion>), int missingParameter = -1, bool hasAnyRefOmittedArgument = false, ImmutableArray<TypeParameterDiagnosticInfo> constraintFailureDiagnosticsOpt = default(ImmutableArray<TypeParameterDiagnosticInfo>))
	{
		Kind = kind;
		BadArgumentsOpt = badArgumentsOpt;
		ArgsToParamsOpt = argsToParamsOpt;
		ConversionsOpt = conversionsOpt;
		BadParameter = missingParameter;
		HasAnyRefOmittedArgument = hasAnyRefOmittedArgument;
		ConstraintFailureDiagnostics = constraintFailureDiagnosticsOpt.NullToEmpty();
	}

	public override bool Equals(object obj)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable;
	}

	public Conversion ConversionForArg(int arg)
	{
		if (ConversionsOpt.IsDefault)
		{
			return Conversion.Identity;
		}
		return ConversionsOpt[arg];
	}

	public int ParameterFromArgument(int arg)
	{
		if (ArgsToParamsOpt.IsDefault)
		{
			return arg;
		}
		return ArgsToParamsOpt[arg];
	}

	internal bool HasUseSiteDiagnosticToReportFor(Symbol symbol)
	{
		if (!SuppressUseSiteDiagnosticsForKind(Kind) && (object)symbol != null)
		{
			return symbol.GetUseSiteDiagnostic() != null;
		}
		return false;
	}

	private static bool SuppressUseSiteDiagnosticsForKind(MemberResolutionKind kind)
	{
		switch (kind)
		{
		case MemberResolutionKind.UnsupportedMetadata:
			return true;
		case MemberResolutionKind.NoCorrespondingParameter:
		case MemberResolutionKind.NoCorrespondingNamedParameter:
		case MemberResolutionKind.DuplicateNamedArgument:
		case MemberResolutionKind.RequiredParameterMissing:
		case MemberResolutionKind.NameUsedForPositional:
		case MemberResolutionKind.LessDerived:
			return true;
		default:
			return false;
		}
	}

	public static MemberAnalysisResult ArgumentParameterMismatch(ArgumentAnalysisResult argAnalysis)
	{
		return argAnalysis.Kind switch
		{
			ArgumentAnalysisResultKind.NoCorrespondingParameter => NoCorrespondingParameter(argAnalysis.ArgumentPosition), 
			ArgumentAnalysisResultKind.NoCorrespondingNamedParameter => NoCorrespondingNamedParameter(argAnalysis.ArgumentPosition), 
			ArgumentAnalysisResultKind.DuplicateNamedArgument => DuplicateNamedArgument(argAnalysis.ArgumentPosition), 
			ArgumentAnalysisResultKind.RequiredParameterMissing => RequiredParameterMissing(argAnalysis.ParameterPosition), 
			ArgumentAnalysisResultKind.NameUsedForPositional => NameUsedForPositional(argAnalysis.ArgumentPosition), 
			ArgumentAnalysisResultKind.BadNonTrailingNamedArgument => BadNonTrailingNamedArgument(argAnalysis.ArgumentPosition), 
			_ => throw ExceptionUtilities.UnexpectedValue(argAnalysis.Kind), 
		};
	}

	public static MemberAnalysisResult NameUsedForPositional(int argumentPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.NameUsedForPositional, ImmutableArray.Create(argumentPosition));
	}

	public static MemberAnalysisResult BadNonTrailingNamedArgument(int argumentPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.BadNonTrailingNamedArgument, ImmutableArray.Create(argumentPosition));
	}

	public static MemberAnalysisResult NoCorrespondingParameter(int argumentPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.NoCorrespondingParameter, ImmutableArray.Create(argumentPosition));
	}

	public static MemberAnalysisResult NoCorrespondingNamedParameter(int argumentPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.NoCorrespondingNamedParameter, ImmutableArray.Create(argumentPosition));
	}

	public static MemberAnalysisResult DuplicateNamedArgument(int argumentPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.DuplicateNamedArgument, ImmutableArray.Create(argumentPosition));
	}

	public static MemberAnalysisResult RequiredParameterMissing(int parameterPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.RequiredParameterMissing, default, default, default, parameterPosition);
	}

	public static MemberAnalysisResult UseSiteError()
	{
		return new MemberAnalysisResult(MemberResolutionKind.UseSiteError);
	}

	public static MemberAnalysisResult UnsupportedMetadata()
	{
		return new MemberAnalysisResult(MemberResolutionKind.UnsupportedMetadata);
	}

	public static MemberAnalysisResult BadArgumentConversions(ImmutableArray<int> argsToParamsOpt, ImmutableArray<int> badArguments, ImmutableArray<Conversion> conversions)
	{
		return new MemberAnalysisResult(MemberResolutionKind.BadArgumentConversion, badArguments, argsToParamsOpt, conversions);
	}

	public static MemberAnalysisResult InaccessibleTypeArgument()
	{
		return new MemberAnalysisResult(MemberResolutionKind.InaccessibleTypeArgument);
	}

	public static MemberAnalysisResult TypeInferenceFailed()
	{
		return new MemberAnalysisResult(MemberResolutionKind.TypeInferenceFailed);
	}

	public static MemberAnalysisResult TypeInferenceExtensionInstanceArgumentFailed()
	{
		return new MemberAnalysisResult(MemberResolutionKind.TypeInferenceExtensionInstanceArgument);
	}

	public static MemberAnalysisResult StaticInstanceMismatch()
	{
		return new MemberAnalysisResult(MemberResolutionKind.StaticInstanceMismatch);
	}

	public static MemberAnalysisResult ConstructedParameterFailedConstraintsCheck(int parameterPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.ConstructedParameterFailedConstraintCheck, default, default, default, parameterPosition);
	}

	public static MemberAnalysisResult WrongRefKind()
	{
		return new MemberAnalysisResult(MemberResolutionKind.WrongRefKind);
	}

	public static MemberAnalysisResult WrongReturnType()
	{
		return new MemberAnalysisResult(MemberResolutionKind.WrongReturnType);
	}

	public static MemberAnalysisResult LessDerived()
	{
		return new MemberAnalysisResult(MemberResolutionKind.LessDerived);
	}

	public static MemberAnalysisResult NormalForm(ImmutableArray<int> argsToParamsOpt, ImmutableArray<Conversion> conversions, bool hasAnyRefOmittedArgument)
	{
		return new MemberAnalysisResult(MemberResolutionKind.ApplicableInNormalForm, default, argsToParamsOpt, conversions, -1, hasAnyRefOmittedArgument);
	}

	public static MemberAnalysisResult ExpandedForm(ImmutableArray<int> argsToParamsOpt, ImmutableArray<Conversion> conversions, bool hasAnyRefOmittedArgument)
	{
		return new MemberAnalysisResult(MemberResolutionKind.ApplicableInExpandedForm, default, argsToParamsOpt, conversions, -1, hasAnyRefOmittedArgument);
	}

	public static MemberAnalysisResult Worse()
	{
		return new MemberAnalysisResult(MemberResolutionKind.Worse);
	}

	public static MemberAnalysisResult Worst()
	{
		return new MemberAnalysisResult(MemberResolutionKind.Worst);
	}

	internal static MemberAnalysisResult ConstraintFailure(ImmutableArray<TypeParameterDiagnosticInfo> constraintFailureDiagnostics)
	{
		return new MemberAnalysisResult(MemberResolutionKind.ConstraintFailure, default, default, default, -1, hasAnyRefOmittedArgument: false, constraintFailureDiagnostics);
	}
}
