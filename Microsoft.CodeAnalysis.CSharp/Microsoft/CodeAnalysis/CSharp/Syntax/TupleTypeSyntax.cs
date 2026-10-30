using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class TupleTypeSyntax : TypeSyntax
{
	private SyntaxNode elements;

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TupleTypeSyntax)Green).openParenToken, Position, 0);

	public SeparatedSyntaxList<TupleElementSyntax> Elements
	{
		get
		{
			SyntaxNode red = GetRed(ref elements, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<TupleElementSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TupleTypeSyntax)Green).closeParenToken, GetChildPosition(2), GetChildIndex(2));

	internal TupleTypeSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref elements, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return elements;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTupleType(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTupleType(this);
	}

	public TupleTypeSyntax Update(SyntaxToken openParenToken, SeparatedSyntaxList<TupleElementSyntax> elements, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || elements != Elements || closeParenToken != CloseParenToken)
		{
			TupleTypeSyntax tupleTypeSyntax = SyntaxFactory.TupleType(openParenToken, elements, closeParenToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return tupleTypeSyntax.WithAnnotations(annotations);
			}
			return tupleTypeSyntax;
		}
		return this;
	}

	public TupleTypeSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(openParenToken, Elements, CloseParenToken);
	}

	public TupleTypeSyntax WithElements(SeparatedSyntaxList<TupleElementSyntax> elements)
	{
		return Update(OpenParenToken, elements, CloseParenToken);
	}

	public TupleTypeSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(OpenParenToken, Elements, closeParenToken);
	}

	public TupleTypeSyntax AddElements(params TupleElementSyntax[] items)
	{
		return WithElements(Elements.AddRange(items));
	}
}
