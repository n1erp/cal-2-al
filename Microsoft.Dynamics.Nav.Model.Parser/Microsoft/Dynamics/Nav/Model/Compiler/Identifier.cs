namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Identifier : Expression
{
	public string Name { get; private set; }

	public Identifier(string name, TypeIdentifier type, TextPosition position = null)
		: base(AstNodeType.IdentifierNode, type, position)
	{
		Name = name;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitIdentifierNode(this);
	}
}
