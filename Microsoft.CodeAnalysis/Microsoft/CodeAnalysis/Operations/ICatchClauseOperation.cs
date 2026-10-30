using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface ICatchClauseOperation : IOperation
{
	IBlockOperation Handler { get; }

	ImmutableArray<ILocalSymbol> Locals { get; }

	ITypeSymbol ExceptionType { get; }

	IOperation ExceptionDeclarationOrExpression { get; }

	IOperation Filter { get; }
}
