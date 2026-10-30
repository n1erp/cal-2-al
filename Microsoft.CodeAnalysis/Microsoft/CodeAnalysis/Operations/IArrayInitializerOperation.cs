using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IArrayInitializerOperation : IOperation
{
	ImmutableArray<IOperation> ElementValues { get; }
}
