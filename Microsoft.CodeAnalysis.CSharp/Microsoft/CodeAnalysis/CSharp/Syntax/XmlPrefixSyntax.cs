using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class XmlPrefixSyntax : CSharpSyntaxNode
{
	public SyntaxToken Prefix => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlPrefixSyntax)Green).prefix, Position, 0);

	public SyntaxToken ColonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlPrefixSyntax)Green).colonToken, GetChildPosition(1), GetChildIndex(1));

	internal XmlPrefixSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitXmlPrefix(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlPrefix(this);
	}

	public XmlPrefixSyntax Update(SyntaxToken prefix, SyntaxToken colonToken)
	{
		if (prefix != Prefix || colonToken != ColonToken)
		{
			XmlPrefixSyntax xmlPrefixSyntax = SyntaxFactory.XmlPrefix(prefix, colonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlPrefixSyntax.WithAnnotations(annotations);
			}
			return xmlPrefixSyntax;
		}
		return this;
	}

	public XmlPrefixSyntax WithPrefix(SyntaxToken prefix)
	{
		return Update(prefix, ColonToken);
	}

	public XmlPrefixSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(Prefix, colonToken);
	}
}
