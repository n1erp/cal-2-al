using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IRaiseEventOperation : IOperation
{
	IEventReferenceOperation EventReference { get; }

	ImmutableArray<IArgumentOperation> Arguments { get; }
}
