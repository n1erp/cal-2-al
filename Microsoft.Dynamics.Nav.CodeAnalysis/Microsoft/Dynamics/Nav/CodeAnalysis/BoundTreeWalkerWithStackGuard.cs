namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal abstract class BoundTreeWalkerWithStackGuard : BoundTreeWalker
{
	private int recursionDepth;

	protected int RecursionDepth => recursionDepth;

	protected BoundTreeWalkerWithStackGuard()
	{
	}

	protected BoundTreeWalkerWithStackGuard(int recursionDepth)
	{
		this.recursionDepth = recursionDepth;
	}

	public override BoundNode Visit(BoundNode node)
	{
		if (node is BoundExpression node2)
		{
			return VisitExpressionWithStackGuard(ref recursionDepth, node2);
		}
		return base.Visit(node);
	}

	protected BoundExpression VisitExpressionWithStackGuard(BoundExpression node)
	{
		return VisitExpressionWithStackGuard(ref recursionDepth, node);
	}

	protected sealed override BoundExpression VisitExpressionWithoutStackGuard(BoundExpression node)
	{
		return (BoundExpression)base.Visit(node);
	}
}
