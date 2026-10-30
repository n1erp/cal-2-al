using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class FixedStatement : BaseFixedStatement, IFixedOperation, IOperation
{
	public override IVariableDeclarationGroupOperation Variables { get; }

	public override IOperation Body { get; }

	public FixedStatement(ImmutableArray<ILocalSymbol> locals, IVariableDeclarationGroupOperation variables, IOperation body, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Variables = Operation.SetParentOperation(variables, this);
		Body = Operation.SetParentOperation(body, this);
	}
}
