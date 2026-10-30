namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal struct ArgumentAnalysisResult
{
	public readonly EffectiveParameters EffectiveParameters;

	public readonly int ArgumentPosition;

	public readonly int ParameterPosition;

	public readonly ArgumentAnalysisResultKind Kind;

	public bool IsValid => (int)Kind < 1;

	private ArgumentAnalysisResult(ArgumentAnalysisResultKind kind, int argumentPosition, int parameterPosition, EffectiveParameters effectiveParameters)
	{
		Kind = kind;
		ArgumentPosition = argumentPosition;
		ParameterPosition = parameterPosition;
		EffectiveParameters = effectiveParameters;
	}

	public static ArgumentAnalysisResult NoCorrespondingParameter(int argumentPosition, EffectiveParameters effectiveParameters)
	{
		return new ArgumentAnalysisResult(ArgumentAnalysisResultKind.NoCorrespondingParameter, argumentPosition, 0, effectiveParameters);
	}

	public static ArgumentAnalysisResult RequiredParameterMissing(int parameterPosition, EffectiveParameters effectiveParameters)
	{
		return new ArgumentAnalysisResult(ArgumentAnalysisResultKind.RequiredParameterMissing, 0, parameterPosition, effectiveParameters);
	}

	public static ArgumentAnalysisResult NormalForm(EffectiveParameters effectiveParameters)
	{
		return new ArgumentAnalysisResult(ArgumentAnalysisResultKind.Normal, 0, 0, effectiveParameters);
	}
}
