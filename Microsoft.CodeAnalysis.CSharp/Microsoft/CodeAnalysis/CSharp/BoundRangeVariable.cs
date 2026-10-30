using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundRangeVariable : BoundExpression
{
	public override Symbol ExpressionSymbol => RangeVariableSymbol;

	public RangeVariableSymbol RangeVariableSymbol { get; }

	public BoundExpression Value { get; }

	public BoundRangeVariable(SyntaxNode syntax, RangeVariableSymbol rangeVariableSymbol, BoundExpression value, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.RangeVariable, syntax, type, hasErrors || value.HasErrors())
	{
		RangeVariableSymbol = rangeVariableSymbol;
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitRangeVariable(this);
	}

	public BoundRangeVariable Update(RangeVariableSymbol rangeVariableSymbol, BoundExpression value, TypeSymbol type)
	{
		if (rangeVariableSymbol != RangeVariableSymbol || value != Value || type != Type)
		{
			return new BoundRangeVariable(Syntax, rangeVariableSymbol, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
