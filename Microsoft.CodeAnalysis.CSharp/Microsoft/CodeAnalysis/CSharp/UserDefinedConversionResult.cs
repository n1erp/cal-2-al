using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct UserDefinedConversionResult
{
	public readonly ImmutableArray<UserDefinedConversionAnalysis> Results;

	public readonly int Best;

	public readonly UserDefinedConversionResultKind Kind;

	public static UserDefinedConversionResult NoApplicableOperators(ImmutableArray<UserDefinedConversionAnalysis> results)
	{
		return new UserDefinedConversionResult(UserDefinedConversionResultKind.NoApplicableOperators, results, -1);
	}

	public static UserDefinedConversionResult NoBestSourceType(ImmutableArray<UserDefinedConversionAnalysis> results)
	{
		return new UserDefinedConversionResult(UserDefinedConversionResultKind.NoBestSourceType, results, -1);
	}

	public static UserDefinedConversionResult NoBestTargetType(ImmutableArray<UserDefinedConversionAnalysis> results)
	{
		return new UserDefinedConversionResult(UserDefinedConversionResultKind.NoBestTargetType, results, -1);
	}

	public static UserDefinedConversionResult Ambiguous(ImmutableArray<UserDefinedConversionAnalysis> results)
	{
		return new UserDefinedConversionResult(UserDefinedConversionResultKind.Ambiguous, results, -1);
	}

	public static UserDefinedConversionResult Valid(ImmutableArray<UserDefinedConversionAnalysis> results, int best)
	{
		return new UserDefinedConversionResult(UserDefinedConversionResultKind.Valid, results, best);
	}

	private UserDefinedConversionResult(UserDefinedConversionResultKind kind, ImmutableArray<UserDefinedConversionAnalysis> results, int best)
	{
		Kind = kind;
		Results = results;
		Best = best;
	}
}
