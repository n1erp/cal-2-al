using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTableViewPropertyValue : BoundPropertyValue
{
	public ITypeSymbol DeclaringTableSymbol { get; private set; }

	public ImmutableArray<BoundExpression> Sorting { get; private set; }

	public bool? IsAscending { get; private set; }

	public BoundTableRelationConditionExpression TableFilter { get; private set; }

	internal override object ValueObject => null;

	internal override string ValueText => string.Empty;

	public BoundTableViewPropertyValue(SyntaxNode syntax, ITypeSymbol declaringTableSymbol, ImmutableArray<BoundExpression> sorting, bool? isAscending, BoundTableRelationConditionExpression tableFilter, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TableViewPropertyValue, syntax, type, hasErrors || sorting.HasErrors() || tableFilter.HasErrors())
	{
		DeclaringTableSymbol = declaringTableSymbol;
		Sorting = sorting;
		IsAscending = isAscending;
		TableFilter = tableFilter;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTableViewPropertyValue(this);
	}

	public BoundTableViewPropertyValue Update(ITypeSymbol declaringTableSymbol, ImmutableArray<BoundExpression> sorting, bool? isAscending, BoundTableRelationConditionExpression tableFilter, TypeSymbol type)
	{
		if (declaringTableSymbol == DeclaringTableSymbol && !(sorting != Sorting))
		{
			bool? flag = isAscending;
			bool? isAscending2 = IsAscending;
			if (flag == true == (isAscending2 == true) && flag.HasValue == isAscending2.HasValue && tableFilter == TableFilter && !(type != Type))
			{
				return this;
			}
		}
		return new BoundTableViewPropertyValue(Syntax, declaringTableSymbol, sorting, isAscending, tableFilter, type, HasErrors)
		{
			WasCompilerGenerated = WasCompilerGenerated
		};
	}
}
