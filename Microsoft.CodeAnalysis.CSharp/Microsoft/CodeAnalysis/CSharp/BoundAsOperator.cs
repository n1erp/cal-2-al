using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundAsOperator : BoundExpression
{
	public BoundExpression Operand { get; }

	public BoundTypeExpression TargetType { get; }

	public Conversion Conversion { get; }

	public BoundAsOperator(SyntaxNode syntax, BoundExpression operand, BoundTypeExpression targetType, Conversion conversion, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.AsOperator, syntax, type, hasErrors || operand.HasErrors() || targetType.HasErrors())
	{
		Operand = operand;
		TargetType = targetType;
		Conversion = conversion;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAsOperator(this);
	}

	public BoundAsOperator Update(BoundExpression operand, BoundTypeExpression targetType, Conversion conversion, TypeSymbol type)
	{
		if (operand != Operand || targetType != TargetType || conversion != Conversion || type != Type)
		{
			return new BoundAsOperator(Syntax, operand, targetType, conversion, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
