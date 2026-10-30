using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyAddressOfExpression : BaseAddressOfExpression, IAddressOfOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyReference;

	public override IOperation Reference => Operation.SetParentOperation(_lazyReference.Value, this);

	public LazyAddressOfExpression(Lazy<IOperation> reference, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyReference = reference ?? throw new ArgumentNullException("reference");
	}
}
