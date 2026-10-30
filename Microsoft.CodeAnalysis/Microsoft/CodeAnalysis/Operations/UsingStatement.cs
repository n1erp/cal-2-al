using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class UsingStatement : BaseUsingStatement, IUsingOperation, IOperation
{
	public override IOperation Resources { get; }

	public override IOperation Body { get; }

	public UsingStatement(IOperation resources, IOperation body, ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Resources = Operation.SetParentOperation(resources, this);
		Body = Operation.SetParentOperation(body, this);
	}
}
