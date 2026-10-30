using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class XmlCrefAttributeSyntax : XmlAttributeSyntax
{
	private XmlNameSyntax name;

	private CrefSyntax cref;

	public override XmlNameSyntax Name => GetRedAtZero(ref name);

	public override SyntaxToken EqualsToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlCrefAttributeSyntax)Green).equalsToken, GetChildPosition(1), GetChildIndex(1));

	public override SyntaxToken StartQuoteToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlCrefAttributeSyntax)Green).startQuoteToken, GetChildPosition(2), GetChildIndex(2));

	public CrefSyntax Cref => GetRed(ref cref, 3);

	public override SyntaxToken EndQuoteToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlCrefAttributeSyntax)Green).endQuoteToken, GetChildPosition(4), GetChildIndex(4));

	internal XmlCrefAttributeSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref name), 
			3 => GetRed(ref cref, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)name, 
			3 => cref, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlCrefAttribute(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlCrefAttribute(this);
	}

	public XmlCrefAttributeSyntax Update(XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, CrefSyntax cref, SyntaxToken endQuoteToken)
	{
		if (name != Name || equalsToken != EqualsToken || startQuoteToken != StartQuoteToken || cref != Cref || endQuoteToken != EndQuoteToken)
		{
			XmlCrefAttributeSyntax xmlCrefAttributeSyntax = SyntaxFactory.XmlCrefAttribute(name, equalsToken, startQuoteToken, cref, endQuoteToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlCrefAttributeSyntax.WithAnnotations(annotations);
			}
			return xmlCrefAttributeSyntax;
		}
		return this;
	}

	internal override XmlAttributeSyntax WithNameCore(XmlNameSyntax name)
	{
		return WithName(name);
	}

	public new XmlCrefAttributeSyntax WithName(XmlNameSyntax name)
	{
		return Update(name, EqualsToken, StartQuoteToken, Cref, EndQuoteToken);
	}

	internal override XmlAttributeSyntax WithEqualsTokenCore(SyntaxToken equalsToken)
	{
		return WithEqualsToken(equalsToken);
	}

	public new XmlCrefAttributeSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(Name, equalsToken, StartQuoteToken, Cref, EndQuoteToken);
	}

	internal override XmlAttributeSyntax WithStartQuoteTokenCore(SyntaxToken startQuoteToken)
	{
		return WithStartQuoteToken(startQuoteToken);
	}

	public new XmlCrefAttributeSyntax WithStartQuoteToken(SyntaxToken startQuoteToken)
	{
		return Update(Name, EqualsToken, startQuoteToken, Cref, EndQuoteToken);
	}

	public XmlCrefAttributeSyntax WithCref(CrefSyntax cref)
	{
		return Update(Name, EqualsToken, StartQuoteToken, cref, EndQuoteToken);
	}

	internal override XmlAttributeSyntax WithEndQuoteTokenCore(SyntaxToken endQuoteToken)
	{
		return WithEndQuoteToken(endQuoteToken);
	}

	public new XmlCrefAttributeSyntax WithEndQuoteToken(SyntaxToken endQuoteToken)
	{
		return Update(Name, EqualsToken, StartQuoteToken, Cref, endQuoteToken);
	}
}
