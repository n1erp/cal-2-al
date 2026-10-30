using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class TableExpressionContext : ExpressionContext
{
	public TableTypeSymbol RelatedTable { get; }

	public override ITypeSymbol RelatedSymbol => RelatedTable;

	public override ITypeSymbol SourceSymbol => SourceTable;

	private TableTypeSymbol SourceTable { get; }

	private Binder SourceBinder { get; }

	public TableExpressionContext(TableTypeSymbol sourceTable, Binder sourceBinder, TableTypeSymbol relatedTable)
	{
		if (sourceBinder == null)
		{
			throw new ArgumentNullException("sourceBinder");
		}
		if (relatedTable == null)
		{
			throw new ArgumentNullException("relatedTable");
		}
		SourceTable = sourceTable;
		SourceBinder = sourceBinder;
		RelatedTable = relatedTable;
	}

	public static TableExpressionContext CreateWithDefaultBinder(Binder binder, TableTypeSymbol relatedTable)
	{
		return new TableExpressionContext((binder.ContainingMember.HasRelatedTable() ? binder.ContainingMember : binder.ContainingMember.GetContainingSymbolOfType<ObjectTypeSymbol>()).GetRelatedTableSymbol(), binder, relatedTable);
	}

	public override bool TryGetSymbolByNameOrId(bool bindOnSource, int symbolId, string unquotedSymbolName, out ISymbolWithType symbol)
	{
		TableTypeSymbol relatedTable = (bindOnSource ? SourceTable : RelatedTable);
		bool result = SourceBinder.TryGetFieldByNameOrId(relatedTable, symbolId, unquotedSymbolName, out var fieldSymbol);
		symbol = fieldSymbol;
		return result;
	}

	public override bool TryBindSymbolAccess(bool bindOnSource, ExpressionSyntax symbolSyntax, string symbolName, DiagnosticBag diagnostics, out BoundExpression boundExpression)
	{
		TableTypeSymbol relatedTable = (bindOnSource ? SourceTable : RelatedTable);
		return SourceBinder.BindFieldAccess(relatedTable, symbolSyntax, symbolName, diagnostics, out boundExpression);
	}

	public override BoundExpression BindExpression(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		return SourceBinder.BindExpression(node, diagnostics);
	}

	public override bool TryBindOptionSymbolAccess(bool bindOnSource, string relatedMemberName, ExpressionSyntax relatedSymbolSyntax, DiagnosticBag diagnostics, out BoundExpression boundExpression)
	{
		TableTypeSymbol relatedTable = (bindOnSource ? SourceTable : RelatedTable);
		return SourceBinder.BindOptionAccess(relatedTable, relatedMemberName, relatedSymbolSyntax, diagnostics, out boundExpression);
	}
}
