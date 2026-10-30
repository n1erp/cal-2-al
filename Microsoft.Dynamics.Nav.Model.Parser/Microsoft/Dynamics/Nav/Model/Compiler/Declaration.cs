namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Declaration : Statement
{
	public TypeIdentifier TypeIdentifier { get; set; }

	public string Name { get; private set; }

	public Expression InitValue { get; set; }

	public Modifier AccessModifier { get; set; }

	public bool IsConst { get; set; }

	public Declaration(TypeIdentifier type, string name, TextPosition position = null)
		: base(AstNodeType.DeclarationNode, position)
	{
		TypeIdentifier = type;
		Name = name;
	}

	public Declaration(TypeIdentifier typeIdentifier, string name, Expression initValue, Modifier modifier, TextPosition position = null)
		: base(AstNodeType.DeclarationNode, position)
	{
		TypeIdentifier = typeIdentifier;
		Name = name;
		InitValue = initValue;
		AccessModifier = modifier;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(TypeIdentifier, visitor);
		AcceptAstNode(InitValue, visitor);
		visitor.VisitDeclarationNode(this);
	}
}
