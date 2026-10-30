using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundUnaryOperator : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override ConstantValue ConstantValue => ConstantValueOpt;

	public override Symbol ExpressionSymbol => MethodOpt;

	public ImmutableArray<MethodSymbol> OriginalUserDefinedOperatorsOpt { get; }

	public UnaryOperatorKind OperatorKind { get; }

	public BoundExpression Operand { get; }

	public ConstantValue ConstantValueOpt { get; }

	public MethodSymbol MethodOpt { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	internal BoundUnaryOperator(SyntaxNode syntax, UnaryOperatorKind operatorKind, BoundExpression operand, ConstantValue constantValueOpt, MethodSymbol methodOpt, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type, bool hasErrors = false)
		: this(syntax, operatorKind, operand, constantValueOpt, methodOpt, resultKind, type, hasErrors)
	{
		OriginalUserDefinedOperatorsOpt = originalUserDefinedOperatorsOpt;
	}

	public BoundUnaryOperator(SyntaxNode syntax, UnaryOperatorKind operatorKind, BoundExpression operand, ConstantValue constantValueOpt, MethodSymbol methodOpt, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.UnaryOperator, syntax, type, hasErrors || operand.HasErrors())
	{
		OperatorKind = operatorKind;
		Operand = operand;
		ConstantValueOpt = constantValueOpt;
		MethodOpt = methodOpt;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUnaryOperator(this);
	}

	public BoundUnaryOperator Update(UnaryOperatorKind operatorKind, BoundExpression operand, ConstantValue constantValueOpt, MethodSymbol methodOpt, LookupResultKind resultKind, TypeSymbol type)
	{
		if (operatorKind != OperatorKind || operand != Operand || constantValueOpt != ConstantValueOpt || methodOpt != MethodOpt || resultKind != ResultKind || type != Type)
		{
			return new BoundUnaryOperator(Syntax, operatorKind, operand, constantValueOpt, methodOpt, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
