using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class SwitchStatement : BaseSwitchStatement, ISwitchOperation, IOperation
{
	public override IOperation Value { get; }

	public override ImmutableArray<ISwitchCaseOperation> Cases { get; }

	public SwitchStatement(ImmutableArray<ILocalSymbol> locals, IOperation value, ImmutableArray<ISwitchCaseOperation> cases, ILabelSymbol exitLabel, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, exitLabel, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
		Cases = Operation.SetParentOperation(cases, this);
	}
}
