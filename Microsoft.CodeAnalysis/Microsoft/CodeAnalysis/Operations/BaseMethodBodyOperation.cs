using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseMethodBodyOperation : BaseMethodBodyBaseOperation, IMethodBodyOperation, IMethodBodyBaseOperation, IOperation
{
	public sealed override IEnumerable<IOperation> Children
	{
		get
		{
			IBlockOperation blockBody = BlockBody;
			if (blockBody != null)
			{
				yield return blockBody;
			}
			IBlockOperation expressionBody = ExpressionBody;
			if (expressionBody != null)
			{
				yield return expressionBody;
			}
		}
	}

	protected BaseMethodBodyOperation(SemanticModel semanticModel, SyntaxNode syntax)
		: base(OperationKind.MethodBodyOperation, semanticModel, syntax)
	{
	}

	public sealed override void Accept(OperationVisitor visitor)
	{
		visitor.VisitMethodBodyOperation(this);
	}

	public sealed override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitMethodBodyOperation(this, argument);
	}
}
