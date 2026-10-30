namespace Microsoft.CodeAnalysis.Operations;

internal sealed class WithStatement : BaseWithStatement, IWithOperation, IOperation
{
	public override IOperation Body { get; }

	public override IOperation Value { get; }

	public WithStatement(IOperation body, IOperation value, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Body = Operation.SetParentOperation(body, this);
		Value = Operation.SetParentOperation(value, this);
	}
}
