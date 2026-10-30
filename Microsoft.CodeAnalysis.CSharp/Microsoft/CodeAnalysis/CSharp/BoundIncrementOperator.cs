using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundIncrementOperator : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => MethodOpt;

	public ImmutableArray<MethodSymbol> OriginalUserDefinedOperatorsOpt { get; }

	public UnaryOperatorKind OperatorKind { get; }

	public BoundExpression Operand { get; }

	public MethodSymbol MethodOpt { get; }

	public Conversion OperandConversion { get; }

	public Conversion ResultConversion { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	public BoundIncrementOperator(SyntaxNode syntax, UnaryOperatorKind operatorKind, BoundExpression operand, MethodSymbol methodOpt, Conversion operandConversion, Conversion resultConversion, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type, bool hasErrors = false)
		: this(syntax, operatorKind, operand, methodOpt, operandConversion, resultConversion, resultKind, type, hasErrors)
	{
		OriginalUserDefinedOperatorsOpt = originalUserDefinedOperatorsOpt;
	}

	public BoundIncrementOperator(SyntaxNode syntax, UnaryOperatorKind operatorKind, BoundExpression operand, MethodSymbol methodOpt, Conversion operandConversion, Conversion resultConversion, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.IncrementOperator, syntax, type, hasErrors || operand.HasErrors())
	{
		OperatorKind = operatorKind;
		Operand = operand;
		MethodOpt = methodOpt;
		OperandConversion = operandConversion;
		ResultConversion = resultConversion;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitIncrementOperator(this);
	}

	public BoundIncrementOperator Update(UnaryOperatorKind operatorKind, BoundExpression operand, MethodSymbol methodOpt, Conversion operandConversion, Conversion resultConversion, LookupResultKind resultKind, TypeSymbol type)
	{
		if (operatorKind != OperatorKind || operand != Operand || methodOpt != MethodOpt || operandConversion != OperandConversion || resultConversion != ResultConversion || resultKind != ResultKind || type != Type)
		{
			return new BoundIncrementOperator(Syntax, operatorKind, operand, methodOpt, operandConversion, resultConversion, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
