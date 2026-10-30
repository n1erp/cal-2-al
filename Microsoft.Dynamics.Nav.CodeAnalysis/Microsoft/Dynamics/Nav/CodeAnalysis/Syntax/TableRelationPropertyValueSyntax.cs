using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class TableRelationPropertyValueSyntax : PropertyValueSyntax
{
	private IfTableRelationSyntax _ifExpression;

	private MemberAccessOrIdentifierPropertyValueSyntax _relatedTableField;

	private WhereExpressionSyntax _tableFilter;

	private ElseTableRelationSyntax _elseExpression;

	public IfTableRelationSyntax IfExpression => GetRedAtZero(ref _ifExpression);

	public MemberAccessOrIdentifierPropertyValueSyntax RelatedTableField => GetRed(ref _relatedTableField, 1);

	public WhereExpressionSyntax TableFilter => GetRed(ref _tableFilter, 2);

	public ElseTableRelationSyntax ElseExpression => GetRed(ref _elseExpression, 3);

	internal TableRelationPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _ifExpression), 
			1 => GetRed(ref _relatedTableField, 1), 
			2 => GetRed(ref _tableFilter, 2), 
			3 => GetRed(ref _elseExpression, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_ifExpression, 
			1 => _relatedTableField, 
			2 => _tableFilter, 
			3 => _elseExpression, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableRelationPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTableRelationPropertyValue(this);
	}

	public TableRelationPropertyValueSyntax Update(IfTableRelationSyntax ifExpression, MemberAccessOrIdentifierPropertyValueSyntax relatedTableField, WhereExpressionSyntax tableFilter, ElseTableRelationSyntax elseExpression)
	{
		if (ifExpression != IfExpression || relatedTableField != RelatedTableField || tableFilter != TableFilter || elseExpression != ElseExpression)
		{
			TableRelationPropertyValueSyntax tableRelationPropertyValueSyntax = SyntaxFactory.TableRelationPropertyValue(ifExpression, relatedTableField, tableFilter, elseExpression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return tableRelationPropertyValueSyntax.WithAnnotations(annotations);
			}
			return tableRelationPropertyValueSyntax;
		}
		return this;
	}

	public TableRelationPropertyValueSyntax WithIfExpression(IfTableRelationSyntax ifExpression)
	{
		return Update(ifExpression, RelatedTableField, TableFilter, ElseExpression);
	}

	public TableRelationPropertyValueSyntax WithRelatedTableField(MemberAccessOrIdentifierPropertyValueSyntax relatedTableField)
	{
		return Update(IfExpression, relatedTableField, TableFilter, ElseExpression);
	}

	public TableRelationPropertyValueSyntax WithTableFilter(WhereExpressionSyntax tableFilter)
	{
		return Update(IfExpression, RelatedTableField, tableFilter, ElseExpression);
	}

	public TableRelationPropertyValueSyntax WithElseExpression(ElseTableRelationSyntax elseExpression)
	{
		return Update(IfExpression, RelatedTableField, TableFilter, elseExpression);
	}
}
