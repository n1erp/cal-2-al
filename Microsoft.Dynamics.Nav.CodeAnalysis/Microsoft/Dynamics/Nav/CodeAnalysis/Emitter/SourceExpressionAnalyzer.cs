namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter;

internal class SourceExpressionAnalyzer : BoundTreeWalkerWithStackGuard
{
	private bool canBeReused = true;

	public static bool CanBeReused(BoundExpression expression)
	{
		if (expression.ConstantValue != null)
		{
			return false;
		}
		SourceExpressionAnalyzer sourceExpressionAnalyzer = new SourceExpressionAnalyzer();
		sourceExpressionAnalyzer.Visit(expression);
		return sourceExpressionAnalyzer.canBeReused;
	}

	public override BoundNode VisitGlobal(BoundGlobal node)
	{
		if (!node.GlobalSymbol.IsWritable)
		{
			canBeReused = false;
		}
		return null;
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		canBeReused = false;
		return null;
	}
}
