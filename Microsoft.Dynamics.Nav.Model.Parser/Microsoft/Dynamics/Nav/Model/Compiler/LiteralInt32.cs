namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class LiteralInt32 : Expression
{
	public int IntValue { get; private set; }

	public LiteralInt32(int intValue, TextPosition position = null)
		: base(AstNodeType.LiteralInt32Node, null, position)
	{
		IntValue = intValue;
		IsConst = true;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitLiteralNode(this);
	}
}
