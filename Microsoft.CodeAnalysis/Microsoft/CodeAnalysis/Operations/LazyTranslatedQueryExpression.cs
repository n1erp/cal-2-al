using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyTranslatedQueryExpression : BaseTranslatedQueryExpression, ITranslatedQueryOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyOperation;

	public override IOperation Operation => Microsoft.CodeAnalysis.Operation.SetParentOperation(_lazyOperation.Value, this);

	public LazyTranslatedQueryExpression(Lazy<IOperation> operation, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyOperation = operation ?? throw new ArgumentNullException("operation");
	}
}
