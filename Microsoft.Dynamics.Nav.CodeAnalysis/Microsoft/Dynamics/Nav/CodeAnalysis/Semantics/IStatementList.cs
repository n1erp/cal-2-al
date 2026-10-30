using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface IStatementList : IOperation
{
	ImmutableArray<IOperation> Statements { get; }
}
