using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyForLoopStatement : BaseForLoopStatement, IForLoopOperation, ILoopOperation, IOperation
{
	private readonly Lazy<ImmutableArray<IOperation>> _lazyBefore;

	private readonly Lazy<IOperation> _lazyCondition;

	private readonly Lazy<ImmutableArray<IOperation>> _lazyAtLoopBottom;

	private readonly Lazy<IOperation> _lazyBody;

	public override ImmutableArray<IOperation> Before => Operation.SetParentOperation(_lazyBefore.Value, this);

	public override IOperation Condition => Operation.SetParentOperation(_lazyCondition.Value, this);

	public override ImmutableArray<IOperation> AtLoopBottom => Operation.SetParentOperation(_lazyAtLoopBottom.Value, this);

	public override IOperation Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public LazyForLoopStatement(Lazy<ImmutableArray<IOperation>> before, Lazy<IOperation> condition, Lazy<ImmutableArray<IOperation>> atLoopBottom, ImmutableArray<ILocalSymbol> locals, ImmutableArray<ILocalSymbol> conditionLocals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, Lazy<IOperation> body, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, conditionLocals, continueLabel, exitLabel, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyBefore = before ?? throw new ArgumentNullException("before");
		_lazyCondition = condition ?? throw new ArgumentNullException("condition");
		_lazyAtLoopBottom = atLoopBottom ?? throw new ArgumentNullException("atLoopBottom");
		_lazyBody = body ?? throw new ArgumentNullException("body");
	}
}
