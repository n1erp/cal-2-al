using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ObjectCreationExpression : BaseObjectCreationExpression, IObjectCreationOperation, IOperation
{
	public override IObjectOrCollectionInitializerOperation Initializer { get; }

	public override ImmutableArray<IArgumentOperation> Arguments { get; }

	public ObjectCreationExpression(IMethodSymbol constructor, IObjectOrCollectionInitializerOperation initializer, ImmutableArray<IArgumentOperation> arguments, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(constructor, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Initializer = Operation.SetParentOperation(initializer, this);
		Arguments = Operation.SetParentOperation(arguments, this);
	}
}
