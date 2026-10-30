using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class MemberBindingExpressionSyntax : ExpressionSyntax
{
	private SimpleNameSyntax name;

	public SyntaxToken OperatorToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.MemberBindingExpressionSyntax)Green).operatorToken, Position, 0);

	public SimpleNameSyntax Name => GetRed(ref name, 1);

	internal MemberBindingExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref name, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return name;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMemberBindingExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitMemberBindingExpression(this);
	}

	public MemberBindingExpressionSyntax Update(SyntaxToken operatorToken, SimpleNameSyntax name)
	{
		if (operatorToken != OperatorToken || name != Name)
		{
			MemberBindingExpressionSyntax memberBindingExpressionSyntax = SyntaxFactory.MemberBindingExpression(operatorToken, name);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return memberBindingExpressionSyntax.WithAnnotations(annotations);
			}
			return memberBindingExpressionSyntax;
		}
		return this;
	}

	public MemberBindingExpressionSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(operatorToken, Name);
	}

	public MemberBindingExpressionSyntax WithName(SimpleNameSyntax name)
	{
		return Update(OperatorToken, name);
	}
}
