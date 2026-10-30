namespace Microsoft.CodeAnalysis.Operations;

internal sealed class EventReferenceExpression : BaseEventReferenceExpression, IEventReferenceOperation, IMemberReferenceOperation, IOperation
{
	public override IOperation Instance { get; }

	public EventReferenceExpression(IEventSymbol @event, IOperation instance, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(@event, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Instance = Operation.SetParentOperation(instance, this);
	}
}
