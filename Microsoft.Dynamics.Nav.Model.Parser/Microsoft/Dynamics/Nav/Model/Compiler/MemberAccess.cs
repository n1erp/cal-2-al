namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class MemberAccess : Expression
{
	public Expression Obj { get; set; }

	public Expression Member { get; set; }

	public MemberAccess(Expression obj, Expression member, TextPosition position = null)
		: base(AstNodeType.MemberAccessNode, position)
	{
		Obj = obj;
		Member = member;
		TypeIdentifier = member.TypeIdentifier;
		HasSideEffects = obj.HasSideEffects || member.HasSideEffects;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.OnAccepting(this);
		AcceptAstNode(Obj, visitor);
		AcceptAstNode(Member, visitor);
		visitor.VisitMemberAccessNode(this);
	}
}
