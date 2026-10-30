using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class PragmaWarningDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	private SyntaxNode errorCodes;

	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaWarningDirectiveTriviaSyntax)Green).hashToken, Position, 0);

	public SyntaxToken PragmaKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaWarningDirectiveTriviaSyntax)Green).pragmaKeyword, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken WarningKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaWarningDirectiveTriviaSyntax)Green).warningKeyword, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken DisableOrRestoreKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaWarningDirectiveTriviaSyntax)Green).disableOrRestoreKeyword, GetChildPosition(3), GetChildIndex(3));

	public SeparatedSyntaxList<ExpressionSyntax> ErrorCodes
	{
		get
		{
			SyntaxNode red = GetRed(ref errorCodes, 4);
			if (red != null)
			{
				return new SeparatedSyntaxList<ExpressionSyntax>(red, GetChildIndex(4));
			}
			return default;
		}
	}

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaWarningDirectiveTriviaSyntax)Green).endOfDirectiveToken, GetChildPosition(5), GetChildIndex(5));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaWarningDirectiveTriviaSyntax)Green).IsActive;

	internal PragmaWarningDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 4)
		{
			return GetRed(ref errorCodes, 4);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 4)
		{
			return errorCodes;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPragmaWarningDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPragmaWarningDirectiveTrivia(this);
	}

	public PragmaWarningDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken warningKeyword, SyntaxToken disableOrRestoreKeyword, SeparatedSyntaxList<ExpressionSyntax> errorCodes, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || pragmaKeyword != PragmaKeyword || warningKeyword != WarningKeyword || disableOrRestoreKeyword != DisableOrRestoreKeyword || errorCodes != ErrorCodes || endOfDirectiveToken != EndOfDirectiveToken)
		{
			PragmaWarningDirectiveTriviaSyntax pragmaWarningDirectiveTriviaSyntax = SyntaxFactory.PragmaWarningDirectiveTrivia(hashToken, pragmaKeyword, warningKeyword, disableOrRestoreKeyword, errorCodes, endOfDirectiveToken, isActive);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pragmaWarningDirectiveTriviaSyntax.WithAnnotations(annotations);
			}
			return pragmaWarningDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new PragmaWarningDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, PragmaKeyword, WarningKeyword, DisableOrRestoreKeyword, ErrorCodes, EndOfDirectiveToken, IsActive);
	}

	public PragmaWarningDirectiveTriviaSyntax WithPragmaKeyword(SyntaxToken pragmaKeyword)
	{
		return Update(HashToken, pragmaKeyword, WarningKeyword, DisableOrRestoreKeyword, ErrorCodes, EndOfDirectiveToken, IsActive);
	}

	public PragmaWarningDirectiveTriviaSyntax WithWarningKeyword(SyntaxToken warningKeyword)
	{
		return Update(HashToken, PragmaKeyword, warningKeyword, DisableOrRestoreKeyword, ErrorCodes, EndOfDirectiveToken, IsActive);
	}

	public PragmaWarningDirectiveTriviaSyntax WithDisableOrRestoreKeyword(SyntaxToken disableOrRestoreKeyword)
	{
		return Update(HashToken, PragmaKeyword, WarningKeyword, disableOrRestoreKeyword, ErrorCodes, EndOfDirectiveToken, IsActive);
	}

	public PragmaWarningDirectiveTriviaSyntax WithErrorCodes(SeparatedSyntaxList<ExpressionSyntax> errorCodes)
	{
		return Update(HashToken, PragmaKeyword, WarningKeyword, DisableOrRestoreKeyword, errorCodes, EndOfDirectiveToken, IsActive);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new PragmaWarningDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, PragmaKeyword, WarningKeyword, DisableOrRestoreKeyword, ErrorCodes, endOfDirectiveToken, IsActive);
	}

	public PragmaWarningDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, PragmaKeyword, WarningKeyword, DisableOrRestoreKeyword, ErrorCodes, EndOfDirectiveToken, isActive);
	}

	public PragmaWarningDirectiveTriviaSyntax AddErrorCodes(params ExpressionSyntax[] items)
	{
		return WithErrorCodes(ErrorCodes.AddRange(items));
	}
}
