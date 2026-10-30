using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class XmlElementSyntax : XmlNodeSyntax
{
	private XmlElementStartTagSyntax startTag;

	private SyntaxNode content;

	private XmlElementEndTagSyntax endTag;

	public XmlElementStartTagSyntax StartTag => GetRedAtZero(ref startTag);

	public SyntaxList<XmlNodeSyntax> Content => new SyntaxList<XmlNodeSyntax>(GetRed(ref content, 1));

	public XmlElementEndTagSyntax EndTag => GetRed(ref endTag, 2);

	internal XmlElementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref startTag), 
			1 => GetRed(ref content, 1), 
			2 => GetRed(ref endTag, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => startTag, 
			1 => content, 
			2 => endTag, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlElement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlElement(this);
	}

	public XmlElementSyntax Update(XmlElementStartTagSyntax startTag, SyntaxList<XmlNodeSyntax> content, XmlElementEndTagSyntax endTag)
	{
		if (startTag != StartTag || content != Content || endTag != EndTag)
		{
			XmlElementSyntax xmlElementSyntax = SyntaxFactory.XmlElement(startTag, content, endTag);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlElementSyntax.WithAnnotations(annotations);
			}
			return xmlElementSyntax;
		}
		return this;
	}

	public XmlElementSyntax WithStartTag(XmlElementStartTagSyntax startTag)
	{
		return Update(startTag, Content, EndTag);
	}

	public XmlElementSyntax WithContent(SyntaxList<XmlNodeSyntax> content)
	{
		return Update(StartTag, content, EndTag);
	}

	public XmlElementSyntax WithEndTag(XmlElementEndTagSyntax endTag)
	{
		return Update(StartTag, Content, endTag);
	}

	public XmlElementSyntax AddStartTagAttributes(params XmlAttributeSyntax[] items)
	{
		return WithStartTag(StartTag.WithAttributes(StartTag.Attributes.AddRange(items)));
	}

	public XmlElementSyntax AddContent(params XmlNodeSyntax[] items)
	{
		return WithContent(Content.AddRange(items));
	}
}
