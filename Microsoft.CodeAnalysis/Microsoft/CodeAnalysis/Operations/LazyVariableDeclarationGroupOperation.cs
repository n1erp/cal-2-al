using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyVariableDeclarationGroupOperation : BaseVariableDeclarationGroupOperation, IVariableDeclarationGroupOperation, IOperation
{
	private readonly Lazy<ImmutableArray<IVariableDeclarationOperation>> _lazyDeclarations;

	public override ImmutableArray<IVariableDeclarationOperation> Declarations => Operation.SetParentOperation(_lazyDeclarations.Value, this);

	public LazyVariableDeclarationGroupOperation(Lazy<ImmutableArray<IVariableDeclarationOperation>> declarations, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyDeclarations = declarations;
	}
}
