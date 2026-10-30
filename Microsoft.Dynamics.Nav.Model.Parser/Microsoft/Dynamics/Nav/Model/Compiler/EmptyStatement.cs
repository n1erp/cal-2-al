namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class EmptyStatement : Statement
{
	public EmptyStatement(TextPosition position = null)
		: base(AstNodeType.EmptyStatementNode, position)
	{
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitEmptyStatementNode(this);
	}
}
