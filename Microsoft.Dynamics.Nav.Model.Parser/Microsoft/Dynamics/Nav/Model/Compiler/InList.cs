using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class InList : Expression
{
	public Expression Obj { get; set; }

	public List<Expression> List { get; set; }

	public InList(Expression obj, List<Expression> list, TextPosition position = null)
		: base(AstNodeType.InListNode, null, position)
	{
		Obj = obj;
		List = list;
		HasSideEffects = obj.HasSideEffects || Expression.ListHasSideEffects(list);
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(Obj, visitor);
		AcceptExpressions(List, visitor);
		visitor.VisitInListNode(this);
	}
}
