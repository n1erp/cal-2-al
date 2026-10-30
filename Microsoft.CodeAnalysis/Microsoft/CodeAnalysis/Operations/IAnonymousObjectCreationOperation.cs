using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IAnonymousObjectCreationOperation : IOperation
{
	ImmutableArray<IOperation> Initializers { get; }
}
