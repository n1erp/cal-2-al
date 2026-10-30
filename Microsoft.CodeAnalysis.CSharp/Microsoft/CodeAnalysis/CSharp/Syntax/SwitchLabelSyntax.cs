using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class SwitchLabelSyntax : CSharpSyntaxNode
{
	public abstract SyntaxToken Keyword { get; }

	public abstract SyntaxToken ColonToken { get; }

	internal SwitchLabelSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public SwitchLabelSyntax WithKeyword(SyntaxToken keyword)
	{
		return WithKeywordCore(keyword);
	}

	internal abstract SwitchLabelSyntax WithKeywordCore(SyntaxToken keyword);

	public SwitchLabelSyntax WithColonToken(SyntaxToken colonToken)
	{
		return WithColonTokenCore(colonToken);
	}

	internal abstract SwitchLabelSyntax WithColonTokenCore(SyntaxToken colonToken);
}
