using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DynamicInvocationExpression : BaseDynamicInvocationExpression, IDynamicInvocationOperation, IOperation
{
	public override IOperation Operation { get; }

	public override ImmutableArray<IOperation> Arguments { get; }

	public DynamicInvocationExpression(IOperation operation, ImmutableArray<IOperation> arguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> argumentRefKinds, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(argumentNames, argumentRefKinds, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Operation = Microsoft.CodeAnalysis.Operation.SetParentOperation(operation, this);
		Arguments = Microsoft.CodeAnalysis.Operation.SetParentOperation(arguments, this);
	}
}
