namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseSimpleAssignmentExpression : AssignmentExpression, ISimpleAssignmentOperation, IAssignmentOperation, IOperation
{
	public bool IsRef { get; }

	public BaseSimpleAssignmentExpression(bool isRef, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.SimpleAssignment, semanticModel, syntax, type, constantValue, isImplicit)
	{
		IsRef = isRef;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitSimpleAssignment(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitSimpleAssignment(this, argument);
	}
}
