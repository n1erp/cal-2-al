using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyThrowExpression : BaseThrowExpression, IThrowOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyException;

	public override IOperation Exception => Operation.SetParentOperation(_lazyException.Value, this);

	public LazyThrowExpression(Lazy<IOperation> exception, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyException = exception ?? throw new ArgumentNullException("exception");
	}
}
