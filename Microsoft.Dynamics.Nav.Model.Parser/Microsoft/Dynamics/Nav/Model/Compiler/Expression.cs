using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Expression : AstNode
{
	internal static readonly Expression ErrorExpression = new Expression();

	public TypeIdentifier TypeIdentifier { get; set; }

	public bool HasSideEffects { get; set; }

	public bool IsConst { get; set; }

	public Expression()
		: this(AstNodeType.ErrorNode, new TypeIdentifier(0))
	{
	}

	public Expression(AstNodeType nodeType, TypeIdentifier type, TextPosition position = null)
		: base(nodeType, position)
	{
		TypeIdentifier = type;
		HasSideEffects = false;
		IsConst = false;
	}

	public Expression(AstNodeType nodeType, TextPosition position = null)
		: this(nodeType, null, position)
	{
	}

	public static bool ListHasSideEffects(List<Expression> exprList)
	{
		return exprList?.Any((Expression expr) => expr.HasSideEffects) ?? false;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
	}
}
