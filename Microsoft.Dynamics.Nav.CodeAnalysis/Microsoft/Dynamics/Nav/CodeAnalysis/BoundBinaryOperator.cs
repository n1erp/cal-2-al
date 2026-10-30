using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundBinaryOperator : BoundExpression, IBinaryOperatorExpression, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override ConstantValue ConstantValue => ConstantValueOpt;

	public override Symbol ExpressionSymbol => Type;

	public BinaryOperatorKind OperatorKind { get; private set; }

	public BoundExpression Left { get; private set; }

	public BoundExpression Right { get; private set; }

	public ConstantValue ConstantValueOpt { get; private set; }

	public MethodSymbol MethodOpt { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	BinaryOperationKind IBinaryOperatorExpression.BinaryOperationKind => Expression.DeriveBinaryOperationKind(OperatorKind);

	IOperation IBinaryOperatorExpression.LeftOperand => Left;

	IOperation IBinaryOperatorExpression.RightOperand => Right;

	protected override OperationKind ExpressionKind
	{
		get
		{
			switch (OperatorKind & BinaryOperatorKind.OpMask)
			{
			case BinaryOperatorKind.Multiplication:
			case BinaryOperatorKind.Addition:
			case BinaryOperatorKind.Subtraction:
			case BinaryOperatorKind.Division:
			case BinaryOperatorKind.Remainder:
			case BinaryOperatorKind.Equal:
			case BinaryOperatorKind.NotEqual:
			case BinaryOperatorKind.GreaterThan:
			case BinaryOperatorKind.LessThan:
			case BinaryOperatorKind.GreaterThanOrEqual:
			case BinaryOperatorKind.LessThanOrEqual:
			case BinaryOperatorKind.And:
			case BinaryOperatorKind.Xor:
			case BinaryOperatorKind.Or:
				return OperationKind.BinaryOperatorExpression;
			default:
				return OperationKind.InvalidExpression;
			}
		}
	}

	public BoundBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, ConstantValue constantValueOpt, MethodSymbol methodOpt, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.BinaryOperator, syntax, type, hasErrors || left.HasErrors() || right.HasErrors())
	{
		OperatorKind = operatorKind;
		Left = left;
		Right = right;
		ConstantValueOpt = constantValueOpt;
		MethodOpt = methodOpt;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBinaryOperator(this);
	}

	public BoundBinaryOperator Update(BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, ConstantValue constantValueOpt, MethodSymbol methodOpt, LookupResultKind resultKind, TypeSymbol type)
	{
		if (operatorKind != OperatorKind || left != Left || right != Right || constantValueOpt != ConstantValueOpt || methodOpt != MethodOpt || resultKind != ResultKind || type != Type)
		{
			return new BoundBinaryOperator(Syntax, operatorKind, left, right, constantValueOpt, methodOpt, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitBinaryOperatorExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitBinaryOperatorExpression(this, argument);
	}
}
