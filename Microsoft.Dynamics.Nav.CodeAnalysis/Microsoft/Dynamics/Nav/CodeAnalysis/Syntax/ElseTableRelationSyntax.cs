using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ElseTableRelationSyntax : PropertyExpressionSyntax
{
	private TableRelationPropertyValueSyntax _elseTableRelationCondition;

	public SyntaxToken ElseKeywordToken => new SyntaxToken(this, ((InternalElseTableRelationSyntax)Node)._elseKeywordToken, Position, 0);

	public TableRelationPropertyValueSyntax ElseTableRelationCondition => GetRed(ref _elseTableRelationCondition, 1);

	internal ElseTableRelationSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _elseTableRelationCondition, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _elseTableRelationCondition;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitElseTableRelation(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitElseTableRelation(this);
	}

	public ElseTableRelationSyntax Update(SyntaxToken elseKeywordToken, TableRelationPropertyValueSyntax elseTableRelationCondition)
	{
		if (elseKeywordToken != ElseKeywordToken || elseTableRelationCondition != ElseTableRelationCondition)
		{
			ElseTableRelationSyntax elseTableRelationSyntax = SyntaxFactory.ElseTableRelation(elseKeywordToken, elseTableRelationCondition);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return elseTableRelationSyntax.WithAnnotations(annotations);
			}
			return elseTableRelationSyntax;
		}
		return this;
	}

	public ElseTableRelationSyntax WithElseKeywordToken(SyntaxToken elseKeywordToken)
	{
		return Update(elseKeywordToken, ElseTableRelationCondition);
	}

	public ElseTableRelationSyntax WithElseTableRelationCondition(TableRelationPropertyValueSyntax elseTableRelationCondition)
	{
		return Update(ElseKeywordToken, elseTableRelationCondition);
	}
}
