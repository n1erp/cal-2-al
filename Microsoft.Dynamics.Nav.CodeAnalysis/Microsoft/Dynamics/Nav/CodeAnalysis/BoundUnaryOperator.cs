using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundUnaryOperator : BoundExpression, IUnaryOperatorExpression, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override ConstantValue ConstantValue => ConstantValueOpt;

	public override Symbol ExpressionSymbol => Type;

	public UnaryOperatorKind OperatorKind { get; private set; }

	public BoundExpression Operand { get; private set; }

	public ConstantValue ConstantValueOpt { get; private set; }

	public MethodSymbol MethodOpt { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	UnaryOperationKind IUnaryOperatorExpression.UnaryOperationKind => Expression.DeriveUnaryOperationKind(OperatorKind);

	IOperation IUnaryOperatorExpression.Operand => Operand;

	protected override OperationKind ExpressionKind => OperationKind.UnaryOperatorExpression;

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

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitUnaryOperatorExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitUnaryOperatorExpression(this, argument);
	}
}
