namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class LiteralInt64 : Expression
{
	public long IntValue { get; private set; }

	public LiteralInt64(long intValue, TextPosition position = null)
		: base(AstNodeType.LiteralInt64Node, null, position)
	{
		IntValue = intValue;
		IsConst = true;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitLiteralNode(this);
	}
}
