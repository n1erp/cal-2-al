using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Repeat : Statement
{
	public Expression UntilExpr { get; set; }

	public List<Statement> Stmts { get; set; }

	public Repeat(List<Statement> stmts, Expression untilExpr, TextPosition position = null)
		: base(AstNodeType.RepeatNode, position)
	{
		Stmts = stmts;
		UntilExpr = untilExpr;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(UntilExpr, visitor);
		AcceptStatements(Stmts, visitor);
		visitor.VisitRepeatNode(this);
	}
}
