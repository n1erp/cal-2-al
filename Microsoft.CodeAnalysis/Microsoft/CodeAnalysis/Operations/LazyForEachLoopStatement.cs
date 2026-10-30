using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyForEachLoopStatement : BaseForEachLoopStatement, IForEachLoopOperation, ILoopOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyLoopControlVariable;

	private readonly Lazy<IOperation> _lazyCollection;

	private readonly Lazy<ImmutableArray<IOperation>> _lazyNextVariables;

	private readonly Lazy<IOperation> _lazyBody;

	public override IOperation LoopControlVariable => Operation.SetParentOperation(_lazyLoopControlVariable.Value, this);

	public override IOperation Collection => Operation.SetParentOperation(_lazyCollection.Value, this);

	public override ImmutableArray<IOperation> NextVariables => Operation.SetParentOperation(_lazyNextVariables.Value, this);

	public override IOperation Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public LazyForEachLoopStatement(ImmutableArray<ILocalSymbol> locals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, Lazy<IOperation> loopControlVariable, Lazy<IOperation> collection, Lazy<ImmutableArray<IOperation>> nextVariables, Lazy<IOperation> body, ForEachLoopOperationInfo info, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, continueLabel, exitLabel, info, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyLoopControlVariable = loopControlVariable ?? throw new ArgumentNullException("loopControlVariable");
		_lazyCollection = collection ?? throw new ArgumentNullException("collection");
		_lazyNextVariables = nextVariables ?? throw new ArgumentNullException("nextVariables");
		_lazyBody = body ?? throw new ArgumentNullException("body");
	}
}
