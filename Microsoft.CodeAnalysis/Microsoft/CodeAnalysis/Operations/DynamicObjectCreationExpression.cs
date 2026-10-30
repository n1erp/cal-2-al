using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DynamicObjectCreationExpression : BaseDynamicObjectCreationExpression, IDynamicObjectCreationOperation, IOperation
{
	public override ImmutableArray<IOperation> Arguments { get; }

	public override IObjectOrCollectionInitializerOperation Initializer { get; }

	public DynamicObjectCreationExpression(ImmutableArray<IOperation> arguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> argumentRefKinds, IObjectOrCollectionInitializerOperation initializer, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(argumentNames, argumentRefKinds, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Arguments = Operation.SetParentOperation(arguments, this);
		Initializer = Operation.SetParentOperation(initializer, this);
	}
}
