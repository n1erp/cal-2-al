namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseDeconstructionAssignmentExpression : AssignmentExpression, IDeconstructionAssignmentOperation, IAssignmentOperation, IOperation
{
	public BaseDeconstructionAssignmentExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.DeconstructionAssignment, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitDeconstructionAssignment(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitDeconstructionAssignment(this, argument);
	}
}
