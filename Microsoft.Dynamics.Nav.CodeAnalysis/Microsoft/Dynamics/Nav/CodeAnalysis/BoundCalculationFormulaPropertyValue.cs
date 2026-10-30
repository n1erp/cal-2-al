using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundCalculationFormulaPropertyValue : BoundPropertyValue
{
	public SyntaxKind Operator { get; private set; }

	public bool IsMinus { get; private set; }

	public BoundExpression FieldOrTableAccess { get; private set; }

	public BoundTableRelationConditionExpression TableFilter { get; private set; }

	internal override object ValueObject => null;

	internal override string ValueText => string.Empty;

	public BoundCalculationFormulaPropertyValue(SyntaxNode syntax, SyntaxKind @operator, bool isMinus, BoundExpression fieldOrTableAccess, BoundTableRelationConditionExpression tableFilter, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.CalculationFormulaPropertyValue, syntax, type, hasErrors || fieldOrTableAccess.HasErrors() || tableFilter.HasErrors())
	{
		Operator = @operator;
		IsMinus = isMinus;
		FieldOrTableAccess = fieldOrTableAccess;
		TableFilter = tableFilter;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCalculationFormulaPropertyValue(this);
	}

	public BoundCalculationFormulaPropertyValue Update(SyntaxKind @operator, bool isMinus, BoundExpression fieldOrTableAccess, BoundTableRelationConditionExpression tableFilter, TypeSymbol type)
	{
		if (@operator != Operator || isMinus != IsMinus || fieldOrTableAccess != FieldOrTableAccess || tableFilter != TableFilter || type != Type)
		{
			return new BoundCalculationFormulaPropertyValue(Syntax, @operator, isMinus, fieldOrTableAccess, tableFilter, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
