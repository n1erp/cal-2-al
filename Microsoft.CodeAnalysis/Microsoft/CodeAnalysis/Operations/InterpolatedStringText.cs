namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InterpolatedStringText : BaseInterpolatedStringText, IInterpolatedStringTextOperation, IInterpolatedStringContentOperation, IOperation
{
	public override IOperation Text { get; }

	public InterpolatedStringText(IOperation text, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Text = Operation.SetParentOperation(text, this);
	}
}
