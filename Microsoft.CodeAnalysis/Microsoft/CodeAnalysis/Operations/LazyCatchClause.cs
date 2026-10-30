using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyCatchClause : BaseCatchClause, ICatchClauseOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyExceptionDeclarationOrExpression;

	private readonly Lazy<IOperation> _lazyFilter;

	private readonly Lazy<IBlockOperation> _lazyHandler;

	public override IOperation ExceptionDeclarationOrExpression => Operation.SetParentOperation(_lazyExceptionDeclarationOrExpression.Value, this);

	public override IOperation Filter => Operation.SetParentOperation(_lazyFilter.Value, this);

	public override IBlockOperation Handler => Operation.SetParentOperation(_lazyHandler.Value, this);

	public LazyCatchClause(Lazy<IOperation> exceptionDeclarationOrExpression, ITypeSymbol exceptionType, ImmutableArray<ILocalSymbol> locals, Lazy<IOperation> filter, Lazy<IBlockOperation> handler, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(exceptionType, locals, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyExceptionDeclarationOrExpression = exceptionDeclarationOrExpression ?? throw new ArgumentNullException("exceptionDeclarationOrExpression");
		_lazyFilter = filter ?? throw new ArgumentNullException("filter");
		_lazyHandler = handler ?? throw new ArgumentNullException("handler");
	}
}
