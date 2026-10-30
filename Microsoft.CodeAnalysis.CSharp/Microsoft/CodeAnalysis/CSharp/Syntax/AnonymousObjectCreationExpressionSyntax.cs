using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class AnonymousObjectCreationExpressionSyntax : ExpressionSyntax
{
	private SyntaxNode initializers;

	public SyntaxToken NewKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AnonymousObjectCreationExpressionSyntax)Green).newKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AnonymousObjectCreationExpressionSyntax)Green).openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> Initializers
	{
		get
		{
			SyntaxNode red = GetRed(ref initializers, 2);
			if (red != null)
			{
				return new SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax>(red, GetChildIndex(2));
			}
			return default;
		}
	}

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AnonymousObjectCreationExpressionSyntax)Green).closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal AnonymousObjectCreationExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref initializers, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return initializers;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAnonymousObjectCreationExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAnonymousObjectCreationExpression(this);
	}

	public AnonymousObjectCreationExpressionSyntax Update(SyntaxToken newKeyword, SyntaxToken openBraceToken, SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> initializers, SyntaxToken closeBraceToken)
	{
		if (newKeyword != NewKeyword || openBraceToken != OpenBraceToken || initializers != Initializers || closeBraceToken != CloseBraceToken)
		{
			AnonymousObjectCreationExpressionSyntax anonymousObjectCreationExpressionSyntax = SyntaxFactory.AnonymousObjectCreationExpression(newKeyword, openBraceToken, initializers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return anonymousObjectCreationExpressionSyntax.WithAnnotations(annotations);
			}
			return anonymousObjectCreationExpressionSyntax;
		}
		return this;
	}

	public AnonymousObjectCreationExpressionSyntax WithNewKeyword(SyntaxToken newKeyword)
	{
		return Update(newKeyword, OpenBraceToken, Initializers, CloseBraceToken);
	}

	public AnonymousObjectCreationExpressionSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(NewKeyword, openBraceToken, Initializers, CloseBraceToken);
	}

	public AnonymousObjectCreationExpressionSyntax WithInitializers(SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> initializers)
	{
		return Update(NewKeyword, OpenBraceToken, initializers, CloseBraceToken);
	}

	public AnonymousObjectCreationExpressionSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(NewKeyword, OpenBraceToken, Initializers, closeBraceToken);
	}

	public AnonymousObjectCreationExpressionSyntax AddInitializers(params AnonymousObjectMemberDeclaratorSyntax[] items)
	{
		return WithInitializers(Initializers.AddRange(items));
	}
}
