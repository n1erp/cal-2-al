namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ThrowExpression : BaseThrowExpression, IThrowOperation, IOperation
{
	public override IOperation Exception { get; }

	public ThrowExpression(IOperation exception, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Exception = Operation.SetParentOperation(exception, this);
	}
}
