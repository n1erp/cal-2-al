namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Parameter : AstNode
{
	public bool IsVar { get; private set; }

	public string Name { get; private set; }

	public TypeIdentifier TypeIdentifier { get; set; }

	public Parameter(TypeIdentifier typeIdentifier, string name, bool isVar, TextPosition position = null)
		: base(AstNodeType.ParameterNode, position)
	{
		TypeIdentifier = typeIdentifier;
		Name = name;
		IsVar = isVar;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(TypeIdentifier, visitor);
		visitor.VisitParameterNode(this);
	}
}
