using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ShebangDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ShebangDirectiveTriviaSyntax)Green).hashToken, Position, 0);

	public SyntaxToken ExclamationToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ShebangDirectiveTriviaSyntax)Green).exclamationToken, GetChildPosition(1), GetChildIndex(1));

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ShebangDirectiveTriviaSyntax)Green).endOfDirectiveToken, GetChildPosition(2), GetChildIndex(2));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ShebangDirectiveTriviaSyntax)Green).IsActive;

	internal ShebangDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitShebangDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitShebangDirectiveTrivia(this);
	}

	public ShebangDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken exclamationToken, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || exclamationToken != ExclamationToken || endOfDirectiveToken != EndOfDirectiveToken)
		{
			ShebangDirectiveTriviaSyntax shebangDirectiveTriviaSyntax = SyntaxFactory.ShebangDirectiveTrivia(hashToken, exclamationToken, endOfDirectiveToken, isActive);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return shebangDirectiveTriviaSyntax.WithAnnotations(annotations);
			}
			return shebangDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new ShebangDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, ExclamationToken, EndOfDirectiveToken, IsActive);
	}

	public ShebangDirectiveTriviaSyntax WithExclamationToken(SyntaxToken exclamationToken)
	{
		return Update(HashToken, exclamationToken, EndOfDirectiveToken, IsActive);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new ShebangDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, ExclamationToken, endOfDirectiveToken, IsActive);
	}

	public ShebangDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, ExclamationToken, EndOfDirectiveToken, isActive);
	}
}
