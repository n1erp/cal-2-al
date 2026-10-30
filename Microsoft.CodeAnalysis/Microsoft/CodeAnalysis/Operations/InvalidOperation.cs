using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InvalidOperation : BaseInvalidOperation, IInvalidOperation, IOperation
{
	public override IEnumerable<IOperation> Children { get; }

	public InvalidOperation(ImmutableArray<IOperation> children, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Children = Operation.SetParentOperation(children, this);
	}
}
