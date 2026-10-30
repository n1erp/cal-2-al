using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyIsPatternExpression : BaseIsPatternExpression, IIsPatternOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyValue;

	private readonly Lazy<IPatternOperation> _lazyPattern;

	public override IOperation Value => Operation.SetParentOperation(_lazyValue.Value, this);

	public override IPatternOperation Pattern => Operation.SetParentOperation(_lazyPattern.Value, this);

	public LazyIsPatternExpression(Lazy<IOperation> value, Lazy<IPatternOperation> lazyPattern, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyValue = value ?? throw new ArgumentNullException("value");
		_lazyPattern = lazyPattern ?? throw new ArgumentNullException("lazyPattern");
	}
}
