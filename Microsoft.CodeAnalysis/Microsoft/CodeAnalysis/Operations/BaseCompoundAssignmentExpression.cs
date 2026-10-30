namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseCompoundAssignmentExpression : AssignmentExpression, ICompoundAssignmentOperation, IAssignmentOperation, IOperation
{
	public BinaryOperatorKind OperatorKind { get; }

	public bool IsLifted { get; }

	public bool IsChecked { get; }

	public IMethodSymbol OperatorMethod { get; }

	internal IConvertibleConversion InConversionConvertible { get; }

	internal IConvertibleConversion OutConversionConvertible { get; }

	public CommonConversion InConversion => InConversionConvertible.ToCommonConversion();

	public CommonConversion OutConversion => OutConversionConvertible.ToCommonConversion();

	protected BaseCompoundAssignmentExpression(IConvertibleConversion inConversionConvertible, IConvertibleConversion outConversionConvertible, BinaryOperatorKind operatorKind, bool isLifted, bool isChecked, IMethodSymbol operatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.CompoundAssignment, semanticModel, syntax, type, constantValue, isImplicit)
	{
		OperatorKind = operatorKind;
		IsLifted = isLifted;
		IsChecked = isChecked;
		OperatorMethod = operatorMethod;
		InConversionConvertible = inConversionConvertible;
		OutConversionConvertible = outConversionConvertible;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitCompoundAssignment(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitCompoundAssignment(this, argument);
	}
}
