namespace Microsoft.CodeAnalysis.Operations;

internal sealed class IsPatternExpression : BaseIsPatternExpression, IIsPatternOperation, IOperation
{
	public override IOperation Value { get; }

	public override IPatternOperation Pattern { get; }

	public IsPatternExpression(IOperation value, IPatternOperation pattern, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
		Pattern = Operation.SetParentOperation(pattern, this);
	}
}
