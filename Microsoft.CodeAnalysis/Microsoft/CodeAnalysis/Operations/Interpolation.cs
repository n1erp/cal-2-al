namespace Microsoft.CodeAnalysis.Operations;

internal sealed class Interpolation : BaseInterpolation, IInterpolationOperation, IInterpolatedStringContentOperation, IOperation
{
	public override IOperation Expression { get; }

	public override IOperation Alignment { get; }

	public override IOperation FormatString { get; }

	public Interpolation(IOperation expression, IOperation alignment, IOperation formatString, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Expression = Operation.SetParentOperation(expression, this);
		Alignment = Operation.SetParentOperation(alignment, this);
		FormatString = Operation.SetParentOperation(formatString, this);
	}
}
