namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Foreach : Statement
{
	public Expression LoopVar { get; set; }

	public Expression Collection { get; set; }

	public Statement Body { get; set; }

	public Foreach(Expression var, Expression collection, Statement body, TextPosition position = null)
		: base(AstNodeType.ForeachNode, position)
	{
		LoopVar = var;
		Collection = collection;
		Body = body;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(LoopVar, visitor);
		AcceptAstNode(Collection, visitor);
		AcceptAstNode(Body, visitor);
		visitor.VisitForEachNode(this);
	}
}
