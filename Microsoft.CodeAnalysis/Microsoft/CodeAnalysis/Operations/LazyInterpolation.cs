using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyInterpolation : BaseInterpolation, IInterpolationOperation, IInterpolatedStringContentOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyExpression;

	private readonly Lazy<IOperation> _lazyAlignment;

	private readonly Lazy<IOperation> _lazyFormatString;

	public override IOperation Expression => Operation.SetParentOperation(_lazyExpression.Value, this);

	public override IOperation Alignment => Operation.SetParentOperation(_lazyAlignment.Value, this);

	public override IOperation FormatString => Operation.SetParentOperation(_lazyFormatString.Value, this);

	public LazyInterpolation(Lazy<IOperation> expression, Lazy<IOperation> alignment, Lazy<IOperation> formatString, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyExpression = expression;
		_lazyAlignment = alignment;
		_lazyFormatString = formatString;
	}
}
