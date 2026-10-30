namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Unary : Expression
{
	public Token Op { get; private set; }

	public Expression Expr { get; set; }

	public Unary(Token op, Expression expr, TextPosition position = null)
		: base(AstNodeType.UnaryNode, position)
	{
		Op = op;
		Expr = expr;
		TypeIdentifier = expr.TypeIdentifier;
		HasSideEffects = expr.HasSideEffects;
		IsConst = expr.IsConst;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(Expr, visitor);
		visitor.VisitUnaryNode(this);
	}
}
