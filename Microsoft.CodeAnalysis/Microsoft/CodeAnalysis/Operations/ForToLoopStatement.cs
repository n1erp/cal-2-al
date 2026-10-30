using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ForToLoopStatement : BaseForToLoopStatement, IForToLoopOperation, ILoopOperation, IOperation
{
	public override IOperation LoopControlVariable { get; }

	public override IOperation InitialValue { get; }

	public override IOperation LimitValue { get; }

	public override IOperation StepValue { get; }

	public override IOperation Body { get; }

	public override ImmutableArray<IOperation> NextVariables { get; }

	public ForToLoopStatement(ImmutableArray<ILocalSymbol> locals, bool isChecked, (ILocalSymbol LoopObject, ForToLoopOperationUserDefinedInfo UserDefinedInfo) info, ILabelSymbol continueLabel, ILabelSymbol exitLabel, IOperation loopControlVariable, IOperation initialValue, IOperation limitValue, IOperation stepValue, IOperation body, ImmutableArray<IOperation> nextVariables, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, isChecked, info, continueLabel, exitLabel, semanticModel, syntax, type, constantValue, isImplicit)
	{
		LoopControlVariable = Operation.SetParentOperation(loopControlVariable, this);
		InitialValue = Operation.SetParentOperation(initialValue, this);
		LimitValue = Operation.SetParentOperation(limitValue, this);
		StepValue = Operation.SetParentOperation(stepValue, this);
		Body = Operation.SetParentOperation(body, this);
		NextVariables = Operation.SetParentOperation(nextVariables, this);
	}
}
