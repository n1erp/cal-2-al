using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyNameOfExpression : BaseNameOfExpression, INameOfOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyArgument;

	public override IOperation Argument => Operation.SetParentOperation(_lazyArgument.Value, this);

	public LazyNameOfExpression(Lazy<IOperation> argument, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyArgument = argument ?? throw new ArgumentNullException("argument");
	}
}
