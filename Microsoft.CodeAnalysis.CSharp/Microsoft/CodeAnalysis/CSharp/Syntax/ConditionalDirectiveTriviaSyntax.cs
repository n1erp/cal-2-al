using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class ConditionalDirectiveTriviaSyntax : BranchingDirectiveTriviaSyntax
{
	public abstract ExpressionSyntax Condition { get; }

	public abstract bool ConditionValue { get; }

	internal ConditionalDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public ConditionalDirectiveTriviaSyntax WithCondition(ExpressionSyntax condition)
	{
		return WithConditionCore(condition);
	}

	internal abstract ConditionalDirectiveTriviaSyntax WithConditionCore(ExpressionSyntax condition);
}
