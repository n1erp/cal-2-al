using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter;

internal class InDataSetExpressionVisitor : ExpressionVisitor
{
	private readonly PooledHashSet<Symbol> variables;

	public InDataSetExpressionVisitor(PooledHashSet<Symbol> variables)
	{
		this.variables = variables;
	}

	internal override void VisitGlobal(BoundGlobal node)
	{
		variables.Add(node.GlobalSymbol);
	}
}
