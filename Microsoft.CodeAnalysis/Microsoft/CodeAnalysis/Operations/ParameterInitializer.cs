using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ParameterInitializer : BaseParameterInitializer, IParameterInitializerOperation, ISymbolInitializerOperation, IOperation
{
	public override IOperation Value { get; }

	public ParameterInitializer(ImmutableArray<ILocalSymbol> locals, IParameterSymbol parameter, IOperation value, OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, parameter, kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
	}
}
