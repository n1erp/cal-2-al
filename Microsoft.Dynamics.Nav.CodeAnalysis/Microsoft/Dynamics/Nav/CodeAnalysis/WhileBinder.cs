using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class WhileBinder : LoopBinder
{
	private readonly StatementSyntax syntax;

	public WhileBinder(Binder enclosing, StatementSyntax syntax)
		: base(enclosing)
	{
		DebugAssertHelper.Assert(syntax?.IsKind(SyntaxKind.WhileStatement) ?? false);
		this.syntax = syntax;
	}

	internal override BoundWhileStatement BindWhileParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		WhileStatementSyntax whileStatementSyntax = (WhileStatementSyntax)syntax;
		BoundExpression condition = BindBooleanExpression(whileStatementSyntax.Condition, diagnostics);
		BoundStatement body = BindStatement(whileStatementSyntax.Statement, diagnostics);
		return new BoundWhileStatement(whileStatementSyntax, condition, body, BreakLabel);
	}
}
