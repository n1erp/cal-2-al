namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class ParenthesesExpr : Expression
{
	public Expression Expr { get; set; }

	public ParenthesesExpr(Expression expr, TextPosition position = null)
		: base(AstNodeType.BracketExprNode, position)
	{
		Expr = expr;
		TypeIdentifier = expr.TypeIdentifier;
		HasSideEffects = expr.HasSideEffects;
		IsConst = expr.IsConst;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(Expr, visitor);
		visitor.VisitParenthesesExpressionNode(this);
	}
}
