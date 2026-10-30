using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundTupleBinaryOperator : BoundExpression
{
	public BoundExpression Left { get; }

	public BoundExpression Right { get; }

	public BoundExpression ConvertedLeft { get; }

	public BoundExpression ConvertedRight { get; }

	public BinaryOperatorKind OperatorKind { get; }

	public TupleBinaryOperatorInfo.Multiple Operators { get; }

	public BoundTupleBinaryOperator(SyntaxNode syntax, BoundExpression left, BoundExpression right, BoundExpression convertedLeft, BoundExpression convertedRight, BinaryOperatorKind operatorKind, TupleBinaryOperatorInfo.Multiple operators, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TupleBinaryOperator, syntax, type, hasErrors || left.HasErrors() || right.HasErrors() || convertedLeft.HasErrors() || convertedRight.HasErrors())
	{
		Left = left;
		Right = right;
		ConvertedLeft = convertedLeft;
		ConvertedRight = convertedRight;
		OperatorKind = operatorKind;
		Operators = operators;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTupleBinaryOperator(this);
	}

	public BoundTupleBinaryOperator Update(BoundExpression left, BoundExpression right, BoundExpression convertedLeft, BoundExpression convertedRight, BinaryOperatorKind operatorKind, TupleBinaryOperatorInfo.Multiple operators, TypeSymbol type)
	{
		if (left != Left || right != Right || convertedLeft != ConvertedLeft || convertedRight != ConvertedRight || operatorKind != OperatorKind || operators != Operators || type != Type)
		{
			return new BoundTupleBinaryOperator(Syntax, left, right, convertedLeft, convertedRight, operatorKind, operators, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
