using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DynamicIndexerAccessExpression : BaseDynamicIndexerAccessExpression, IDynamicIndexerAccessOperation, IOperation
{
	public override IOperation Operation { get; }

	public override ImmutableArray<IOperation> Arguments { get; }

	public DynamicIndexerAccessExpression(IOperation operation, ImmutableArray<IOperation> arguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> argumentRefKinds, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(argumentNames, argumentRefKinds, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Operation = Microsoft.CodeAnalysis.Operation.SetParentOperation(operation, this);
		Arguments = Microsoft.CodeAnalysis.Operation.SetParentOperation(arguments, this);
	}
}
