namespace Microsoft.CodeAnalysis.Operations;

internal sealed class EventAssignmentOperation : BaseEventAssignmentOperation, IEventAssignmentOperation, IOperation
{
	public override IOperation EventReference { get; }

	public override IOperation HandlerValue { get; }

	public EventAssignmentOperation(IOperation eventReference, IOperation handlerValue, bool adds, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(adds, semanticModel, syntax, type, constantValue, isImplicit)
	{
		EventReference = Operation.SetParentOperation(eventReference, this);
		HandlerValue = Operation.SetParentOperation(handlerValue, this);
	}
}
