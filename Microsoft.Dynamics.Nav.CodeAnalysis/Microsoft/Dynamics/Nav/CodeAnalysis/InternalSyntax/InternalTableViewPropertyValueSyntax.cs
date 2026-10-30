using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalTableViewPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalSortingSyntax _sorting;

	internal readonly InternalOrderSyntax _order;

	internal readonly InternalWhereExpressionSyntax _tableFilter;

	internal override SyntaxNodeDefinition Definition => TableViewPropertyValueSyntaxDefinition.Instance.Value;

	public InternalSortingSyntax Sorting => _sorting;

	public InternalOrderSyntax Order => _order;

	public InternalWhereExpressionSyntax TableFilter => _tableFilter;

	internal InternalTableViewPropertyValueSyntax(SyntaxKind kind, InternalSortingSyntax sorting, InternalOrderSyntax order, InternalWhereExpressionSyntax tableFilter, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		if (sorting != null)
		{
			AdjustFlagsAndWidth(sorting);
			_sorting = sorting;
		}
		if (order != null)
		{
			AdjustFlagsAndWidth(order);
			_order = order;
		}
		if (tableFilter != null)
		{
			AdjustFlagsAndWidth(tableFilter);
			_tableFilter = tableFilter;
		}
	}

	internal InternalTableViewPropertyValueSyntax(SyntaxKind kind, InternalSortingSyntax sorting, InternalOrderSyntax order, InternalWhereExpressionSyntax tableFilter, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		if (sorting != null)
		{
			AdjustFlagsAndWidth(sorting);
			_sorting = sorting;
		}
		if (order != null)
		{
			AdjustFlagsAndWidth(order);
			_order = order;
		}
		if (tableFilter != null)
		{
			AdjustFlagsAndWidth(tableFilter);
			_tableFilter = tableFilter;
		}
	}

	internal InternalTableViewPropertyValueSyntax(SyntaxKind kind, InternalSortingSyntax sorting, InternalOrderSyntax order, InternalWhereExpressionSyntax tableFilter)
		: base(kind)
	{
		SlotCount = 3;
		if (sorting != null)
		{
			AdjustFlagsAndWidth(sorting);
			_sorting = sorting;
		}
		if (order != null)
		{
			AdjustFlagsAndWidth(order);
			_order = order;
		}
		if (tableFilter != null)
		{
			AdjustFlagsAndWidth(tableFilter);
			_tableFilter = tableFilter;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_sorting, 
			1 => _order, 
			2 => _tableFilter, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new TableViewPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableViewPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitTableViewPropertyValue(this);
	}

	public InternalTableViewPropertyValueSyntax Update(InternalSortingSyntax sorting, InternalOrderSyntax order, InternalWhereExpressionSyntax tableFilter)
	{
		if (sorting != Sorting || order != Order || tableFilter != TableFilter)
		{
			return InternalSyntaxFactory.TableViewPropertyValue(sorting, order, tableFilter).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalTableViewPropertyValueSyntax(Kind, _sorting, _order, _tableFilter, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalTableViewPropertyValueSyntax(Kind, _sorting, _order, _tableFilter, GetDiagnostics(), annotations);
	}
}
