namespace Microsoft.CodeAnalysis.Operations;

public interface IParameterReferenceOperation : IOperation
{
	IParameterSymbol Parameter { get; }
}
