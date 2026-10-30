namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class DefaultValue : Expression
{
	public Expression Expr { get; set; }

	public DefaultValue(TypeIdentifier type, Expression expr, TextPosition position = null)
		: base(AstNodeType.DefaultValueNode, type, position)
	{
		Expr = expr;
		HasSideEffects = expr.HasSideEffects;
		IsConst = expr.IsConst;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(TypeIdentifier, visitor);
		AcceptAstNode(Expr, visitor);
		visitor.VisitDefaultValueNode(this);
	}
}
