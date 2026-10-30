namespace Microsoft.CodeAnalysis.Operations;

public interface IMemberReferenceOperation : IOperation
{
	IOperation Instance { get; }

	ISymbol Member { get; }
}
