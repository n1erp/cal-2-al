namespace Microsoft.CodeAnalysis.Operations;

internal sealed class SimpleAssignmentExpression : BaseSimpleAssignmentExpression, ISimpleAssignmentOperation, IAssignmentOperation, IOperation
{
	public override IOperation Target { get; }

	public override IOperation Value { get; }

	public SimpleAssignmentExpression(IOperation target, bool isRef, IOperation value, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(isRef, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Target = Operation.SetParentOperation(target, this);
		Value = Operation.SetParentOperation(value, this);
	}
}
