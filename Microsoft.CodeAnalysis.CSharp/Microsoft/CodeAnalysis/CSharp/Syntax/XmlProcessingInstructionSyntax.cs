using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class XmlProcessingInstructionSyntax : XmlNodeSyntax
{
	private XmlNameSyntax name;

	public SyntaxToken StartProcessingInstructionToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlProcessingInstructionSyntax)Green).startProcessingInstructionToken, Position, 0);

	public XmlNameSyntax Name => GetRed(ref name, 1);

	public SyntaxTokenList TextTokens
	{
		get
		{
			GreenNode slot = Green.GetSlot(2);
			if (slot != null)
			{
				return new SyntaxTokenList(this, slot, GetChildPosition(2), GetChildIndex(2));
			}
			return default;
		}
	}

	public SyntaxToken EndProcessingInstructionToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.XmlProcessingInstructionSyntax)Green).endProcessingInstructionToken, GetChildPosition(3), GetChildIndex(3));

	internal XmlProcessingInstructionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref name, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return name;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlProcessingInstruction(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlProcessingInstruction(this);
	}

	public XmlProcessingInstructionSyntax Update(SyntaxToken startProcessingInstructionToken, XmlNameSyntax name, SyntaxTokenList textTokens, SyntaxToken endProcessingInstructionToken)
	{
		if (startProcessingInstructionToken != StartProcessingInstructionToken || name != Name || textTokens != TextTokens || endProcessingInstructionToken != EndProcessingInstructionToken)
		{
			XmlProcessingInstructionSyntax xmlProcessingInstructionSyntax = SyntaxFactory.XmlProcessingInstruction(startProcessingInstructionToken, name, textTokens, endProcessingInstructionToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlProcessingInstructionSyntax.WithAnnotations(annotations);
			}
			return xmlProcessingInstructionSyntax;
		}
		return this;
	}

	public XmlProcessingInstructionSyntax WithStartProcessingInstructionToken(SyntaxToken startProcessingInstructionToken)
	{
		return Update(startProcessingInstructionToken, Name, TextTokens, EndProcessingInstructionToken);
	}

	public XmlProcessingInstructionSyntax WithName(XmlNameSyntax name)
	{
		return Update(StartProcessingInstructionToken, name, TextTokens, EndProcessingInstructionToken);
	}

	public XmlProcessingInstructionSyntax WithTextTokens(SyntaxTokenList textTokens)
	{
		return Update(StartProcessingInstructionToken, Name, textTokens, EndProcessingInstructionToken);
	}

	public XmlProcessingInstructionSyntax WithEndProcessingInstructionToken(SyntaxToken endProcessingInstructionToken)
	{
		return Update(StartProcessingInstructionToken, Name, TextTokens, endProcessingInstructionToken);
	}

	public XmlProcessingInstructionSyntax AddTextTokens(params SyntaxToken[] items)
	{
		return WithTextTokens(TextTokens.AddRange(items));
	}
}
