using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IBlockOperation : IOperation
{
	ImmutableArray<IOperation> Operations { get; }

	ImmutableArray<ILocalSymbol> Locals { get; }
}
