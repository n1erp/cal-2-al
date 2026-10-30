namespace Microsoft.CodeAnalysis.Operations;

public interface IArgumentOperation : IOperation
{
	ArgumentKind ArgumentKind { get; }

	IParameterSymbol Parameter { get; }

	IOperation Value { get; }

	CommonConversion InConversion { get; }

	CommonConversion OutConversion { get; }
}
