using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IInterpolatedStringOperation : IOperation
{
	ImmutableArray<IInterpolatedStringContentOperation> Parts { get; }
}
