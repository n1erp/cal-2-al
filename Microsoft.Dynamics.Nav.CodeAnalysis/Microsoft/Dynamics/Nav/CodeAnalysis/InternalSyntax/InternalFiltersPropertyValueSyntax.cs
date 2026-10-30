using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalFiltersPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalWhereExpressionSyntax _tableFilter;

	internal override SyntaxNodeDefinition Definition => FiltersPropertyValueSyntaxDefinition.Instance.Value;

	public InternalWhereExpressionSyntax TableFilter => _tableFilter;

	internal InternalFiltersPropertyValueSyntax(SyntaxKind kind, InternalWhereExpressionSyntax tableFilter, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(tableFilter);
		_tableFilter = tableFilter;
	}

	internal InternalFiltersPropertyValueSyntax(SyntaxKind kind, InternalWhereExpressionSyntax tableFilter, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(tableFilter);
		_tableFilter = tableFilter;
	}

	internal InternalFiltersPropertyValueSyntax(SyntaxKind kind, InternalWhereExpressionSyntax tableFilter)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(tableFilter);
		_tableFilter = tableFilter;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _tableFilter;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new FiltersPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFiltersPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitFiltersPropertyValue(this);
	}

	public InternalFiltersPropertyValueSyntax Update(InternalWhereExpressionSyntax tableFilter)
	{
		if (tableFilter != TableFilter)
		{
			return InternalSyntaxFactory.FiltersPropertyValue(tableFilter).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalFiltersPropertyValueSyntax(Kind, _tableFilter, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalFiltersPropertyValueSyntax(Kind, _tableFilter, GetDiagnostics(), annotations);
	}
}
