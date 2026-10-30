using System;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class LiteralTime : Expression
{
	public DateTime TimeValue { get; private set; }

	public LiteralTime(DateTime timeValue, TextPosition position = null)
		: base(AstNodeType.LiteralTimeNode, null, position)
	{
		TimeValue = timeValue;
		IsConst = true;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitLiteralNode(this);
	}
}
