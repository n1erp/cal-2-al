using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ConditionalExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax condition;

	private ExpressionSyntax whenTrue;

	private ExpressionSyntax whenFalse;

	public ExpressionSyntax Condition => GetRedAtZero(ref condition);

	public SyntaxToken QuestionToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConditionalExpressionSyntax)Green).questionToken, GetChildPosition(1), GetChildIndex(1));

	public ExpressionSyntax WhenTrue => GetRed(ref whenTrue, 2);

	public SyntaxToken ColonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ConditionalExpressionSyntax)Green).colonToken, GetChildPosition(3), GetChildIndex(3));

	public ExpressionSyntax WhenFalse => GetRed(ref whenFalse, 4);

	internal ConditionalExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref condition), 
			2 => GetRed(ref whenTrue, 2), 
			4 => GetRed(ref whenFalse, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)condition, 
			2 => whenTrue, 
			4 => whenFalse, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConditionalExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConditionalExpression(this);
	}

	public ConditionalExpressionSyntax Update(ExpressionSyntax condition, SyntaxToken questionToken, ExpressionSyntax whenTrue, SyntaxToken colonToken, ExpressionSyntax whenFalse)
	{
		if (condition != Condition || questionToken != QuestionToken || whenTrue != WhenTrue || colonToken != ColonToken || whenFalse != WhenFalse)
		{
			ConditionalExpressionSyntax conditionalExpressionSyntax = SyntaxFactory.ConditionalExpression(condition, questionToken, whenTrue, colonToken, whenFalse);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return conditionalExpressionSyntax.WithAnnotations(annotations);
			}
			return conditionalExpressionSyntax;
		}
		return this;
	}

	public ConditionalExpressionSyntax WithCondition(ExpressionSyntax condition)
	{
		return Update(condition, QuestionToken, WhenTrue, ColonToken, WhenFalse);
	}

	public ConditionalExpressionSyntax WithQuestionToken(SyntaxToken questionToken)
	{
		return Update(Condition, questionToken, WhenTrue, ColonToken, WhenFalse);
	}

	public ConditionalExpressionSyntax WithWhenTrue(ExpressionSyntax whenTrue)
	{
		return Update(Condition, QuestionToken, whenTrue, ColonToken, WhenFalse);
	}

	public ConditionalExpressionSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(Condition, QuestionToken, WhenTrue, colonToken, WhenFalse);
	}

	public ConditionalExpressionSyntax WithWhenFalse(ExpressionSyntax whenFalse)
	{
		return Update(Condition, QuestionToken, WhenTrue, ColonToken, whenFalse);
	}
}
