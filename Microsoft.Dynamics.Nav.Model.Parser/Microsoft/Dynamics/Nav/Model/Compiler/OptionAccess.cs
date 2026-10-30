namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class OptionAccess : Expression
{
	public Expression Option { get; set; }

	public Expression Ident { get; set; }

	public OptionAccess(Expression option, Expression ident, TextPosition position = null)
		: base(AstNodeType.OptionAccessNode, position)
	{
		Option = option;
		Ident = ident;
		TypeIdentifier = option.TypeIdentifier;
		HasSideEffects = option.HasSideEffects || ident.HasSideEffects;
		IsConst = true;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		AcceptAstNode(Option, visitor);
		AcceptAstNode(Ident, visitor);
		visitor.VisitOptionAccessNode(this);
	}
}
