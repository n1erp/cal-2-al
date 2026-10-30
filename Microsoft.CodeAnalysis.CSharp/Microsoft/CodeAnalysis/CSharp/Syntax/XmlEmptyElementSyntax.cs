using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class XmlEmptyElementSyntax : XmlNodeSyntax
{
	private XmlNameSyntax name;

	private SyntaxNode attributes;

	public SyntaxToken LessThanToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlEmptyElementSyntax)Green).lessThanToken, Position, 0);

	public XmlNameSyntax Name => GetRed(ref name, 1);

	public SyntaxList<XmlAttributeSyntax> Attributes => new SyntaxList<XmlAttributeSyntax>(GetRed(ref attributes, 2));

	public SyntaxToken SlashGreaterThanToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlEmptyElementSyntax)Green).slashGreaterThanToken, GetChildPosition(3), GetChildIndex(3));

	internal XmlEmptyElementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitXmlEmptyElement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlEmptyElement(this);
	}

	public XmlEmptyElementSyntax Update(SyntaxToken lessThanToken, XmlNameSyntax name, SyntaxList<XmlAttributeSyntax> attributes, SyntaxToken slashGreaterThanToken)
	{
		if (lessThanToken != LessThanToken || name != Name || attributes != Attributes || slashGreaterThanToken != SlashGreaterThanToken)
		{
			XmlEmptyElementSyntax xmlEmptyElementSyntax = SyntaxFactory.XmlEmptyElement(lessThanToken, name, attributes, slashGreaterThanToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlEmptyElementSyntax.WithAnnotations(annotations);
			}
			return xmlEmptyElementSyntax;
		}
		return this;
	}

	public XmlEmptyElementSyntax WithLessThanToken(SyntaxToken lessThanToken)
	{
		return Update(lessThanToken, Name, Attributes, SlashGreaterThanToken);
	}

	public XmlEmptyElementSyntax WithName(XmlNameSyntax name)
	{
		return Update(LessThanToken, name, Attributes, SlashGreaterThanToken);
	}

	public XmlEmptyElementSyntax WithAttributes(SyntaxList<XmlAttributeSyntax> attributes)
	{
		return Update(LessThanToken, Name, attributes, SlashGreaterThanToken);
	}

	public XmlEmptyElementSyntax WithSlashGreaterThanToken(SyntaxToken slashGreaterThanToken)
	{
		return Update(LessThanToken, Name, Attributes, slashGreaterThanToken);
	}

	public XmlEmptyElementSyntax AddAttributes(params XmlAttributeSyntax[] items)
	{
		return WithAttributes(Attributes.AddRange(items));
	}
}
