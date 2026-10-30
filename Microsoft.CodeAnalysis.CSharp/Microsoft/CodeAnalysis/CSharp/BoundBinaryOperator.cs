using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundBinaryOperator : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override ConstantValue ConstantValue => ConstantValueOpt;

	public override Symbol ExpressionSymbol => MethodOpt;

	public ImmutableArray<MethodSymbol> OriginalUserDefinedOperatorsOpt { get; }

	public BinaryOperatorKind OperatorKind { get; }

	public BoundExpression Left { get; }

	public BoundExpression Right { get; }

	public ConstantValue ConstantValueOpt { get; }

	public MethodSymbol MethodOpt { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	public BoundBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, ConstantValue constantValueOpt, MethodSymbol methodOpt, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type, bool hasErrors = false)
		: this(syntax, operatorKind, left, right, constantValueOpt, methodOpt, resultKind, type, hasErrors)
	{
		OriginalUserDefinedOperatorsOpt = originalUserDefinedOperatorsOpt;
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
}
