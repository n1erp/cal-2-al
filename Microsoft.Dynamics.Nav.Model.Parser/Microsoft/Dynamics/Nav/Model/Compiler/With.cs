namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class With : Statement
{
	public Expression WithId { get; set; }

	public Statement Stmt { get; set; }

	public With(Expression withId, Statement stmt, TextPosition position = null)
		: base(AstNodeType.WithNode, position)
	{
		WithId = withId;
		Stmt = stmt;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.OnAccepting(this);
		AcceptAstNode(WithId, visitor);
		AcceptAstNode(Stmt, visitor);
		visitor.VisitWithNode(this);
	}
}
