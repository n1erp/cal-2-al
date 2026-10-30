using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class EndRegionDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EndRegionDirectiveTriviaSyntax)Green).hashToken, Position, 0);

	public SyntaxToken EndRegionKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EndRegionDirectiveTriviaSyntax)Green).endRegionKeyword, GetChildPosition(1), GetChildIndex(1));

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EndRegionDirectiveTriviaSyntax)Green).endOfDirectiveToken, GetChildPosition(2), GetChildIndex(2));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EndRegionDirectiveTriviaSyntax)Green).IsActive;

	internal EndRegionDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitEndRegionDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEndRegionDirectiveTrivia(this);
	}

	public EndRegionDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken endRegionKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || endRegionKeyword != EndRegionKeyword || endOfDirectiveToken != EndOfDirectiveToken)
		{
			EndRegionDirectiveTriviaSyntax endRegionDirectiveTriviaSyntax = SyntaxFactory.EndRegionDirectiveTrivia(hashToken, endRegionKeyword, endOfDirectiveToken, isActive);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return endRegionDirectiveTriviaSyntax.WithAnnotations(annotations);
			}
			return endRegionDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new EndRegionDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, EndRegionKeyword, EndOfDirectiveToken, IsActive);
	}

	public EndRegionDirectiveTriviaSyntax WithEndRegionKeyword(SyntaxToken endRegionKeyword)
	{
		return Update(HashToken, endRegionKeyword, EndOfDirectiveToken, IsActive);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new EndRegionDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, EndRegionKeyword, endOfDirectiveToken, IsActive);
	}

	public EndRegionDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, EndRegionKeyword, EndOfDirectiveToken, isActive);
	}
}
