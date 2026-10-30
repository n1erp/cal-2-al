namespace Microsoft.CodeAnalysis.Operations;

public interface IMethodReferenceOperation : IMemberReferenceOperation, IOperation
{
	IMethodSymbol Method { get; }

	bool IsVirtual { get; }
}
