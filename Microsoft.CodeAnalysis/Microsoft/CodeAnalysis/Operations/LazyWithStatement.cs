using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyWithStatement : BaseWithStatement, IWithOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyBody;

	private readonly Lazy<IOperation> _lazyValue;

	public override IOperation Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public override IOperation Value => Operation.SetParentOperation(_lazyValue.Value, this);

	public LazyWithStatement(Lazy<IOperation> body, Lazy<IOperation> value, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyBody = body ?? throw new ArgumentNullException("body");
		_lazyValue = value ?? throw new ArgumentNullException("value");
	}
}
