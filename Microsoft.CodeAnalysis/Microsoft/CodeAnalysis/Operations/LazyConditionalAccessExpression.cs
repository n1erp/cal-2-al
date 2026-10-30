using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyConditionalAccessExpression : BaseConditionalAccessExpression, IConditionalAccessOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyWhenNotNull;

	private readonly Lazy<IOperation> _lazyOperation;

	public override IOperation Operation => Microsoft.CodeAnalysis.Operation.SetParentOperation(_lazyOperation.Value, this);

	public override IOperation WhenNotNull => Microsoft.CodeAnalysis.Operation.SetParentOperation(_lazyWhenNotNull.Value, this);

	public LazyConditionalAccessExpression(Lazy<IOperation> whenNotNull, Lazy<IOperation> operation, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyWhenNotNull = whenNotNull ?? throw new ArgumentNullException("whenNotNull");
		_lazyOperation = operation ?? throw new ArgumentNullException("operation");
	}
}
