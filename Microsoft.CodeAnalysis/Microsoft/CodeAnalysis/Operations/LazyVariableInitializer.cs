using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyVariableInitializer : BaseVariableInitializer, IVariableInitializerOperation, ISymbolInitializerOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyValue;

	public override IOperation Value => Operation.SetParentOperation(_lazyValue.Value, this);

	public LazyVariableInitializer(Lazy<IOperation> value, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyValue = value ?? throw new ArgumentNullException("value");
	}
}
