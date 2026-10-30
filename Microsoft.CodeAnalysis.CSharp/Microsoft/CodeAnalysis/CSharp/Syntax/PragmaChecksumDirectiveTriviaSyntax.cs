using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class PragmaChecksumDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaChecksumDirectiveTriviaSyntax)Green).hashToken, Position, 0);

	public SyntaxToken PragmaKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaChecksumDirectiveTriviaSyntax)Green).pragmaKeyword, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken ChecksumKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaChecksumDirectiveTriviaSyntax)Green).checksumKeyword, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken File => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaChecksumDirectiveTriviaSyntax)Green).file, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken Guid => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaChecksumDirectiveTriviaSyntax)Green).guid, GetChildPosition(4), GetChildIndex(4));

	public SyntaxToken Bytes => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaChecksumDirectiveTriviaSyntax)Green).bytes, GetChildPosition(5), GetChildIndex(5));

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaChecksumDirectiveTriviaSyntax)Green).endOfDirectiveToken, GetChildPosition(6), GetChildIndex(6));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PragmaChecksumDirectiveTriviaSyntax)Green).IsActive;

	internal PragmaChecksumDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitPragmaChecksumDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPragmaChecksumDirectiveTrivia(this);
	}

	public PragmaChecksumDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken checksumKeyword, SyntaxToken file, SyntaxToken guid, SyntaxToken bytes, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || pragmaKeyword != PragmaKeyword || checksumKeyword != ChecksumKeyword || file != File || guid != Guid || bytes != Bytes || endOfDirectiveToken != EndOfDirectiveToken)
		{
			PragmaChecksumDirectiveTriviaSyntax pragmaChecksumDirectiveTriviaSyntax = SyntaxFactory.PragmaChecksumDirectiveTrivia(hashToken, pragmaKeyword, checksumKeyword, file, guid, bytes, endOfDirectiveToken, isActive);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pragmaChecksumDirectiveTriviaSyntax.WithAnnotations(annotations);
			}
			return pragmaChecksumDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new PragmaChecksumDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, PragmaKeyword, ChecksumKeyword, File, Guid, Bytes, EndOfDirectiveToken, IsActive);
	}

	public PragmaChecksumDirectiveTriviaSyntax WithPragmaKeyword(SyntaxToken pragmaKeyword)
	{
		return Update(HashToken, pragmaKeyword, ChecksumKeyword, File, Guid, Bytes, EndOfDirectiveToken, IsActive);
	}

	public PragmaChecksumDirectiveTriviaSyntax WithChecksumKeyword(SyntaxToken checksumKeyword)
	{
		return Update(HashToken, PragmaKeyword, checksumKeyword, File, Guid, Bytes, EndOfDirectiveToken, IsActive);
	}

	public PragmaChecksumDirectiveTriviaSyntax WithFile(SyntaxToken file)
	{
		return Update(HashToken, PragmaKeyword, ChecksumKeyword, file, Guid, Bytes, EndOfDirectiveToken, IsActive);
	}

	public PragmaChecksumDirectiveTriviaSyntax WithGuid(SyntaxToken guid)
	{
		return Update(HashToken, PragmaKeyword, ChecksumKeyword, File, guid, Bytes, EndOfDirectiveToken, IsActive);
	}

	public PragmaChecksumDirectiveTriviaSyntax WithBytes(SyntaxToken bytes)
	{
		return Update(HashToken, PragmaKeyword, ChecksumKeyword, File, Guid, bytes, EndOfDirectiveToken, IsActive);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new PragmaChecksumDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, PragmaKeyword, ChecksumKeyword, File, Guid, Bytes, endOfDirectiveToken, IsActive);
	}

	public PragmaChecksumDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, PragmaKeyword, ChecksumKeyword, File, Guid, Bytes, EndOfDirectiveToken, isActive);
	}
}
