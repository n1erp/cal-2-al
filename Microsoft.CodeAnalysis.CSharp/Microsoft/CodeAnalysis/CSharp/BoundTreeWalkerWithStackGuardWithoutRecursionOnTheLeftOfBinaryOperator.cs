using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator : BoundTreeWalkerWithStackGuard
{
	protected BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator()
	{
	}

	protected BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator(int recursionDepth)
		: base(recursionDepth)
	{
	}

	public sealed override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		if (node.Left.Kind != BoundKind.BinaryOperator)
		{
			return base.VisitBinaryOperator(node);
		}
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		instance.Push(node.Right);
		BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)node.Left;
		instance.Push(boundBinaryOperator.Right);
		BoundExpression left = boundBinaryOperator.Left;
		while (left.Kind == BoundKind.BinaryOperator)
		{
			boundBinaryOperator = (BoundBinaryOperator)left;
			instance.Push(boundBinaryOperator.Right);
			left = boundBinaryOperator.Left;
		}
		Visit(left);
		while (instance.Count > 0)
		{
			Visit(instance.Pop());
		}
		instance.Free();
		return null;
	}
}
