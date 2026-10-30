using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyRaiseEventStatement : BaseRaiseEventStatement, IRaiseEventOperation, IOperation
{
	private readonly Lazy<IEventReferenceOperation> _lazyEventReference;

	private readonly Lazy<ImmutableArray<IArgumentOperation>> _lazyArguments;

	public override IEventReferenceOperation EventReference => Operation.SetParentOperation(_lazyEventReference.Value, this);

	public override ImmutableArray<IArgumentOperation> Arguments => Operation.SetParentOperation(_lazyArguments.Value, this);

	public LazyRaiseEventStatement(Lazy<IEventReferenceOperation> eventReference, Lazy<ImmutableArray<IArgumentOperation>> arguments, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyEventReference = eventReference;
		_lazyArguments = arguments;
	}
}
