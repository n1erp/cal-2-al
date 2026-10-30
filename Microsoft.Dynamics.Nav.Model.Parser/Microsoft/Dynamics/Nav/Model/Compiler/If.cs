namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class If : Statement
{
	public Expression IfExpr { get; set; }

	public Statement IfStmt { get; set; }

	public Statement ElseStmt { get; set; }

	public If(Expression ifExpr, Statement ifStmt, Statement elseStmt, TextPosition position = null)
		: base(AstNodeType.IfNode, position)
	{
		IfExpr = ifExpr;
		IfStmt = ifStmt;
		ElseStmt = elseStmt;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(IfExpr, visitor);
		AcceptAstNode(IfStmt, visitor);
		AcceptAstNode(ElseStmt, visitor);
		visitor.VisitIfNode(this);
	}
}
