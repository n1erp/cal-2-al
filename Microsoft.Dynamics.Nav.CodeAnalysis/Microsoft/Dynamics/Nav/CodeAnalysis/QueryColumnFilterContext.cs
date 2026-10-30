using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class QueryColumnFilterContext : ExpressionContext
{
	private readonly SynthesizeQueryElementContainer queryElementSymbol;

	public override ITypeSymbol RelatedSymbol => queryElementSymbol;

	public override ITypeSymbol SourceSymbol => queryElementSymbol;

	private Binder SourceBinder { get; }

	public QueryColumnFilterContext(SynthesizeQueryElementContainer queryElementSymbol, Binder sourceBinder)
	{
		if (queryElementSymbol == null)
		{
			throw new ArgumentNullException("queryElementSymbol");
		}
		if (sourceBinder == null)
		{
			throw new ArgumentNullException("sourceBinder");
		}
		this.queryElementSymbol = queryElementSymbol;
		SourceBinder = sourceBinder;
	}

	public override BoundExpression BindExpression(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		return SourceBinder.BindExpression(node, diagnostics);
	}

	public override bool TryBindOptionSymbolAccess(bool bindOnSource, string relatedMemberName, ExpressionSyntax relatedSymbolSyntax, DiagnosticBag diagnostics, out BoundExpression boundExpression)
	{
		QueryDataItemSymbol queryDataItemSymbol = (QueryDataItemSymbol)queryElementSymbol.ContainingSymbol;
		return SourceBinder.BindOptionAccess(queryDataItemSymbol.RelatedTable, queryElementSymbol.ContainedSymbol.RelatedField?.Name, relatedSymbolSyntax, diagnostics, out boundExpression);
	}

	public override bool TryBindSymbolAccess(bool bindOnSource, ExpressionSyntax symbolSyntax, string symbolName, DiagnosticBag diagnostics, out BoundExpression boundExpression)
	{
		return BindQueryElementAccess(symbolSyntax, symbolName, diagnostics, out boundExpression);
	}

	public override bool TryGetSymbolByNameOrId(bool bindOnSource, int symbolId, string unquotedSymbolName, out ISymbolWithType symbol)
	{
		if (!bindOnSource)
		{
			symbol = null;
			return false;
		}
		if (string.Equals(unquotedSymbolName, queryElementSymbol.Name, StringComparison.OrdinalIgnoreCase))
		{
			symbol = queryElementSymbol.ContainedSymbol;
			return true;
		}
		symbol = null;
		return false;
	}

	private bool BindQueryElementAccess(ExpressionSyntax relatedSymbolSyntax, string relatedQueryColumnName, DiagnosticBag diagnostics, out BoundExpression boundExpression)
	{
		DebugAssertHelper.Assert(relatedSymbolSyntax != null);
		DebugAssertHelper.Assert(relatedQueryColumnName != null);
		boundExpression = null;
		if (!string.Equals(relatedQueryColumnName.UnquoteIdentifier(), queryElementSymbol.Name, StringComparison.OrdinalIgnoreCase))
		{
			diagnostics.Add(ErrorCode.ERR_NameNotInContext, relatedSymbolSyntax.GetLocation(), relatedQueryColumnName);
			return false;
		}
		QueryTypeSymbol containingSymbolOfType = queryElementSymbol.GetContainingSymbolOfType<QueryTypeSymbol>();
		DebugAssertHelper.Assert(containingSymbolOfType != null);
		BoundClassType receiverOpt = new BoundClassType((relatedSymbolSyntax as MemberAccessExpressionSyntax)?.Expression ?? relatedSymbolSyntax, QueryClassTypeSymbol.Instance, containingSymbolOfType);
		boundExpression = new BoundQueryElementAccess(relatedSymbolSyntax, receiverOpt, queryElementSymbol.ContainedSymbol, LookupResultKind.Viable, queryElementSymbol.ContainedSymbol.Type);
		if (boundExpression.Kind != BoundKind.BadExpression)
		{
			return !diagnostics.HasAnyErrors();
		}
		return false;
	}
}
