using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class ArrayIndex : Expression
{
	public Expression ArrayExpr { get; set; }

	public List<Expression> Indices { get; set; }

	public ArrayIndex(Expression arrayExpr, List<Expression> indices, TextPosition position = null)
		: base(AstNodeType.ArrayIndexNode, position)
	{
		ArrayExpr = arrayExpr;
		Indices = indices;
		HasSideEffects = arrayExpr.HasSideEffects || Expression.ListHasSideEffects(indices);
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(ArrayExpr, visitor);
		AcceptExpressions(Indices, visitor);
		visitor.VisitArrayIndexNode(this);
	}
}
