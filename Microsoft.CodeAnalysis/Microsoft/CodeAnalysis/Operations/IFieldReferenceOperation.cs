namespace Microsoft.CodeAnalysis.Operations;

public interface IFieldReferenceOperation : IMemberReferenceOperation, IOperation
{
	IFieldSymbol Field { get; }

	bool IsDeclaration { get; }
}
