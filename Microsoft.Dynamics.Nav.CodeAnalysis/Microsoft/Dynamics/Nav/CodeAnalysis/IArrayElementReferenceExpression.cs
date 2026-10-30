using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IArrayElementReferenceExpression : IOperation
{
	IOperation ArrayReference { get; }

	ImmutableArray<IOperation> Indexes { get; }
}
