using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class TableViewPropertyValueSyntax : PropertyValueSyntax
{
	private SortingSyntax _sorting;

	private OrderSyntax _order;

	private WhereExpressionSyntax _tableFilter;

	public SortingSyntax Sorting => GetRedAtZero(ref _sorting);

	public OrderSyntax Order => GetRed(ref _order, 1);

	public WhereExpressionSyntax TableFilter => GetRed(ref _tableFilter, 2);

	internal TableViewPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _sorting), 
			1 => GetRed(ref _order, 1), 
			2 => GetRed(ref _tableFilter, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_sorting, 
			1 => _order, 
			2 => _tableFilter, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableViewPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTableViewPropertyValue(this);
	}

	public TableViewPropertyValueSyntax Update(SortingSyntax sorting, OrderSyntax order, WhereExpressionSyntax tableFilter)
	{
		if (sorting != Sorting || order != Order || tableFilter != TableFilter)
		{
			TableViewPropertyValueSyntax tableViewPropertyValueSyntax = SyntaxFactory.TableViewPropertyValue(sorting, order, tableFilter);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return tableViewPropertyValueSyntax.WithAnnotations(annotations);
			}
			return tableViewPropertyValueSyntax;
		}
		return this;
	}

	public TableViewPropertyValueSyntax WithSorting(SortingSyntax sorting)
	{
		return Update(sorting, Order, TableFilter);
	}

	public TableViewPropertyValueSyntax WithOrder(OrderSyntax order)
	{
		return Update(Sorting, order, TableFilter);
	}

	public TableViewPropertyValueSyntax WithTableFilter(WhereExpressionSyntax tableFilter)
	{
		return Update(Sorting, Order, tableFilter);
	}

	public TableViewPropertyValueSyntax AddSortingSortingFields(params IdentifierNameSyntax[] items)
	{
		SortingSyntax sortingSyntax = Sorting ?? SyntaxFactory.Sorting();
		return WithSorting(sortingSyntax.WithSortingFields(sortingSyntax.SortingFields.AddRange(items)));
	}
}
