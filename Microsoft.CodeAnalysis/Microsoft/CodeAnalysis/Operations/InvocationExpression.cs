using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InvocationExpression : BaseInvocationExpression, IInvocationOperation, IOperation
{
	public override IOperation Instance { get; }

	public override ImmutableArray<IArgumentOperation> Arguments { get; }

	public InvocationExpression(IMethodSymbol targetMethod, IOperation instance, bool isVirtual, ImmutableArray<IArgumentOperation> arguments, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(targetMethod, isVirtual, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Instance = Operation.SetParentOperation(instance, this);
		Arguments = Operation.SetParentOperation(arguments, this);
	}
}
