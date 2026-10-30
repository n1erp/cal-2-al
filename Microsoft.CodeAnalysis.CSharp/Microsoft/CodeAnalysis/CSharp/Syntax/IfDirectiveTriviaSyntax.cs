using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class IfDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
{
	private ExpressionSyntax condition;

	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IfDirectiveTriviaSyntax)Green).hashToken, Position, 0);

	public SyntaxToken IfKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IfDirectiveTriviaSyntax)Green).ifKeyword, GetChildPosition(1), GetChildIndex(1));

	public override ExpressionSyntax Condition => GetRed(ref condition, 2);

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IfDirectiveTriviaSyntax)Green).endOfDirectiveToken, GetChildPosition(3), GetChildIndex(3));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IfDirectiveTriviaSyntax)Green).IsActive;

	public override bool BranchTaken => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IfDirectiveTriviaSyntax)Green).BranchTaken;

	public override bool ConditionValue => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IfDirectiveTriviaSyntax)Green).ConditionValue;

	internal IfDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref condition, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return condition;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIfDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIfDirectiveTrivia(this);
	}

	public IfDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken ifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
	{
		if (hashToken != HashToken || ifKeyword != IfKeyword || condition != Condition || endOfDirectiveToken != EndOfDirectiveToken)
		{
			IfDirectiveTriviaSyntax ifDirectiveTriviaSyntax = SyntaxFactory.IfDirectiveTrivia(hashToken, ifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return ifDirectiveTriviaSyntax.WithAnnotations(annotations);
			}
			return ifDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new IfDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, IfKeyword, Condition, EndOfDirectiveToken, IsActive, BranchTaken, ConditionValue);
	}

	public IfDirectiveTriviaSyntax WithIfKeyword(SyntaxToken ifKeyword)
	{
		return Update(HashToken, ifKeyword, Condition, EndOfDirectiveToken, IsActive, BranchTaken, ConditionValue);
	}

	internal override ConditionalDirectiveTriviaSyntax WithConditionCore(ExpressionSyntax condition)
	{
		return WithCondition(condition);
	}

	public new IfDirectiveTriviaSyntax WithCondition(ExpressionSyntax condition)
	{
		return Update(HashToken, IfKeyword, condition, EndOfDirectiveToken, IsActive, BranchTaken, ConditionValue);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new IfDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, IfKeyword, Condition, endOfDirectiveToken, IsActive, BranchTaken, ConditionValue);
	}

	public IfDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, IfKeyword, Condition, EndOfDirectiveToken, isActive, BranchTaken, ConditionValue);
	}

	public IfDirectiveTriviaSyntax WithBranchTaken(bool branchTaken)
	{
		return Update(HashToken, IfKeyword, Condition, EndOfDirectiveToken, IsActive, branchTaken, ConditionValue);
	}

	public IfDirectiveTriviaSyntax WithConditionValue(bool conditionValue)
	{
		return Update(HashToken, IfKeyword, Condition, EndOfDirectiveToken, IsActive, BranchTaken, conditionValue);
	}
}
