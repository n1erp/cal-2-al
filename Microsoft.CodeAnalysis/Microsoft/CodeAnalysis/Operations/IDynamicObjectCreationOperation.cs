using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IDynamicObjectCreationOperation : IOperation
{
	IObjectOrCollectionInitializerOperation Initializer { get; }

	ImmutableArray<IOperation> Arguments { get; }
}
