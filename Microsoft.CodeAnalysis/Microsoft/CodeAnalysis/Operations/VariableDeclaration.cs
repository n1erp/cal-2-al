using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class VariableDeclaration : BaseVariableDeclaration
{
	public override ImmutableArray<IVariableDeclaratorOperation> Declarators { get; }

	public override IVariableInitializerOperation Initializer { get; }

	public VariableDeclaration(ImmutableArray<IVariableDeclaratorOperation> declarations, IVariableInitializerOperation initializer, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Declarators = Operation.SetParentOperation(declarations, this);
		Initializer = Operation.SetParentOperation(initializer, this);
	}
}
