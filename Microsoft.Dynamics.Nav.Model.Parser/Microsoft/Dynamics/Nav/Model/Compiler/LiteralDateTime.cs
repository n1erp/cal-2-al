using System;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class LiteralDateTime : Expression
{
	public DateTime DateTimeValue { get; private set; }

	public LiteralDateTime(DateTime dateTimeValue, TextPosition position = null)
		: base(AstNodeType.LiteralDateTimeNode, null, position)
	{
		DateTimeValue = dateTimeValue;
		IsConst = true;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitLiteralNode(this);
	}
}
