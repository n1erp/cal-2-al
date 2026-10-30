namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class LiteralBool : Expression
{
	public bool BoolValue { get; private set; }

	public LiteralBool(bool boolValue, TextPosition position = null)
		: base(AstNodeType.LiteralBoolNode, null, position)
	{
		BoolValue = boolValue;
		IsConst = true;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitLiteralNode(this);
	}
}
