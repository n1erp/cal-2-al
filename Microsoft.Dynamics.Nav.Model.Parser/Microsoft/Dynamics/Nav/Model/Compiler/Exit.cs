namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Exit : Statement
{
	public Expression ExitExpr { get; set; }

	public Exit(Expression exitExpr, TextPosition position = null)
		: base(AstNodeType.ExitNode, position)
	{
		ExitExpr = exitExpr;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(ExitExpr, visitor);
		visitor.VisitExitNode(this);
	}
}
