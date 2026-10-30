using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyEventReferenceExpression : BaseEventReferenceExpression, IEventReferenceOperation, IMemberReferenceOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyInstance;

	public override IOperation Instance => Operation.SetParentOperation(_lazyInstance.Value, this);

	public LazyEventReferenceExpression(IEventSymbol @event, Lazy<IOperation> instance, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(@event, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyInstance = instance ?? throw new ArgumentNullException("instance");
	}
}
