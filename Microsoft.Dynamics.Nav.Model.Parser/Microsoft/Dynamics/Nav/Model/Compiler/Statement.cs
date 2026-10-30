namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Statement : AstNode
{
	private static readonly Statement errorStatement = new EmptyStatement();

	internal static Statement ErrorStatement => errorStatement;

	public Statement()
	{
	}

	public Statement(AstNodeType nodeType, TextPosition position = null)
		: base(nodeType, position)
	{
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.Visit(errorStatement);
	}
}
