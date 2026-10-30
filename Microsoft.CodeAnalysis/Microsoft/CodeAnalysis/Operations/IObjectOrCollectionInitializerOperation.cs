using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IObjectOrCollectionInitializerOperation : IOperation
{
	ImmutableArray<IOperation> Initializers { get; }
}
