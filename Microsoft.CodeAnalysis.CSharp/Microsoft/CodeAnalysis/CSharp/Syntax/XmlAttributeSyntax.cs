using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class XmlAttributeSyntax : CSharpSyntaxNode
{
	public abstract XmlNameSyntax Name { get; }

	public abstract SyntaxToken EqualsToken { get; }

	public abstract SyntaxToken StartQuoteToken { get; }

	public abstract SyntaxToken EndQuoteToken { get; }

	internal XmlAttributeSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public XmlAttributeSyntax WithName(XmlNameSyntax name)
	{
		return WithNameCore(name);
	}

	internal abstract XmlAttributeSyntax WithNameCore(XmlNameSyntax name);

	public XmlAttributeSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return WithEqualsTokenCore(equalsToken);
	}

	internal abstract XmlAttributeSyntax WithEqualsTokenCore(SyntaxToken equalsToken);

	public XmlAttributeSyntax WithStartQuoteToken(SyntaxToken startQuoteToken)
	{
		return WithStartQuoteTokenCore(startQuoteToken);
	}

	internal abstract XmlAttributeSyntax WithStartQuoteTokenCore(SyntaxToken startQuoteToken);

	public XmlAttributeSyntax WithEndQuoteToken(SyntaxToken endQuoteToken)
	{
		return WithEndQuoteTokenCore(endQuoteToken);
	}

	internal abstract XmlAttributeSyntax WithEndQuoteTokenCore(SyntaxToken endQuoteToken);
}
