namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Cast : Expression
{
	public Expression Expr { get; set; }

	public Cast(TypeIdentifier type, Expression expr, TextPosition position = null)
		: base(AstNodeType.CastNode, type, position)
	{
		Expr = expr;
		HasSideEffects = expr.HasSideEffects;
		IsConst = expr.IsConst;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(Expr, visitor);
		visitor.VisitCastNode(this);
	}
}
