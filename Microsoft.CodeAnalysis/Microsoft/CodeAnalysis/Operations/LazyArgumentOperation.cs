using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyArgumentOperation : BaseArgument
{
	private readonly Lazy<IOperation> _lazyValue;

	public override IOperation Value => Operation.SetParentOperation(_lazyValue.Value, this);

	public LazyArgumentOperation(Lazy<IOperation> value, ArgumentKind argumentKind, IConvertibleConversion inConversionOpt, IConvertibleConversion outConversionOpt, IParameterSymbol parameter, SemanticModel semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(argumentKind, parameter, inConversionOpt, outConversionOpt, semanticModel, syntax, isImplicit)
	{
		_lazyValue = value;
	}
}
