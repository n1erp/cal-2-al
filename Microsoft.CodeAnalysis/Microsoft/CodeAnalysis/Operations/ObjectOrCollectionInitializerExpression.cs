using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ObjectOrCollectionInitializerExpression : BaseObjectOrCollectionInitializerExpression, IObjectOrCollectionInitializerOperation, IOperation
{
	public override ImmutableArray<IOperation> Initializers { get; }

	public ObjectOrCollectionInitializerExpression(ImmutableArray<IOperation> initializers, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Initializers = Operation.SetParentOperation(initializers, this);
	}
}
