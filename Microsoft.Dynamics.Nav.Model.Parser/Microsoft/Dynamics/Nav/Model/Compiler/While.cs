namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class While : Statement
{
	public Expression WhileExpr { get; set; }

	public Statement Stmt { get; set; }

	public While(Expression whileExpr, Statement stmt, TextPosition position = null)
		: base(AstNodeType.WhileNode, position)
	{
		WhileExpr = whileExpr;
		Stmt = stmt;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(WhileExpr, visitor);
		AcceptAstNode(Stmt, visitor);
		visitor.VisitWhileNode(this);
	}
}
