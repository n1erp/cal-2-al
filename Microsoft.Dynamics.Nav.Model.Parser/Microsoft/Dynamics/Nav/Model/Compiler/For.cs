namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class For : Statement
{
	public Expression LoopVar { get; set; }

	public Expression Init { get; set; }

	public Expression To { get; set; }

	public Statement Body { get; set; }

	public Token Op { get; private set; }

	public For()
		: this(Expression.ErrorExpression, Expression.ErrorExpression, Token.t_none, Expression.ErrorExpression, Statement.ErrorStatement)
	{
	}

	public For(Expression var, Expression init, Token op, Expression to, Statement body, TextPosition position = null)
		: base(AstNodeType.ForNode, position)
	{
		LoopVar = var;
		Init = init;
		Op = op;
		To = to;
		Body = body;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(LoopVar, visitor);
		AcceptAstNode(Init, visitor);
		AcceptAstNode(To, visitor);
		AcceptAstNode(Body, visitor);
		visitor.VisitForNode(this);
	}
}
