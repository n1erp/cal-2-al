using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class RaiseEventStatement : BaseRaiseEventStatement, IRaiseEventOperation, IOperation
{
	public override IEventReferenceOperation EventReference { get; }

	public override ImmutableArray<IArgumentOperation> Arguments { get; }

	public RaiseEventStatement(IEventReferenceOperation eventReference, ImmutableArray<IArgumentOperation> arguments, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		EventReference = Operation.SetParentOperation(eventReference, this);
		Arguments = Operation.SetParentOperation(arguments, this);
	}
}
