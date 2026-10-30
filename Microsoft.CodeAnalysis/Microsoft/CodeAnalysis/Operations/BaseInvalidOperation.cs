namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseInvalidOperation : Operation, IInvalidOperation, IOperation
{
	protected BaseInvalidOperation(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Invalid, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInvalid(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitInvalid(this, argument);
	}
}
