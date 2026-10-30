using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class XmlCDataSectionSyntax : XmlNodeSyntax
{
	public SyntaxToken StartCDataToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlCDataSectionSyntax)Green).startCDataToken, Position, 0);

	public SyntaxTokenList TextTokens
	{
		get
		{
			GreenNode slot = Green.GetSlot(1);
			if (slot != null)
			{
				return new SyntaxTokenList(this, slot, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken EndCDataToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlCDataSectionSyntax)Green).endCDataToken, GetChildPosition(2), GetChildIndex(2));

	internal XmlCDataSectionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitXmlCDataSection(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlCDataSection(this);
	}

	public XmlCDataSectionSyntax Update(SyntaxToken startCDataToken, SyntaxTokenList textTokens, SyntaxToken endCDataToken)
	{
		if (startCDataToken != StartCDataToken || textTokens != TextTokens || endCDataToken != EndCDataToken)
		{
			XmlCDataSectionSyntax xmlCDataSectionSyntax = SyntaxFactory.XmlCDataSection(startCDataToken, textTokens, endCDataToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlCDataSectionSyntax.WithAnnotations(annotations);
			}
			return xmlCDataSectionSyntax;
		}
		return this;
	}

	public XmlCDataSectionSyntax WithStartCDataToken(SyntaxToken startCDataToken)
	{
		return Update(startCDataToken, TextTokens, EndCDataToken);
	}

	public XmlCDataSectionSyntax WithTextTokens(SyntaxTokenList textTokens)
	{
		return Update(StartCDataToken, textTokens, EndCDataToken);
	}

	public XmlCDataSectionSyntax WithEndCDataToken(SyntaxToken endCDataToken)
	{
		return Update(StartCDataToken, TextTokens, endCDataToken);
	}

	public XmlCDataSectionSyntax AddTextTokens(params SyntaxToken[] items)
	{
		return WithTextTokens(TextTokens.AddRange(items));
	}
}
