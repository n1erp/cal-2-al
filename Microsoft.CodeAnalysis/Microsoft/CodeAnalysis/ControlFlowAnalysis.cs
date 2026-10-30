using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public abstract class ControlFlowAnalysis
{
	public abstract ImmutableArray<SyntaxNode> EntryPoints { get; }

	public abstract ImmutableArray<SyntaxNode> ExitPoints { get; }

	public abstract bool EndPointIsReachable { get; }

	public abstract bool StartPointIsReachable { get; }

	public abstract ImmutableArray<SyntaxNode> ReturnStatements { get; }

	public abstract bool Succeeded { get; }
}
