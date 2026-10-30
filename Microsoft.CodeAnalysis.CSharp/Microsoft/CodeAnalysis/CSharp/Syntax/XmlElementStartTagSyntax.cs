using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class XmlElementStartTagSyntax : CSharpSyntaxNode
{
	private XmlNameSyntax name;

	private SyntaxNode attributes;

	public SyntaxToken LessThanToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlElementStartTagSyntax)Green).lessThanToken, Position, 0);

	public XmlNameSyntax Name => GetRed(ref name, 1);

	public SyntaxList<XmlAttributeSyntax> Attributes => new SyntaxList<XmlAttributeSyntax>(GetRed(ref attributes, 2));

	public SyntaxToken GreaterThanToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlElementStartTagSyntax)Green).greaterThanToken, GetChildPosition(3), GetChildIndex(3));

	internal XmlElementStartTagSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref name, 1), 
			2 => GetRed(ref attributes, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => name, 
			2 => attributes, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlElementStartTag(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlElementStartTag(this);
	}

	public XmlElementStartTagSyntax Update(SyntaxToken lessThanToken, XmlNameSyntax name, SyntaxList<XmlAttributeSyntax> attributes, SyntaxToken greaterThanToken)
	{
		if (lessThanToken != LessThanToken || name != Name || attributes != Attributes || greaterThanToken != GreaterThanToken)
		{
			XmlElementStartTagSyntax xmlElementStartTagSyntax = SyntaxFactory.XmlElementStartTag(lessThanToken, name, attributes, greaterThanToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlElementStartTagSyntax.WithAnnotations(annotations);
			}
			return xmlElementStartTagSyntax;
		}
		return this;
	}

	public XmlElementStartTagSyntax WithLessThanToken(SyntaxToken lessThanToken)
	{
		return Update(lessThanToken, Name, Attributes, GreaterThanToken);
	}

	public XmlElementStartTagSyntax WithName(XmlNameSyntax name)
	{
		return Update(LessThanToken, name, Attributes, GreaterThanToken);
	}

	public XmlElementStartTagSyntax WithAttributes(SyntaxList<XmlAttributeSyntax> attributes)
	{
		return Update(LessThanToken, Name, attributes, GreaterThanToken);
	}

	public XmlElementStartTagSyntax WithGreaterThanToken(SyntaxToken greaterThanToken)
	{
		return Update(LessThanToken, Name, Attributes, greaterThanToken);
	}

	public XmlElementStartTagSyntax AddAttributes(params XmlAttributeSyntax[] items)
	{
		return WithAttributes(Attributes.AddRange(items));
	}
}
