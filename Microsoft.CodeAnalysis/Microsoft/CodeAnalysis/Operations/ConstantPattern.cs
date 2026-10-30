namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ConstantPattern : BaseConstantPattern, IConstantPatternOperation, IPatternOperation, IOperation
{
	public override IOperation Value { get; }

	public ConstantPattern(IOperation value, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
	}
}
