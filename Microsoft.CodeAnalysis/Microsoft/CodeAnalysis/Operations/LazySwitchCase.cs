using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazySwitchCase : BaseSwitchCase, ISwitchCaseOperation, IOperation
{
	private readonly Lazy<ImmutableArray<ICaseClauseOperation>> _lazyClauses;

	private readonly Lazy<IOperation> _lazyCondition;

	private readonly Lazy<ImmutableArray<IOperation>> _lazyBody;

	public override ImmutableArray<ICaseClauseOperation> Clauses => Operation.SetParentOperation(_lazyClauses.Value, this);

	public override IOperation Condition => Operation.SetParentOperation(_lazyCondition.Value, null);

	public override ImmutableArray<IOperation> Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public LazySwitchCase(ImmutableArray<ILocalSymbol> locals, Lazy<IOperation> condition, Lazy<ImmutableArray<ICaseClauseOperation>> clauses, Lazy<ImmutableArray<IOperation>> body, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyClauses = clauses;
		_lazyCondition = condition;
		_lazyBody = body;
	}
}
