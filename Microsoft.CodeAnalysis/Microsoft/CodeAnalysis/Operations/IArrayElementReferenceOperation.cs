using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IArrayElementReferenceOperation : IOperation
{
	IOperation ArrayReference { get; }

	ImmutableArray<IOperation> Indices { get; }
}
