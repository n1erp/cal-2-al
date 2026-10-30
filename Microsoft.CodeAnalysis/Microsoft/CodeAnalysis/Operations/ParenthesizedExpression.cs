namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ParenthesizedExpression : BaseParenthesizedExpression, IParenthesizedOperation, IOperation
{
	public override IOperation Operand { get; }

	public ParenthesizedExpression(IOperation operand, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Operand = Operation.SetParentOperation(operand, this);
	}
}
