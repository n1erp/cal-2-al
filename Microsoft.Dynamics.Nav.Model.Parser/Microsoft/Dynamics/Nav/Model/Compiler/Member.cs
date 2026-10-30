namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Member : AstNode
{
	public Modifier AccessModifier { get; private set; }

	public Member(AstNodeType nodeType, Modifier modifier, TextPosition position = null)
		: base(nodeType, position)
	{
		AccessModifier = modifier;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitMemberNode(this);
	}
}
