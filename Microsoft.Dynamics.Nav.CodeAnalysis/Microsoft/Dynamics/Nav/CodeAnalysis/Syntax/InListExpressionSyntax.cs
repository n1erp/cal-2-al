using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class InListExpressionSyntax : CodeExpressionSyntax
{
	private CodeExpressionSyntax _expression;

	private BracketedArgumentListSyntax _listExpression;

	public CodeExpressionSyntax Expression => GetRedAtZero(ref _expression);

	public SyntaxToken InKeywordToken => new SyntaxToken(this, ((InternalInListExpressionSyntax)Node)._inKeywordToken, GetChildPosition(1), GetChildIndex(1));

	public BracketedArgumentListSyntax ListExpression => GetRed(ref _listExpression, 2);

	internal InListExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _expression), 
			2 => GetRed(ref _listExpression, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_expression, 
			2 => _listExpression, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInListExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInListExpression(this);
	}

	public InListExpressionSyntax Update(CodeExpressionSyntax expression, SyntaxToken inKeywordToken, BracketedArgumentListSyntax listExpression)
	{
		if (expression != Expression || inKeywordToken != InKeywordToken || listExpression != ListExpression)
		{
			InListExpressionSyntax inListExpressionSyntax = SyntaxFactory.InListExpression(expression, inKeywordToken, listExpression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return inListExpressionSyntax.WithAnnotations(annotations);
			}
			return inListExpressionSyntax;
		}
		return this;
	}

	public InListExpressionSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(expression, InKeywordToken, ListExpression);
	}

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WithIn")]
	public InListExpressionSyntax WithInKeywordToken(SyntaxToken inKeywordToken)
	{
		return Update(Expression, inKeywordToken, ListExpression);
	}

	public InListExpressionSyntax WithListExpression(BracketedArgumentListSyntax listExpression)
	{
		return Update(Expression, InKeywordToken, listExpression);
	}

	public InListExpressionSyntax AddListExpressionArguments(params CodeExpressionSyntax[] items)
	{
		return WithListExpression(ListExpression.WithArguments(ListExpression.Arguments.AddRange(items)));
	}
}
