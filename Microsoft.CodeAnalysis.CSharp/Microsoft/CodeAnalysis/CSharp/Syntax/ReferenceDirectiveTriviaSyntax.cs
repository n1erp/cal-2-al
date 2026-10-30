using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ReferenceDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ReferenceDirectiveTriviaSyntax)Green).hashToken, Position, 0);

	public SyntaxToken ReferenceKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ReferenceDirectiveTriviaSyntax)Green).referenceKeyword, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken File => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ReferenceDirectiveTriviaSyntax)Green).file, GetChildPosition(2), GetChildIndex(2));

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ReferenceDirectiveTriviaSyntax)Green).endOfDirectiveToken, GetChildPosition(3), GetChildIndex(3));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ReferenceDirectiveTriviaSyntax)Green).IsActive;

	internal ReferenceDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitReferenceDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitReferenceDirectiveTrivia(this);
	}

	public ReferenceDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken referenceKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || referenceKeyword != ReferenceKeyword || file != File || endOfDirectiveToken != EndOfDirectiveToken)
		{
			ReferenceDirectiveTriviaSyntax referenceDirectiveTriviaSyntax = SyntaxFactory.ReferenceDirectiveTrivia(hashToken, referenceKeyword, file, endOfDirectiveToken, isActive);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return referenceDirectiveTriviaSyntax.WithAnnotations(annotations);
			}
			return referenceDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new ReferenceDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, ReferenceKeyword, File, EndOfDirectiveToken, IsActive);
	}

	public ReferenceDirectiveTriviaSyntax WithReferenceKeyword(SyntaxToken referenceKeyword)
	{
		return Update(HashToken, referenceKeyword, File, EndOfDirectiveToken, IsActive);
	}

	public ReferenceDirectiveTriviaSyntax WithFile(SyntaxToken file)
	{
		return Update(HashToken, ReferenceKeyword, file, EndOfDirectiveToken, IsActive);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new ReferenceDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, ReferenceKeyword, File, endOfDirectiveToken, IsActive);
	}

	public ReferenceDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, ReferenceKeyword, File, EndOfDirectiveToken, isActive);
	}
}
