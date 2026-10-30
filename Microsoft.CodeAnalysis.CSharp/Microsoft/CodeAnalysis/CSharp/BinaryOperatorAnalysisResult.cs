using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct BinaryOperatorAnalysisResult
{
	public readonly Conversion LeftConversion;

	public readonly Conversion RightConversion;

	public readonly BinaryOperatorSignature Signature;

	public readonly OperatorAnalysisResultKind Kind;

	public bool IsValid => Kind == OperatorAnalysisResultKind.Applicable;

	public bool HasValue => Kind != OperatorAnalysisResultKind.Undefined;

	private BinaryOperatorAnalysisResult(OperatorAnalysisResultKind kind, BinaryOperatorSignature signature, Conversion leftConversion, Conversion rightConversion)
	{
		Kind = kind;
		Signature = signature;
		LeftConversion = leftConversion;
		RightConversion = rightConversion;
	}

	public override bool Equals(object obj)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable;
	}

	public static BinaryOperatorAnalysisResult Applicable(BinaryOperatorSignature signature, Conversion leftConversion, Conversion rightConversion)
	{
		return new BinaryOperatorAnalysisResult(OperatorAnalysisResultKind.Applicable, signature, leftConversion, rightConversion);
	}

	public static BinaryOperatorAnalysisResult Inapplicable(BinaryOperatorSignature signature, Conversion leftConversion, Conversion rightConversion)
	{
		return new BinaryOperatorAnalysisResult(OperatorAnalysisResultKind.Inapplicable, signature, leftConversion, rightConversion);
	}

	public BinaryOperatorAnalysisResult Worse()
	{
		return new BinaryOperatorAnalysisResult(OperatorAnalysisResultKind.Worse, Signature, LeftConversion, RightConversion);
	}
}
