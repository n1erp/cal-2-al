namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Assign : Statement
{
	public Token Op { get; private set; }

	public Expression Target { get; set; }

	public Expression Source { get; set; }

	public Assign(Expression target, Token op, Expression source, TextPosition position = null)
		: base(AstNodeType.AssignNode, position)
	{
		Target = target;
		Source = source;
		Op = op;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(Target, visitor);
		AcceptAstNode(Source, visitor);
		visitor.VisitAssignNode(this);
	}
}
