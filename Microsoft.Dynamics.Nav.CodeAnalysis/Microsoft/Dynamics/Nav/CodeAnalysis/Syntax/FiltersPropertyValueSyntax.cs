using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FiltersPropertyValueSyntax : PropertyValueSyntax
{
	private WhereExpressionSyntax _tableFilter;

	public WhereExpressionSyntax TableFilter => GetRedAtZero(ref _tableFilter);

	internal FiltersPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _tableFilter);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _tableFilter;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFiltersPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFiltersPropertyValue(this);
	}

	public FiltersPropertyValueSyntax Update(WhereExpressionSyntax tableFilter)
	{
		if (tableFilter != TableFilter)
		{
			FiltersPropertyValueSyntax filtersPropertyValueSyntax = SyntaxFactory.FiltersPropertyValue(tableFilter);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return filtersPropertyValueSyntax.WithAnnotations(annotations);
			}
			return filtersPropertyValueSyntax;
		}
		return this;
	}

	public FiltersPropertyValueSyntax WithTableFilter(WhereExpressionSyntax tableFilter)
	{
		return Update(tableFilter);
	}
}
