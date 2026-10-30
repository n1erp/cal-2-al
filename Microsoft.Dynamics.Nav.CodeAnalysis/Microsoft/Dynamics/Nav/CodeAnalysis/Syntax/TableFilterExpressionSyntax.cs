using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class TableFilterExpressionSyntax : PropertyExpressionSyntax
{
	private SyntaxNode _conditions;

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalTableFilterExpressionSyntax)Node)._openParenthesisToken, Position, 0);

	public SeparatedSyntaxList<PropertyExpressionSyntax> Conditions
	{
		get
		{
			SyntaxNode red = GetRed(ref _conditions, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<PropertyExpressionSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalTableFilterExpressionSyntax)Node)._closeParenthesisToken, GetChildPosition(2), GetChildIndex(2));

	internal TableFilterExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _conditions, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _conditions;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableFilterExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTableFilterExpression(this);
	}

	public TableFilterExpressionSyntax Update(SyntaxToken openParenthesisToken, SeparatedSyntaxList<PropertyExpressionSyntax> conditions, SyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || conditions != Conditions || closeParenthesisToken != CloseParenthesisToken)
		{
			TableFilterExpressionSyntax tableFilterExpressionSyntax = SyntaxFactory.TableFilterExpression(openParenthesisToken, conditions, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return tableFilterExpressionSyntax.WithAnnotations(annotations);
			}
			return tableFilterExpressionSyntax;
		}
		return this;
	}

	public TableFilterExpressionSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(openParenthesisToken, Conditions, CloseParenthesisToken);
	}

	public TableFilterExpressionSyntax WithConditions(SeparatedSyntaxList<PropertyExpressionSyntax> conditions)
	{
		return Update(OpenParenthesisToken, conditions, CloseParenthesisToken);
	}

	public TableFilterExpressionSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(OpenParenthesisToken, Conditions, closeParenthesisToken);
	}

	public TableFilterExpressionSyntax AddConditions(params PropertyExpressionSyntax[] items)
	{
		return WithConditions(Conditions.AddRange(items));
	}
}
