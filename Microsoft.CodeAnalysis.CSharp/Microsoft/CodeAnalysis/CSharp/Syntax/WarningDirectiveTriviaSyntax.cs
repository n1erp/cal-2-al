using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class WarningDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.WarningDirectiveTriviaSyntax)Green).hashToken, Position, 0);

	public SyntaxToken WarningKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.WarningDirectiveTriviaSyntax)Green).warningKeyword, GetChildPosition(1), GetChildIndex(1));

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.WarningDirectiveTriviaSyntax)Green).endOfDirectiveToken, GetChildPosition(2), GetChildIndex(2));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.WarningDirectiveTriviaSyntax)Green).IsActive;

	internal WarningDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitWarningDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitWarningDirectiveTrivia(this);
	}

	public WarningDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken warningKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || warningKeyword != WarningKeyword || endOfDirectiveToken != EndOfDirectiveToken)
		{
			WarningDirectiveTriviaSyntax warningDirectiveTriviaSyntax = SyntaxFactory.WarningDirectiveTrivia(hashToken, warningKeyword, endOfDirectiveToken, isActive);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return warningDirectiveTriviaSyntax.WithAnnotations(annotations);
			}
			return warningDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new WarningDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, WarningKeyword, EndOfDirectiveToken, IsActive);
	}

	public WarningDirectiveTriviaSyntax WithWarningKeyword(SyntaxToken warningKeyword)
	{
		return Update(HashToken, warningKeyword, EndOfDirectiveToken, IsActive);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new WarningDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, WarningKeyword, endOfDirectiveToken, IsActive);
	}

	public WarningDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, WarningKeyword, EndOfDirectiveToken, isActive);
	}
}
