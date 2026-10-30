using System;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class LiteralDate : Expression
{
	public DateTime DateValue { get; private set; }

	public LiteralDate(DateTime dateValue, TextPosition position = null)
		: base(AstNodeType.LiteralDateNode, null, position)
	{
		DateValue = dateValue;
		IsConst = true;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitLiteralNode(this);
	}
}
