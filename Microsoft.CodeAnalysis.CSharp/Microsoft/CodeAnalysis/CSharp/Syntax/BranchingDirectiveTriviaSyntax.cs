using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class BranchingDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	public abstract bool BranchTaken { get; }

	internal BranchingDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public new BranchingDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return (BranchingDirectiveTriviaSyntax)WithHashTokenCore(hashToken);
	}

	public new BranchingDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return (BranchingDirectiveTriviaSyntax)WithEndOfDirectiveTokenCore(endOfDirectiveToken);
	}
}
