using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyEventAssignmentOperation : BaseEventAssignmentOperation, IEventAssignmentOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyEventReference;

	private readonly Lazy<IOperation> _lazyHandlerValue;

	public override IOperation EventReference => Operation.SetParentOperation(_lazyEventReference.Value, this);

	public override IOperation HandlerValue => Operation.SetParentOperation(_lazyHandlerValue.Value, this);

	public LazyEventAssignmentOperation(Lazy<IOperation> eventReference, Lazy<IOperation> handlerValue, bool adds, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(adds, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyEventReference = eventReference ?? throw new ArgumentNullException("eventReference");
		_lazyHandlerValue = handlerValue ?? throw new ArgumentNullException("handlerValue");
	}
}
