using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ForLoopStatement : BaseForLoopStatement, IForLoopOperation, ILoopOperation, IOperation
{
	public override ImmutableArray<IOperation> Before { get; }

	public override IOperation Condition { get; }

	public override ImmutableArray<IOperation> AtLoopBottom { get; }

	public override IOperation Body { get; }

	public ForLoopStatement(ImmutableArray<IOperation> before, IOperation condition, ImmutableArray<IOperation> atLoopBottom, ImmutableArray<ILocalSymbol> locals, ImmutableArray<ILocalSymbol> conditionLocals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, IOperation body, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, conditionLocals, continueLabel, exitLabel, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Before = Operation.SetParentOperation(before, this);
		Condition = Operation.SetParentOperation(condition, this);
		AtLoopBottom = Operation.SetParentOperation(atLoopBottom, this);
		Body = Operation.SetParentOperation(body, this);
	}
}
