using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class WhereExpressionSyntax : PropertyExpressionSyntax
{
	private TableFilterExpressionSyntax _filter;

	public SyntaxToken WhereKeywordToken => new SyntaxToken(this, ((InternalWhereExpressionSyntax)Node)._whereKeywordToken, Position, 0);

	public TableFilterExpressionSyntax Filter => GetRed(ref _filter, 1);

	internal WhereExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _filter, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _filter;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWhereExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitWhereExpression(this);
	}

	public WhereExpressionSyntax Update(SyntaxToken whereKeywordToken, TableFilterExpressionSyntax filter)
	{
		if (whereKeywordToken != WhereKeywordToken || filter != Filter)
		{
			WhereExpressionSyntax whereExpressionSyntax = SyntaxFactory.WhereExpression(whereKeywordToken, filter);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return whereExpressionSyntax.WithAnnotations(annotations);
			}
			return whereExpressionSyntax;
		}
		return this;
	}

	public WhereExpressionSyntax WithWhereKeywordToken(SyntaxToken whereKeywordToken)
	{
		return Update(whereKeywordToken, Filter);
	}

	public WhereExpressionSyntax WithFilter(TableFilterExpressionSyntax filter)
	{
		return Update(WhereKeywordToken, filter);
	}

	public WhereExpressionSyntax AddFilterConditions(params PropertyExpressionSyntax[] items)
	{
		return WithFilter(Filter.WithConditions(Filter.Conditions.AddRange(items)));
	}
}
