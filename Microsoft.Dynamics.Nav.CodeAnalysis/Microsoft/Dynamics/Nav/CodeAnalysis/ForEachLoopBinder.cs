using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class ForEachLoopBinder : LoopBinder
{
	private readonly ForEachStatementSyntax forEachSyntax;

	public ForEachLoopBinder(Binder enclosing, ForEachStatementSyntax syntax)
		: base(enclosing)
	{
		DebugAssertHelper.Assert(syntax != null);
		forEachSyntax = syntax;
	}

	internal override BoundForEachStatement BindForEachParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return BindForEachStatementParts(forEachSyntax, diagnostics);
	}

	private BoundForEachStatement BindForEachStatementParts(ForEachStatementSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(node.IterationVariable, diagnostics, BindValueKind.Assignment);
		BoundExpression boundExpression2 = BindValue(node.Expression, diagnostics, BindValueKind.RValue);
		bool flag = boundExpression2.HasAnyErrors;
		if (!flag && !boundExpression2.Type.IsEnumerableType())
		{
			Binder.Error(diagnostics, ErrorCode.ERR_ForEachExpressionMustBeEnumerable, node.Expression);
			flag = true;
		}
		flag = flag || boundExpression.HasAnyErrors;
		Conversion elementConversion = Conversion.Identity;
		if (!flag)
		{
			TypeSymbol enumeratorElementType = boundExpression2.Type.GetEnumeratorElementType();
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			elementConversion = Conversions.ClassifyConversionFromType(enumeratorElementType, boundExpression.Type, ref useSiteDiagnostics);
			if (!elementConversion.IsValid)
			{
				Binder.Error(diagnostics, ErrorCode.ERR_NoImplicitConversion, node.ForEachKeywordToken, new FormattedSymbol(enumeratorElementType, SymbolDisplayFormat.ErrorMessageFormat), new FormattedSymbol(boundExpression.Type, SymbolDisplayFormat.ErrorMessageFormat));
				flag = true;
			}
		}
		BoundStatement boundStatement = BindStatement(node.Statement, diagnostics);
		return new BoundForEachStatement(node, elementConversion, boundExpression, boundExpression2, boundStatement, BreakLabel, flag || boundStatement.HasAnyErrors);
	}
}
