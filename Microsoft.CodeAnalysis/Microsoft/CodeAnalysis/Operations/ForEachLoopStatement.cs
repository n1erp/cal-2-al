using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ForEachLoopStatement : BaseForEachLoopStatement, IForEachLoopOperation, ILoopOperation, IOperation
{
	public override IOperation LoopControlVariable { get; }

	public override IOperation Collection { get; }

	public override ImmutableArray<IOperation> NextVariables { get; }

	public override IOperation Body { get; }

	public ForEachLoopStatement(ImmutableArray<ILocalSymbol> locals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, IOperation loopControlVariable, IOperation collection, ImmutableArray<IOperation> nextVariables, IOperation body, ForEachLoopOperationInfo info, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, continueLabel, exitLabel, info, semanticModel, syntax, type, constantValue, isImplicit)
	{
		LoopControlVariable = Operation.SetParentOperation(loopControlVariable, this);
		Collection = Operation.SetParentOperation(collection, this);
		NextVariables = Operation.SetParentOperation(nextVariables, this);
		Body = Operation.SetParentOperation(body, this);
	}
}
