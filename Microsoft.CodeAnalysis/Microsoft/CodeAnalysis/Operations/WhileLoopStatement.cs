using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class WhileLoopStatement : BaseWhileLoopStatement, IWhileLoopOperation, ILoopOperation, IOperation
{
	public override IOperation Condition { get; }

	public override IOperation Body { get; }

	public override IOperation IgnoredCondition { get; }

	public WhileLoopStatement(IOperation condition, IOperation body, IOperation ignoredCondition, ImmutableArray<ILocalSymbol> locals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, bool conditionIsTop, bool conditionIsUntil, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, continueLabel, exitLabel, conditionIsTop, conditionIsUntil, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Condition = Operation.SetParentOperation(condition, this);
		Body = Operation.SetParentOperation(body, this);
		IgnoredCondition = Operation.SetParentOperation(ignoredCondition, this);
	}
}
