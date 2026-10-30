using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IArrayCreationOperation : IOperation
{
	ImmutableArray<IOperation> DimensionSizes { get; }

	IArrayInitializerOperation Initializer { get; }
}
