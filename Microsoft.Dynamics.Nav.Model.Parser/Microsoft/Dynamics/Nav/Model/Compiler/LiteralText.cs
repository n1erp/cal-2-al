namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class LiteralText : Expression
{
	public string TextValue { get; private set; }

	public LiteralText(string textValue, TypeIdentifier type, TextPosition position = null)
		: base(AstNodeType.LiteralTextNode, type, position)
	{
		TextValue = textValue;
		IsConst = true;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitLiteralNode(this);
	}
}
