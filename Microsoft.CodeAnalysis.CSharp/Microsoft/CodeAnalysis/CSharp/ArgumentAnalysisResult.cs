using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct ArgumentAnalysisResult
{
	public readonly ImmutableArray<int> ArgsToParamsOpt;

	public readonly int ArgumentPosition;

	public readonly int ParameterPosition;

	public readonly ArgumentAnalysisResultKind Kind;

	public bool IsValid => (int)Kind < 2;

	public int ParameterFromArgument(int arg)
	{
		if (ArgsToParamsOpt.IsDefault)
		{
			return arg;
		}
		return ArgsToParamsOpt[arg];
	}

	private ArgumentAnalysisResult(ArgumentAnalysisResultKind kind, int argumentPosition, int parameterPosition, ImmutableArray<int> argsToParamsOpt)
	{
		Kind = kind;
		ArgumentPosition = argumentPosition;
		ParameterPosition = parameterPosition;
		ArgsToParamsOpt = argsToParamsOpt;
	}

	public static ArgumentAnalysisResult NameUsedForPositional(int argumentPosition)
	{
		return new ArgumentAnalysisResult(ArgumentAnalysisResultKind.NameUsedForPositional, argumentPosition, 0, default);
	}

	public static ArgumentAnalysisResult NoCorrespondingParameter(int argumentPosition)
	{
		return new ArgumentAnalysisResult(ArgumentAnalysisResultKind.NoCorrespondingParameter, argumentPosition, 0, default);
	}

	public static ArgumentAnalysisResult NoCorrespondingNamedParameter(int argumentPosition)
	{
		return new ArgumentAnalysisResult(ArgumentAnalysisResultKind.NoCorrespondingNamedParameter, argumentPosition, 0, default);
	}

	public static ArgumentAnalysisResult DuplicateNamedArgument(int argumentPosition)
	{
		return new ArgumentAnalysisResult(ArgumentAnalysisResultKind.DuplicateNamedArgument, argumentPosition, 0, default);
	}

	public static ArgumentAnalysisResult RequiredParameterMissing(int parameterPosition)
	{
		return new ArgumentAnalysisResult(ArgumentAnalysisResultKind.RequiredParameterMissing, 0, parameterPosition, default);
	}

	public static ArgumentAnalysisResult BadNonTrailingNamedArgument(int argumentPosition)
	{
		return new ArgumentAnalysisResult(ArgumentAnalysisResultKind.BadNonTrailingNamedArgument, argumentPosition, 0, default);
	}

	public static ArgumentAnalysisResult NormalForm(ImmutableArray<int> argsToParamsOpt)
	{
		return new ArgumentAnalysisResult(ArgumentAnalysisResultKind.Normal, 0, 0, argsToParamsOpt);
	}

	public static ArgumentAnalysisResult ExpandedForm(ImmutableArray<int> argsToParamsOpt)
	{
		return new ArgumentAnalysisResult(ArgumentAnalysisResultKind.Expanded, 0, 0, argsToParamsOpt);
	}
}
