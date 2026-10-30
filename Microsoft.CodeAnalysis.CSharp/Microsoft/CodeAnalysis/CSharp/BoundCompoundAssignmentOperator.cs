using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundCompoundAssignmentOperator : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => Operator.Method;

	public ImmutableArray<MethodSymbol> OriginalUserDefinedOperatorsOpt { get; }

	public BinaryOperatorSignature Operator { get; }

	public BoundExpression Left { get; }

	public BoundExpression Right { get; }

	public Conversion LeftConversion { get; }

	public Conversion FinalConversion { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	public BoundCompoundAssignmentOperator(SyntaxNode syntax, BinaryOperatorSignature @operator, BoundExpression left, BoundExpression right, Conversion leftConversion, Conversion finalConversion, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type, bool hasErrors = false)
		: this(syntax, @operator, left, right, leftConversion, finalConversion, resultKind, type, hasErrors)
	{
		OriginalUserDefinedOperatorsOpt = originalUserDefinedOperatorsOpt;
	}

	public BoundCompoundAssignmentOperator(SyntaxNode syntax, BinaryOperatorSignature @operator, BoundExpression left, BoundExpression right, Conversion leftConversion, Conversion finalConversion, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.CompoundAssignmentOperator, syntax, type, hasErrors || left.HasErrors() || right.HasErrors())
	{
		Operator = @operator;
		Left = left;
		Right = right;
		LeftConversion = leftConversion;
		FinalConversion = finalConversion;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCompoundAssignmentOperator(this);
	}

	public BoundCompoundAssignmentOperator Update(BinaryOperatorSignature @operator, BoundExpression left, BoundExpression right, Conversion leftConversion, Conversion finalConversion, LookupResultKind resultKind, TypeSymbol type)
	{
		if (@operator != Operator || left != Left || right != Right || leftConversion != LeftConversion || finalConversion != FinalConversion || resultKind != ResultKind || type != Type)
		{
			return new BoundCompoundAssignmentOperator(Syntax, @operator, left, right, leftConversion, finalConversion, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
