using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyInterpolatedStringText : BaseInterpolatedStringText, IInterpolatedStringTextOperation, IInterpolatedStringContentOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyText;

	public override IOperation Text => Operation.SetParentOperation(_lazyText.Value, this);

	public LazyInterpolatedStringText(Lazy<IOperation> text, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyText = text;
	}
}
