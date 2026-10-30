using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IInListExpression : IOperation
{
	IOperation Value { get; }

	ImmutableArray<IOperation> List { get; }
}
