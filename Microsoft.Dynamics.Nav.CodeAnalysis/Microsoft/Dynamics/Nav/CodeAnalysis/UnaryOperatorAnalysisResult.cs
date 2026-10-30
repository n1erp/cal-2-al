namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal struct UnaryOperatorAnalysisResult
{
	public readonly UnaryOperatorSignature Signature;

	public readonly Conversion Conversion;

	public readonly OperatorAnalysisResultKind Kind;

	public bool IsValid => Kind == OperatorAnalysisResultKind.Applicable;

	public bool HasValue => Kind != OperatorAnalysisResultKind.Undefined;

	private UnaryOperatorAnalysisResult(OperatorAnalysisResultKind kind, UnaryOperatorSignature signature, Conversion conversion)
	{
		Kind = kind;
		Signature = signature;
		Conversion = conversion;
	}

	public static UnaryOperatorAnalysisResult Applicable(UnaryOperatorSignature signature, Conversion conversion)
	{
		return new UnaryOperatorAnalysisResult(OperatorAnalysisResultKind.Applicable, signature, conversion);
	}

	public static UnaryOperatorAnalysisResult Inapplicable(UnaryOperatorSignature signature, Conversion conversion)
	{
		return new UnaryOperatorAnalysisResult(OperatorAnalysisResultKind.Inapplicable, signature, conversion);
	}

	public UnaryOperatorAnalysisResult Worse()
	{
		return new UnaryOperatorAnalysisResult(OperatorAnalysisResultKind.Worse, Signature, Conversion);
	}
}
