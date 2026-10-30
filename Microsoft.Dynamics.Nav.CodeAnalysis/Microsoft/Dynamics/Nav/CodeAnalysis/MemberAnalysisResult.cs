using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[SuppressMessage("Performance", "RS0008", Justification = "Equality not actually implemented")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal struct MemberAnalysisResult
{
	public readonly ImmutableArray<Conversion> ConversionsOpt;

	public readonly ImmutableArray<int> BadArgumentsOpt;

	public readonly EffectiveParameters EffectiveParameters;

	public readonly int BadParameter;

	public readonly MemberResolutionKind Kind;

	public bool IsApplicable
	{
		get
		{
			MemberResolutionKind kind = Kind;
			if (kind == MemberResolutionKind.Applicable || kind - 8 <= MemberResolutionKind.Applicable)
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
			if (kind == MemberResolutionKind.Applicable)
			{
				return true;
			}
			return false;
		}
	}

	private MemberAnalysisResult(MemberResolutionKind kind, EffectiveParameters effectiveParameters = null, ImmutableArray<int> badArgumentsOpt = default(ImmutableArray<int>), ImmutableArray<Conversion> conversionsOpt = default(ImmutableArray<Conversion>), int missingParameter = -1)
	{
		Kind = kind;
		EffectiveParameters = effectiveParameters;
		BadArgumentsOpt = badArgumentsOpt;
		ConversionsOpt = conversionsOpt;
		BadParameter = missingParameter;
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
		if (kind - 2 <= MemberResolutionKind.Applicable)
		{
			return true;
		}
		return false;
	}

	public static MemberAnalysisResult ArgumentParameterMismatch(ArgumentAnalysisResult argAnalysis)
	{
		return argAnalysis.Kind switch
		{
			ArgumentAnalysisResultKind.NoCorrespondingParameter => NoCorrespondingParameter(argAnalysis.EffectiveParameters, argAnalysis.ArgumentPosition), 
			ArgumentAnalysisResultKind.RequiredParameterMissing => RequiredParameterMissing(argAnalysis.EffectiveParameters, argAnalysis.ParameterPosition), 
			_ => throw ExceptionUtilities.UnexpectedValue(argAnalysis.Kind), 
		};
	}

	public static MemberAnalysisResult NoCorrespondingParameter(EffectiveParameters effectiveParameters, int argumentPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.NoCorrespondingParameter, effectiveParameters, ImmutableArray.Create(argumentPosition));
	}

	public static MemberAnalysisResult RequiredParameterMissing(EffectiveParameters effectiveParameters, int parameterPosition)
	{
		return new MemberAnalysisResult(MemberResolutionKind.RequiredParameterMissing, effectiveParameters, default, default, parameterPosition);
	}

	public static MemberAnalysisResult BadMembers(EffectiveParameters effectiveParameters, ImmutableArray<int> badMembers)
	{
		DebugAssertHelper.Assert(badMembers.Length != 0);
		return new MemberAnalysisResult(MemberResolutionKind.MemberRequired, effectiveParameters, badMembers);
	}

	public static MemberAnalysisResult BadTypeFromFirstArgumentConversions(EffectiveParameters effectiveParameters, ImmutableArray<int> badArguments, ImmutableArray<Conversion> conversions)
	{
		DebugAssertHelper.Assert(conversions.Length != 0);
		DebugAssertHelper.Assert(badArguments.Length != 0);
		return new MemberAnalysisResult(MemberResolutionKind.BadTypeFromFirstArguments, effectiveParameters, badArguments, conversions);
	}

	public static MemberAnalysisResult BadArgumentConversions(EffectiveParameters effectiveParameters, ImmutableArray<int> badArguments, ImmutableArray<Conversion> conversions)
	{
		DebugAssertHelper.Assert(conversions.Length != 0);
		DebugAssertHelper.Assert(badArguments.Length != 0);
		return new MemberAnalysisResult(MemberResolutionKind.BadArguments, effectiveParameters, badArguments, conversions);
	}

	public static MemberAnalysisResult NormalForm(EffectiveParameters effectiveParameters, ImmutableArray<Conversion> conversions = default(ImmutableArray<Conversion>))
	{
		return new MemberAnalysisResult(MemberResolutionKind.Applicable, effectiveParameters, default, conversions);
	}

	public static MemberAnalysisResult Worse()
	{
		return new MemberAnalysisResult(MemberResolutionKind.Worse);
	}

	public static MemberAnalysisResult Worst()
	{
		return new MemberAnalysisResult(MemberResolutionKind.Worst);
	}

	internal string GetDebuggerDisplay()
	{
		return string.Concat(Kind, " Parameters: ", EffectiveParameters?.GetDebuggerDisplay());
	}
}
