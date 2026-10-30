using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyWhileLoopStatement : BaseWhileLoopStatement, IWhileLoopOperation, ILoopOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyCondition;

	private readonly Lazy<IOperation> _lazyBody;

	private readonly Lazy<IOperation> _lazyIgnoredCondition;

	public override IOperation Condition => Operation.SetParentOperation(_lazyCondition.Value, this);

	public override IOperation Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public override IOperation IgnoredCondition => Operation.SetParentOperation(_lazyIgnoredCondition.Value, this);

	public LazyWhileLoopStatement(Lazy<IOperation> condition, Lazy<IOperation> body, Lazy<IOperation> ignoredCondition, ImmutableArray<ILocalSymbol> locals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, bool conditionIsTop, bool conditionIsUntil, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, continueLabel, exitLabel, conditionIsTop, conditionIsUntil, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyCondition = condition ?? throw new ArgumentNullException("condition");
		_lazyBody = body ?? throw new ArgumentNullException("body");
		_lazyIgnoredCondition = ignoredCondition ?? throw new ArgumentNullException("ignoredCondition");
	}
}
