using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseMethodBodyBaseOperation : Operation, IMethodBodyBaseOperation, IOperation
{
	public abstract IBlockOperation BlockBody { get; }

	public abstract IBlockOperation ExpressionBody { get; }

	public abstract override IEnumerable<IOperation> Children { get; }

	protected BaseMethodBodyBaseOperation(OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax)
		: base(kind, semanticModel, syntax, null, default, isImplicit: false)
	{
	}
}
