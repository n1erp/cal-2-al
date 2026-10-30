namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class ExpressionStatement : Statement
{
	public Expression Expr { get; set; }

	public ExpressionStatement(Expression expr, TextPosition position = null)
		: base(AstNodeType.ExprStatementNode, position)
	{
		Expr = expr;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(Expr, visitor);
		visitor.VisitExpressionStatementNode(this);
	}
}
