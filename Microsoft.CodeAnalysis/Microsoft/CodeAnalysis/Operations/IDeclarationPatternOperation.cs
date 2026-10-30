namespace Microsoft.CodeAnalysis.Operations;

public interface IDeclarationPatternOperation : IPatternOperation, IOperation
{
	ISymbol DeclaredSymbol { get; }
}
