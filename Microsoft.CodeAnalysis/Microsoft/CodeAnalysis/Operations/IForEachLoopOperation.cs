using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IForEachLoopOperation : ILoopOperation, IOperation
{
	IOperation LoopControlVariable { get; }

	IOperation Collection { get; }

	ImmutableArray<IOperation> NextVariables { get; }
}
