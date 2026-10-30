using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IInvocationExpression : IOperation
{
	IMethodSymbol TargetMethod { get; }

	IOperation Instance { get; }

	ImmutableArray<IArgument> Arguments { get; }
}
