using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyForToLoopStatement : BaseForToLoopStatement, IForToLoopOperation, ILoopOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyLoopControlVariable;

	private readonly Lazy<IOperation> _lazyInitialValue;

	private readonly Lazy<IOperation> _lazyLimitValue;

	private readonly Lazy<IOperation> _lazyStepValue;

	private readonly Lazy<IOperation> _lazyBody;

	private readonly Lazy<ImmutableArray<IOperation>> _lazyNextVariables;

	public override IOperation LoopControlVariable => Operation.SetParentOperation(_lazyLoopControlVariable.Value, this);

	public override IOperation InitialValue => Operation.SetParentOperation(_lazyInitialValue.Value, this);

	public override IOperation LimitValue => Operation.SetParentOperation(_lazyLimitValue.Value, this);

	public override IOperation StepValue => Operation.SetParentOperation(_lazyStepValue.Value, this);

	public override IOperation Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public override ImmutableArray<IOperation> NextVariables => Operation.SetParentOperation(_lazyNextVariables.Value, this);

	public LazyForToLoopStatement(ImmutableArray<ILocalSymbol> locals, bool isChecked, (ILocalSymbol LoopObject, ForToLoopOperationUserDefinedInfo UserDefinedInfo) info, ILabelSymbol continueLabel, ILabelSymbol exitLabel, Lazy<IOperation> loopControlVariable, Lazy<IOperation> initialValue, Lazy<IOperation> limitValue, Lazy<IOperation> stepValue, Lazy<IOperation> body, Lazy<ImmutableArray<IOperation>> nextVariables, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, isChecked, info, continueLabel, exitLabel, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyLoopControlVariable = loopControlVariable ?? throw new ArgumentNullException("loopControlVariable");
		_lazyInitialValue = initialValue ?? throw new ArgumentNullException("initialValue");
		_lazyLimitValue = limitValue ?? throw new ArgumentNullException("limitValue");
		_lazyStepValue = stepValue ?? throw new ArgumentNullException("stepValue");
		_lazyBody = body ?? throw new ArgumentNullException("body");
		_lazyNextVariables = nextVariables ?? throw new ArgumentNullException("nextVariables");
	}
}
