namespace Microsoft.CodeAnalysis.Operations;

public interface IBranchOperation : IOperation
{
	ILabelSymbol Target { get; }

	BranchKind BranchKind { get; }
}
