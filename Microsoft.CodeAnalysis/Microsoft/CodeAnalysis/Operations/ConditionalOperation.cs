namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ConditionalOperation : BaseConditionalOperation, IConditionalOperation, IOperation
{
	public override IOperation Condition { get; }

	public override IOperation WhenTrue { get; }

	public override IOperation WhenFalse { get; }

	public ConditionalOperation(IOperation condition, IOperation whenTrue, IOperation whenFalse, bool isRef, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(isRef, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Condition = Operation.SetParentOperation(condition, this);
		WhenTrue = Operation.SetParentOperation(whenTrue, this);
		WhenFalse = Operation.SetParentOperation(whenFalse, this);
	}
}
