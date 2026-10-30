using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class BoundExpressionExtensions
{
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "parent")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "binder")]
	public static void GetExpressionSymbols(this BoundExpression node, ArrayBuilder<Symbol> symbols, BoundNode parent, Binder binder)
	{
		BoundKind kind = node.Kind;
		if (kind == BoundKind.BadExpression)
		{
			symbols.AddRange(((BoundBadExpression)node).Symbols);
			return;
		}
		Symbol expressionSymbol = node.ExpressionSymbol;
		if ((object)expressionSymbol != null)
		{
			symbols.Add(expressionSymbol);
		}
	}
}
