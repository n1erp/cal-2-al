using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class SwitchCase : BaseSwitchCase, ISwitchCaseOperation, IOperation
{
	public override ImmutableArray<ICaseClauseOperation> Clauses { get; }

	public override IOperation Condition { get; }

	public override ImmutableArray<IOperation> Body { get; }

	public SwitchCase(ImmutableArray<ILocalSymbol> locals, IOperation condition, ImmutableArray<ICaseClauseOperation> clauses, ImmutableArray<IOperation> body, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Clauses = Operation.SetParentOperation(clauses, this);
		Condition = Operation.SetParentOperation(condition, null);
		Body = Operation.SetParentOperation(body, this);
	}
}
