using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class VariableDeclarationGroupOperation : BaseVariableDeclarationGroupOperation, IVariableDeclarationGroupOperation, IOperation
{
	public override ImmutableArray<IVariableDeclarationOperation> Declarations { get; }

	public VariableDeclarationGroupOperation(ImmutableArray<IVariableDeclarationOperation> declarations, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Declarations = Operation.SetParentOperation(declarations, this);
	}
}
