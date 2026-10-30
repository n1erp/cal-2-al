using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class MethodBodySynthesizer
{
	internal static BoundBlock ConstructSingleInvocationMethodBody(SyntheticBoundNodeFactory F, MethodSymbol methodToInvoke, bool useBaseReference)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (ParameterSymbol parameter in F.CurrentFunction.Parameters)
		{
			instance.Add(F.Parameter(parameter));
		}
		BoundExpression boundExpression = F.Call(useBaseReference ? ((BoundExpression)F.Base()) : ((BoundExpression)F.This()), methodToInvoke, instance.ToImmutableAndFree());
		if (!F.CurrentFunction.ReturnsVoid)
		{
			return F.Block(F.Return(boundExpression));
		}
		return F.Block(F.ExpressionStatement(boundExpression), F.Return());
	}
}
