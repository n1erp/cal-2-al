using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class RepeatBinder : LoopBinder
{
	private readonly RepeatStatementSyntax syntax;

	public RepeatBinder(Binder enclosing, RepeatStatementSyntax syntax)
		: base(enclosing)
	{
		this.syntax = syntax;
	}

	internal override BoundRepeatStatement BindRepeatParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		BoundExpression condition = BindBooleanExpression(syntax.Condition, diagnostics);
		ImmutableArray<BoundStatement> statements = Binder.BindStatements(syntax.Statements, diagnostics, this);
		BoundStatementList body = new BoundStatementList(syntax, statements);
		return new BoundRepeatStatement(syntax, body, condition, BreakLabel);
	}
}
