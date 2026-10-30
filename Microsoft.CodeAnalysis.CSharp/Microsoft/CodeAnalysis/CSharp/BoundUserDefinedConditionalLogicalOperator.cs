using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundUserDefinedConditionalLogicalOperator : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => LogicalOperator;

	public ImmutableArray<MethodSymbol> OriginalUserDefinedOperatorsOpt { get; }

	public BinaryOperatorKind OperatorKind { get; }

	public BoundExpression Left { get; }

	public BoundExpression Right { get; }

	public MethodSymbol LogicalOperator { get; }

	public MethodSymbol TrueOperator { get; }

	public MethodSymbol FalseOperator { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	public BoundUserDefinedConditionalLogicalOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, MethodSymbol logicalOperator, MethodSymbol trueOperator, MethodSymbol falseOperator, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type, bool hasErrors = false)
		: this(syntax, operatorKind, left, right, logicalOperator, trueOperator, falseOperator, resultKind, type, hasErrors)
	{
		OriginalUserDefinedOperatorsOpt = originalUserDefinedOperatorsOpt;
	}

	public BoundUserDefinedConditionalLogicalOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, MethodSymbol logicalOperator, MethodSymbol trueOperator, MethodSymbol falseOperator, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.UserDefinedConditionalLogicalOperator, syntax, type, hasErrors || left.HasErrors() || right.HasErrors())
	{
		OperatorKind = operatorKind;
		Left = left;
		Right = right;
		LogicalOperator = logicalOperator;
		TrueOperator = trueOperator;
		FalseOperator = falseOperator;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUserDefinedConditionalLogicalOperator(this);
	}

	public BoundUserDefinedConditionalLogicalOperator Update(BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, MethodSymbol logicalOperator, MethodSymbol trueOperator, MethodSymbol falseOperator, LookupResultKind resultKind, TypeSymbol type)
	{
		if (operatorKind != OperatorKind || left != Left || right != Right || logicalOperator != LogicalOperator || trueOperator != TrueOperator || falseOperator != FalseOperator || resultKind != ResultKind || type != Type)
		{
			return new BoundUserDefinedConditionalLogicalOperator(Syntax, operatorKind, left, right, logicalOperator, trueOperator, falseOperator, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
