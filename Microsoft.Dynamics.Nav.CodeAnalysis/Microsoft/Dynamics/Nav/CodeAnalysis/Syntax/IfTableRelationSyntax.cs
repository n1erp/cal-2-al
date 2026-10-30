using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class IfTableRelationSyntax : PropertyExpressionSyntax
{
	private TableFilterExpressionSyntax _ifTableRelationCondition;

	public SyntaxToken IfKeywordToken => new SyntaxToken(this, ((InternalIfTableRelationSyntax)Node)._ifKeywordToken, Position, 0);

	public TableFilterExpressionSyntax IfTableRelationCondition => GetRed(ref _ifTableRelationCondition, 1);

	internal IfTableRelationSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _ifTableRelationCondition, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _ifTableRelationCondition;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIfTableRelation(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitIfTableRelation(this);
	}

	public IfTableRelationSyntax Update(SyntaxToken ifKeywordToken, TableFilterExpressionSyntax ifTableRelationCondition)
	{
		if (ifKeywordToken != IfKeywordToken || ifTableRelationCondition != IfTableRelationCondition)
		{
			IfTableRelationSyntax ifTableRelationSyntax = SyntaxFactory.IfTableRelation(ifKeywordToken, ifTableRelationCondition);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return ifTableRelationSyntax.WithAnnotations(annotations);
			}
			return ifTableRelationSyntax;
		}
		return this;
	}

	public IfTableRelationSyntax WithIfKeywordToken(SyntaxToken ifKeywordToken)
	{
		return Update(ifKeywordToken, IfTableRelationCondition);
	}

	public IfTableRelationSyntax WithIfTableRelationCondition(TableFilterExpressionSyntax ifTableRelationCondition)
	{
		return Update(IfKeywordToken, ifTableRelationCondition);
	}

	public IfTableRelationSyntax AddIfTableRelationConditionConditions(params PropertyExpressionSyntax[] items)
	{
		return WithIfTableRelationCondition(IfTableRelationCondition.WithConditions(IfTableRelationCondition.Conditions.AddRange(items)));
	}
}
