using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class HasDynamicArgumentsExpression : Operation
{
	public ImmutableArray<string> ArgumentNames { get; }

	public ImmutableArray<RefKind> ArgumentRefKinds { get; }

	public abstract ImmutableArray<IOperation> Arguments { get; }

	protected HasDynamicArgumentsExpression(OperationKind operationKind, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> argumentRefKinds, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(operationKind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		ArgumentNames = argumentNames;
		ArgumentRefKinds = argumentRefKinds;
	}
}
