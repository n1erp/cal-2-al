using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class XmlNameSyntax : CSharpSyntaxNode
{
	private XmlPrefixSyntax prefix;

	public XmlPrefixSyntax Prefix => GetRedAtZero(ref prefix);

	public SyntaxToken LocalName => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlNameSyntax)Green).localName, GetChildPosition(1), GetChildIndex(1));

	internal XmlNameSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref prefix);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return prefix;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlName(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlName(this);
	}

	public XmlNameSyntax Update(XmlPrefixSyntax prefix, SyntaxToken localName)
	{
		if (prefix != Prefix || localName != LocalName)
		{
			XmlNameSyntax xmlNameSyntax = SyntaxFactory.XmlName(prefix, localName);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlNameSyntax.WithAnnotations(annotations);
			}
			return xmlNameSyntax;
		}
		return this;
	}

	public XmlNameSyntax WithPrefix(XmlPrefixSyntax prefix)
	{
		return Update(prefix, LocalName);
	}

	public XmlNameSyntax WithLocalName(SyntaxToken localName)
	{
		return Update(Prefix, localName);
	}
}
