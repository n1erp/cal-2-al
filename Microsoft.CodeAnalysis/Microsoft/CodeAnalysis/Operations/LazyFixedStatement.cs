using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyFixedStatement : BaseFixedStatement, IFixedOperation, IOperation
{
	private readonly Lazy<IVariableDeclarationGroupOperation> _lazyVariables;

	private readonly Lazy<IOperation> _lazyBody;

	public override IVariableDeclarationGroupOperation Variables => Operation.SetParentOperation(_lazyVariables.Value, this);

	public override IOperation Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public LazyFixedStatement(ImmutableArray<ILocalSymbol> locals, Lazy<IVariableDeclarationGroupOperation> variables, Lazy<IOperation> body, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyVariables = variables ?? throw new ArgumentNullException("variables");
		_lazyBody = body ?? throw new ArgumentNullException("body");
	}
}
