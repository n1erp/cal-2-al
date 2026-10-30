using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class TryStatement : BaseTryStatement, ITryOperation, IOperation
{
	public override IBlockOperation Body { get; }

	public override ImmutableArray<ICatchClauseOperation> Catches { get; }

	public override IBlockOperation Finally { get; }

	public TryStatement(IBlockOperation body, ImmutableArray<ICatchClauseOperation> catches, IBlockOperation finallyHandler, ILabelSymbol exitLabel, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(exitLabel, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Body = Operation.SetParentOperation(body, this);
		Catches = Operation.SetParentOperation(catches, this);
		Finally = Operation.SetParentOperation(finallyHandler, this);
	}
}
