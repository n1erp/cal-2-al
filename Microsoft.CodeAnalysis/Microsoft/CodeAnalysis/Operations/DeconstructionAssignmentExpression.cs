namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DeconstructionAssignmentExpression : BaseDeconstructionAssignmentExpression, IDeconstructionAssignmentOperation, IAssignmentOperation, IOperation
{
	public override IOperation Target { get; }

	public override IOperation Value { get; }

	public DeconstructionAssignmentExpression(IOperation target, IOperation value, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Target = Operation.SetParentOperation(target, this);
		Value = Operation.SetParentOperation(value, this);
	}
}
