namespace Microsoft.CodeAnalysis.Operations;

public interface IParameterInitializerOperation : ISymbolInitializerOperation, IOperation
{
	IParameterSymbol Parameter { get; }
}
