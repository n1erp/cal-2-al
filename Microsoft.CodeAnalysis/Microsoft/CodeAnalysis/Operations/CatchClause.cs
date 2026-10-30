using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class CatchClause : BaseCatchClause, ICatchClauseOperation, IOperation
{
	public override IBlockOperation Handler { get; }

	public override IOperation Filter { get; }

	public override IOperation ExceptionDeclarationOrExpression { get; }

	public CatchClause(IOperation exceptionDeclarationOrExpression, ITypeSymbol exceptionType, ImmutableArray<ILocalSymbol> locals, IOperation filter, IBlockOperation handler, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(exceptionType, locals, semanticModel, syntax, type, constantValue, isImplicit)
	{
		ExceptionDeclarationOrExpression = Operation.SetParentOperation(exceptionDeclarationOrExpression, this);
		Filter = Operation.SetParentOperation(filter, this);
		Handler = Operation.SetParentOperation(handler, this);
	}
}
