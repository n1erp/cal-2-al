using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal abstract class ExpressionContext
{
	public abstract ITypeSymbol RelatedSymbol { get; }

	public abstract ITypeSymbol SourceSymbol { get; }

	public abstract bool TryGetSymbolByNameOrId(bool bindOnSource, int symbolId, string unquotedSymbolName, out ISymbolWithType symbol);

	public abstract bool TryBindSymbolAccess(bool bindOnSource, ExpressionSyntax symbolSyntax, string symbolName, DiagnosticBag diagnostics, out BoundExpression boundExpression);

	public abstract BoundExpression BindExpression(ExpressionSyntax node, DiagnosticBag diagnostics);

	public abstract bool TryBindOptionSymbolAccess(bool bindOnSource, string relatedMemberName, ExpressionSyntax relatedSymbolSyntax, DiagnosticBag diagnostics, out BoundExpression boundExpression);
}
