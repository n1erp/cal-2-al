using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class VariableDeclarator : BaseVariableDeclarator
{
	public override IVariableInitializerOperation Initializer { get; }

	public override ImmutableArray<IOperation> IgnoredArguments { get; }

	public VariableDeclarator(ILocalSymbol symbol, IVariableInitializerOperation initializer, ImmutableArray<IOperation> ignoredArguments, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(symbol, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Initializer = Operation.SetParentOperation(initializer, this);
		IgnoredArguments = Operation.SetParentOperation(ignoredArguments, this);
	}
}
