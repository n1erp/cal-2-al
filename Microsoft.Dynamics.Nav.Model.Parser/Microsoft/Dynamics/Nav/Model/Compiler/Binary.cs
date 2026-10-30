namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Binary : Expression
{
	public Token Op { get; set; }

	public Expression Lhs { get; set; }

	public Expression Rhs { get; set; }

	public Binary(Expression lhs, Expression rhs, Token op, TypeIdentifier type, TextPosition position = null)
		: base(AstNodeType.BinaryNode, type, position)
	{
		Lhs = lhs;
		Rhs = rhs;
		Op = op;
		HasSideEffects = lhs.HasSideEffects || rhs.HasSideEffects;
		IsConst = lhs.IsConst && rhs.IsConst;
	}

	public Binary(Expression lhs, Expression rhs, Token op, TextPosition position = null)
		: this(lhs, rhs, op, null, position)
	{
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(Lhs, visitor);
		AcceptAstNode(Rhs, visitor);
		visitor.VisitBinaryNode(this);
	}
}
