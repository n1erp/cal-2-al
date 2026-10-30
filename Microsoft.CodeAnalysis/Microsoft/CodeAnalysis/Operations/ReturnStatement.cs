namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ReturnStatement : BaseReturnStatement, IReturnOperation, IOperation
{
	public override IOperation ReturnedValue { get; }

	public ReturnStatement(OperationKind kind, IOperation returnedValue, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		ReturnedValue = Operation.SetParentOperation(returnedValue, this);
	}
}
