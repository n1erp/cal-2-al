namespace Microsoft.CodeAnalysis.Operations;

internal sealed class IncrementExpression : BaseIncrementExpression, IIncrementOrDecrementOperation, IOperation
{
	public override IOperation Target { get; }

	public IncrementExpression(bool isDecrement, bool isPostfix, bool isLifted, bool isChecked, IOperation target, IMethodSymbol operatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(isDecrement, isPostfix, isLifted, isChecked, operatorMethod, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Target = Operation.SetParentOperation(target, this);
	}
}
