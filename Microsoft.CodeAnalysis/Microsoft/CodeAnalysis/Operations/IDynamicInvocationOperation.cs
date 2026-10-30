using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IDynamicInvocationOperation : IOperation
{
	IOperation Operation { get; }

	ImmutableArray<IOperation> Arguments { get; }
}
