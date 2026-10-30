using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalTableRelationPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalIfTableRelationSyntax _ifExpression;

	internal readonly InternalMemberAccessOrIdentifierPropertyValueSyntax _relatedTableField;

	internal readonly InternalWhereExpressionSyntax _tableFilter;

	internal readonly InternalElseTableRelationSyntax _elseExpression;

	internal override SyntaxNodeDefinition Definition => TableRelationPropertyValueSyntaxDefinition.Instance.Value;

	public InternalIfTableRelationSyntax IfExpression => _ifExpression;

	public InternalMemberAccessOrIdentifierPropertyValueSyntax RelatedTableField => _relatedTableField;

	public InternalWhereExpressionSyntax TableFilter => _tableFilter;

	public InternalElseTableRelationSyntax ElseExpression => _elseExpression;

	internal InternalTableRelationPropertyValueSyntax(SyntaxKind kind, InternalIfTableRelationSyntax ifExpression, InternalMemberAccessOrIdentifierPropertyValueSyntax relatedTableField, InternalWhereExpressionSyntax tableFilter, InternalElseTableRelationSyntax elseExpression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		if (ifExpression != null)
		{
			AdjustFlagsAndWidth(ifExpression);
			_ifExpression = ifExpression;
		}
		AdjustFlagsAndWidth(relatedTableField);
		_relatedTableField = relatedTableField;
		if (tableFilter != null)
		{
			AdjustFlagsAndWidth(tableFilter);
			_tableFilter = tableFilter;
		}
		if (elseExpression != null)
		{
			AdjustFlagsAndWidth(elseExpression);
			_elseExpression = elseExpression;
		}
	}

	internal InternalTableRelationPropertyValueSyntax(SyntaxKind kind, InternalIfTableRelationSyntax ifExpression, InternalMemberAccessOrIdentifierPropertyValueSyntax relatedTableField, InternalWhereExpressionSyntax tableFilter, InternalElseTableRelationSyntax elseExpression, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		if (ifExpression != null)
		{
			AdjustFlagsAndWidth(ifExpression);
			_ifExpression = ifExpression;
		}
		AdjustFlagsAndWidth(relatedTableField);
		_relatedTableField = relatedTableField;
		if (tableFilter != null)
		{
			AdjustFlagsAndWidth(tableFilter);
			_tableFilter = tableFilter;
		}
		if (elseExpression != null)
		{
			AdjustFlagsAndWidth(elseExpression);
			_elseExpression = elseExpression;
		}
	}

	internal InternalTableRelationPropertyValueSyntax(SyntaxKind kind, InternalIfTableRelationSyntax ifExpression, InternalMemberAccessOrIdentifierPropertyValueSyntax relatedTableField, InternalWhereExpressionSyntax tableFilter, InternalElseTableRelationSyntax elseExpression)
		: base(kind)
	{
		SlotCount = 4;
		if (ifExpression != null)
		{
			AdjustFlagsAndWidth(ifExpression);
			_ifExpression = ifExpression;
		}
		AdjustFlagsAndWidth(relatedTableField);
		_relatedTableField = relatedTableField;
		if (tableFilter != null)
		{
			AdjustFlagsAndWidth(tableFilter);
			_tableFilter = tableFilter;
		}
		if (elseExpression != null)
		{
			AdjustFlagsAndWidth(elseExpression);
			_elseExpression = elseExpression;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_ifExpression, 
			1 => _relatedTableField, 
			2 => _tableFilter, 
			3 => _elseExpression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new TableRelationPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableRelationPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitTableRelationPropertyValue(this);
	}

	public InternalTableRelationPropertyValueSyntax Update(InternalIfTableRelationSyntax ifExpression, InternalMemberAccessOrIdentifierPropertyValueSyntax relatedTableField, InternalWhereExpressionSyntax tableFilter, InternalElseTableRelationSyntax elseExpression)
	{
		if (ifExpression != IfExpression || relatedTableField != RelatedTableField || tableFilter != TableFilter || elseExpression != ElseExpression)
		{
			return InternalSyntaxFactory.TableRelationPropertyValue(ifExpression, relatedTableField, tableFilter, elseExpression).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalTableRelationPropertyValueSyntax(Kind, _ifExpression, _relatedTableField, _tableFilter, _elseExpression, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalTableRelationPropertyValueSyntax(Kind, _ifExpression, _relatedTableField, _tableFilter, _elseExpression, GetDiagnostics(), annotations);
	}
}
