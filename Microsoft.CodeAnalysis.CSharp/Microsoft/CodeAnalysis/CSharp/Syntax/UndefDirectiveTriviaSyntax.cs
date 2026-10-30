using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class UndefDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UndefDirectiveTriviaSyntax)Green).hashToken, Position, 0);

	public SyntaxToken UndefKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UndefDirectiveTriviaSyntax)Green).undefKeyword, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken Name => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UndefDirectiveTriviaSyntax)Green).name, GetChildPosition(2), GetChildIndex(2));

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UndefDirectiveTriviaSyntax)Green).endOfDirectiveToken, GetChildPosition(3), GetChildIndex(3));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UndefDirectiveTriviaSyntax)Green).IsActive;

	internal UndefDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitUndefDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUndefDirectiveTrivia(this);
	}

	public UndefDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken undefKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || undefKeyword != UndefKeyword || name != Name || endOfDirectiveToken != EndOfDirectiveToken)
		{
			UndefDirectiveTriviaSyntax undefDirectiveTriviaSyntax = SyntaxFactory.UndefDirectiveTrivia(hashToken, undefKeyword, name, endOfDirectiveToken, isActive);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return undefDirectiveTriviaSyntax.WithAnnotations(annotations);
			}
			return undefDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new UndefDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, UndefKeyword, Name, EndOfDirectiveToken, IsActive);
	}

	public UndefDirectiveTriviaSyntax WithUndefKeyword(SyntaxToken undefKeyword)
	{
		return Update(HashToken, undefKeyword, Name, EndOfDirectiveToken, IsActive);
	}

	public UndefDirectiveTriviaSyntax WithName(SyntaxToken name)
	{
		return Update(HashToken, UndefKeyword, name, EndOfDirectiveToken, IsActive);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new UndefDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, UndefKeyword, Name, endOfDirectiveToken, IsActive);
	}

	public UndefDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, UndefKeyword, Name, EndOfDirectiveToken, isActive);
	}
}
