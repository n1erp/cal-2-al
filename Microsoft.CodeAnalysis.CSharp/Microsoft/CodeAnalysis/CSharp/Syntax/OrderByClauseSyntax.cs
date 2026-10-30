using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class OrderByClauseSyntax : QueryClauseSyntax
{
	private SyntaxNode orderings;

	public SyntaxToken OrderByKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OrderByClauseSyntax)Green).orderByKeyword, Position, 0);

	public SeparatedSyntaxList<OrderingSyntax> Orderings
	{
		get
		{
			SyntaxNode red = GetRed(ref orderings, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<OrderingSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	internal OrderByClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref orderings, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return orderings;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOrderByClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOrderByClause(this);
	}

	public OrderByClauseSyntax Update(SyntaxToken orderByKeyword, SeparatedSyntaxList<OrderingSyntax> orderings)
	{
		if (orderByKeyword != OrderByKeyword || orderings != Orderings)
		{
			OrderByClauseSyntax orderByClauseSyntax = SyntaxFactory.OrderByClause(orderByKeyword, orderings);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return orderByClauseSyntax.WithAnnotations(annotations);
			}
			return orderByClauseSyntax;
		}
		return this;
	}

	public OrderByClauseSyntax WithOrderByKeyword(SyntaxToken orderByKeyword)
	{
		return Update(orderByKeyword, Orderings);
	}

	public OrderByClauseSyntax WithOrderings(SeparatedSyntaxList<OrderingSyntax> orderings)
	{
		return Update(OrderByKeyword, orderings);
	}

	public OrderByClauseSyntax AddOrderings(params OrderingSyntax[] items)
	{
		return WithOrderings(Orderings.AddRange(items));
	}
}
