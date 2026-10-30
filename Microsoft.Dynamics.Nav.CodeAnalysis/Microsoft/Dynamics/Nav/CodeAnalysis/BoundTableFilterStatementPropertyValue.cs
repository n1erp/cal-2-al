using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTableFilterStatementPropertyValue : BoundPropertyValue
{
	public ITypeSymbol DeclaringTableSymbol { get; private set; }

	public BoundTableRelationConditionExpression TableFilter { get; private set; }

	internal override object ValueObject => null;

	internal override string ValueText => string.Empty;

	public BoundTableFilterStatementPropertyValue(SyntaxNode syntax, ITypeSymbol declaringTableSymbol, BoundTableRelationConditionExpression tableFilter, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TableFilterStatementPropertyValue, syntax, type, hasErrors || tableFilter.HasErrors())
	{
		DeclaringTableSymbol = declaringTableSymbol;
		TableFilter = tableFilter;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTableFilterStatementPropertyValue(this);
	}

	public BoundTableFilterStatementPropertyValue Update(ITypeSymbol declaringTableSymbol, BoundTableRelationConditionExpression tableFilter, TypeSymbol type)
	{
		if (declaringTableSymbol != DeclaringTableSymbol || tableFilter != TableFilter || type != Type)
		{
			return new BoundTableFilterStatementPropertyValue(Syntax, declaringTableSymbol, tableFilter, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
