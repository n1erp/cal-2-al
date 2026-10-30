using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface ITupleOperation : IOperation
{
	ImmutableArray<IOperation> Elements { get; }

	ITypeSymbol NaturalType { get; }
}
