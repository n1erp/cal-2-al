namespace Microsoft.CodeAnalysis.Operations;

internal sealed class NameOfExpression : BaseNameOfExpression, INameOfOperation, IOperation
{
	public override IOperation Argument { get; }

	public NameOfExpression(IOperation argument, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Argument = Operation.SetParentOperation(argument, this);
	}
}
