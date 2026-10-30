using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ConstructorConstraintSyntax : TypeParameterConstraintSyntax
{
	public SyntaxToken NewKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConstructorConstraintSyntax)Green).newKeyword, Position, 0);

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConstructorConstraintSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConstructorConstraintSyntax)Green).closeParenToken, GetChildPosition(2), GetChildIndex(2));

	internal ConstructorConstraintSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConstructorConstraint(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConstructorConstraint(this);
	}

	public ConstructorConstraintSyntax Update(SyntaxToken newKeyword, SyntaxToken openParenToken, SyntaxToken closeParenToken)
	{
		if (newKeyword != NewKeyword || openParenToken != OpenParenToken || closeParenToken != CloseParenToken)
		{
			ConstructorConstraintSyntax constructorConstraintSyntax = SyntaxFactory.ConstructorConstraint(newKeyword, openParenToken, closeParenToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return constructorConstraintSyntax.WithAnnotations(annotations);
			}
			return constructorConstraintSyntax;
		}
		return this;
	}

	public ConstructorConstraintSyntax WithNewKeyword(SyntaxToken newKeyword)
	{
		return Update(newKeyword, OpenParenToken, CloseParenToken);
	}

	public ConstructorConstraintSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(NewKeyword, openParenToken, CloseParenToken);
	}

	public ConstructorConstraintSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(NewKeyword, OpenParenToken, closeParenToken);
	}
}
