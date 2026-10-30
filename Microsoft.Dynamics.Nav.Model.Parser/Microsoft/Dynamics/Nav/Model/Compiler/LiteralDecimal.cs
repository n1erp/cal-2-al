namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class LiteralDecimal : Expression
{
	public decimal DecValue { get; private set; }

	public LiteralDecimal(decimal decValue, TextPosition position = null)
		: base(AstNodeType.LiteralDecimalNode, null, position)
	{
		DecValue = decValue;
		IsConst = true;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitLiteralNode(this);
	}
}
