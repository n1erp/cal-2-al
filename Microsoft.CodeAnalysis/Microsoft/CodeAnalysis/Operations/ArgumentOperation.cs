namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ArgumentOperation : BaseArgument
{
	public override IOperation Value { get; }

	public ArgumentOperation(IOperation value, ArgumentKind argumentKind, IParameterSymbol parameter, IConvertibleConversion inConversionOpt, IConvertibleConversion outConversionOpt, SemanticModel semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(argumentKind, parameter, inConversionOpt, outConversionOpt, semanticModel, syntax, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
	}
}
