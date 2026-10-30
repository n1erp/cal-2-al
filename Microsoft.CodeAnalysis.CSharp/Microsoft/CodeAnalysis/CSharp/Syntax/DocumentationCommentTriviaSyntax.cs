using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class DocumentationCommentTriviaSyntax : StructuredTriviaSyntax
{
	private SyntaxNode content;

	public SyntaxList<XmlNodeSyntax> Content => new SyntaxList<XmlNodeSyntax>(GetRed(ref content, 0));

	public SyntaxToken EndOfComment => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DocumentationCommentTriviaSyntax)Green).endOfComment, GetChildPosition(1), GetChildIndex(1));

	internal DocumentationCommentTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref content);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return content;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDocumentationCommentTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDocumentationCommentTrivia(this);
	}

	public DocumentationCommentTriviaSyntax Update(SyntaxList<XmlNodeSyntax> content, SyntaxToken endOfComment)
	{
		if (content != Content || endOfComment != EndOfComment)
		{
			DocumentationCommentTriviaSyntax documentationCommentTriviaSyntax = SyntaxFactory.DocumentationCommentTrivia(Kind(), content, endOfComment);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return documentationCommentTriviaSyntax.WithAnnotations(annotations);
			}
			return documentationCommentTriviaSyntax;
		}
		return this;
	}

	public DocumentationCommentTriviaSyntax WithContent(SyntaxList<XmlNodeSyntax> content)
	{
		return Update(content, EndOfComment);
	}

	public DocumentationCommentTriviaSyntax WithEndOfComment(SyntaxToken endOfComment)
	{
		return Update(Content, endOfComment);
	}

	public DocumentationCommentTriviaSyntax AddContent(params XmlNodeSyntax[] items)
	{
		return WithContent(Content.AddRange(items));
	}
}
