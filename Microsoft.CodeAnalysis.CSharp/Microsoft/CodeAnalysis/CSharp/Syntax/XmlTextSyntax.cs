using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class XmlTextSyntax : XmlNodeSyntax
{
	public SyntaxTokenList TextTokens
	{
		get
		{
			GreenNode slot = Green.GetSlot(0);
			if (slot != null)
			{
				return new SyntaxTokenList(this, slot, Position, 0);
			}
			return default;
		}
	}

	internal XmlTextSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitXmlText(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlText(this);
	}

	public XmlTextSyntax Update(SyntaxTokenList textTokens)
	{
		if (textTokens != TextTokens)
		{
			XmlTextSyntax xmlTextSyntax = SyntaxFactory.XmlText(textTokens);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlTextSyntax.WithAnnotations(annotations);
			}
			return xmlTextSyntax;
		}
		return this;
	}

	public XmlTextSyntax WithTextTokens(SyntaxTokenList textTokens)
	{
		return Update(textTokens);
	}

	public XmlTextSyntax AddTextTokens(params SyntaxToken[] items)
	{
		return WithTextTokens(TextTokens.AddRange(items));
	}
}
