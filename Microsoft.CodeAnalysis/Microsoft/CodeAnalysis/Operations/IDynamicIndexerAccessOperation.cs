using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IDynamicIndexerAccessOperation : IOperation
{
	IOperation Operation { get; }

	ImmutableArray<IOperation> Arguments { get; }
}
