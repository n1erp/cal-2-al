using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ElifDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
{
	private ExpressionSyntax condition;

	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ElifDirectiveTriviaSyntax)Green).hashToken, Position, 0);

	public SyntaxToken ElifKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ElifDirectiveTriviaSyntax)Green).elifKeyword, GetChildPosition(1), GetChildIndex(1));

	public override ExpressionSyntax Condition => GetRed(ref condition, 2);

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ElifDirectiveTriviaSyntax)Green).endOfDirectiveToken, GetChildPosition(3), GetChildIndex(3));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ElifDirectiveTriviaSyntax)Green).IsActive;

	public override bool BranchTaken => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ElifDirectiveTriviaSyntax)Green).BranchTaken;

	public override bool ConditionValue => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ElifDirectiveTriviaSyntax)Green).ConditionValue;

	internal ElifDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitElifDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitElifDirectiveTrivia(this);
	}

	public ElifDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken elifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
	{
		if (hashToken != HashToken || elifKeyword != ElifKeyword || condition != Condition || endOfDirectiveToken != EndOfDirectiveToken)
		{
			ElifDirectiveTriviaSyntax elifDirectiveTriviaSyntax = SyntaxFactory.ElifDirectiveTrivia(hashToken, elifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return elifDirectiveTriviaSyntax.WithAnnotations(annotations);
			}
			return elifDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new ElifDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, ElifKeyword, Condition, EndOfDirectiveToken, IsActive, BranchTaken, ConditionValue);
	}

	public ElifDirectiveTriviaSyntax WithElifKeyword(SyntaxToken elifKeyword)
	{
		return Update(HashToken, elifKeyword, Condition, EndOfDirectiveToken, IsActive, BranchTaken, ConditionValue);
	}

	internal override ConditionalDirectiveTriviaSyntax WithConditionCore(ExpressionSyntax condition)
	{
		return WithCondition(condition);
	}

	public new ElifDirectiveTriviaSyntax WithCondition(ExpressionSyntax condition)
	{
		return Update(HashToken, ElifKeyword, condition, EndOfDirectiveToken, IsActive, BranchTaken, ConditionValue);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new ElifDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, ElifKeyword, Condition, endOfDirectiveToken, IsActive, BranchTaken, ConditionValue);
	}

	public ElifDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, ElifKeyword, Condition, EndOfDirectiveToken, isActive, BranchTaken, ConditionValue);
	}

	public ElifDirectiveTriviaSyntax WithBranchTaken(bool branchTaken)
	{
		return Update(HashToken, ElifKeyword, Condition, EndOfDirectiveToken, IsActive, branchTaken, ConditionValue);
	}

	public ElifDirectiveTriviaSyntax WithConditionValue(bool conditionValue)
	{
		return Update(HashToken, ElifKeyword, Condition, EndOfDirectiveToken, IsActive, BranchTaken, conditionValue);
	}
}
