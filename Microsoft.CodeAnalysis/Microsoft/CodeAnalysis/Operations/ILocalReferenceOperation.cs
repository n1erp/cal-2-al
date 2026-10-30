namespace Microsoft.CodeAnalysis.Operations;

public interface ILocalReferenceOperation : IOperation
{
	ILocalSymbol Local { get; }

	bool IsDeclaration { get; }
}
