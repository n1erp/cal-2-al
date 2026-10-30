using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class ForLoopBinder : LoopBinder
{
	private readonly ForStatementSyntax forSyntax;

	public ForLoopBinder(Binder enclosing, ForStatementSyntax syntax)
		: base(enclosing)
	{
		DebugAssertHelper.Assert(syntax != null);
		forSyntax = syntax;
	}

	internal override BoundForStatement BindForParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return BindForParts(forSyntax, diagnostics);
	}

	private BoundForStatement BindForParts(ForStatementSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(node.LoopVariable, diagnostics, BindValueKind.Assignment);
		bool flag = boundExpression.HasAnyErrors;
		if (!flag && !boundExpression.Type.IsValidForLoopVariableType())
		{
			Binder.Error(diagnostics, ErrorCode.ERR_ForLoopVariableMustBeNumeric, node.LoopVariable);
			flag = true;
		}
		BoundExpression boundExpression2 = BindValue(node.InitialValue, diagnostics, BindValueKind.RValue);
		if (!flag && !boundExpression2.HasAnyErrors)
		{
			boundExpression2 = GenerateConversionForAssignment(boundExpression, boundExpression2, diagnostics);
		}
		BoundExpression boundExpression3 = BindValue(node.EndValue, diagnostics, BindValueKind.RValue);
		if (!flag && !boundExpression2.HasAnyErrors)
		{
			boundExpression3 = GenerateConversionForAssignment(boundExpression, boundExpression3, diagnostics);
		}
		BoundStatement boundStatement = BindStatement(node.Statement, diagnostics);
		return new BoundForStatement(node, boundExpression, boundExpression2, boundExpression3, boundStatement, node.OperatorKeywordToken.Kind == SyntaxKind.DownToKeyword, BreakLabel, boundExpression.HasAnyErrors || boundExpression2.HasAnyErrors || boundExpression3.HasAnyErrors || boundStatement.HasAnyErrors);
	}
}
