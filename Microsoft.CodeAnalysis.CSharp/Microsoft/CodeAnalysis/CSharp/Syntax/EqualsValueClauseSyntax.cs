using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class EqualsValueClauseSyntax : CSharpSyntaxNode
{
	private ExpressionSyntax value;

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EqualsValueClauseSyntax)Green).equalsToken, Position, 0);

	public ExpressionSyntax Value => GetRed(ref value, 1);

	internal EqualsValueClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref value, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return value;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEqualsValueClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEqualsValueClause(this);
	}

	public EqualsValueClauseSyntax Update(SyntaxToken equalsToken, ExpressionSyntax value)
	{
		if (equalsToken != EqualsToken || value != Value)
		{
			EqualsValueClauseSyntax equalsValueClauseSyntax = SyntaxFactory.EqualsValueClause(equalsToken, value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return equalsValueClauseSyntax.WithAnnotations(annotations);
			}
			return equalsValueClauseSyntax;
		}
		return this;
	}

	public EqualsValueClauseSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(equalsToken, Value);
	}

	public EqualsValueClauseSyntax WithValue(ExpressionSyntax value)
	{
		return Update(EqualsToken, value);
	}
}
