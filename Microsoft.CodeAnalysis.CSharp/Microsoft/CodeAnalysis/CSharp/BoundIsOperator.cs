using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundIsOperator : BoundExpression
{
	public BoundExpression Operand { get; }

	public BoundTypeExpression TargetType { get; }

	public Conversion Conversion { get; }

	public BoundIsOperator(SyntaxNode syntax, BoundExpression operand, BoundTypeExpression targetType, Conversion conversion, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.IsOperator, syntax, type, hasErrors || operand.HasErrors() || targetType.HasErrors())
	{
		Operand = operand;
		TargetType = targetType;
		Conversion = conversion;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitIsOperator(this);
	}

	public BoundIsOperator Update(BoundExpression operand, BoundTypeExpression targetType, Conversion conversion, TypeSymbol type)
	{
		if (operand != Operand || targetType != TargetType || conversion != Conversion || type != Type)
		{
			return new BoundIsOperator(Syntax, operand, targetType, conversion, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
