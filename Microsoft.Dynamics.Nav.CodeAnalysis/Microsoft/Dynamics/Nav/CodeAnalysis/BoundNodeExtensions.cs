using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class BoundNodeExtensions
{
	public static bool HasErrors<T>(this ImmutableArray<T> nodeArray) where T : BoundNode
	{
		if (nodeArray.IsDefault)
		{
			return false;
		}
		int i = 0;
		for (int length = nodeArray.Length; i < length; i++)
		{
			if (nodeArray[i].HasErrors)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasErrors(this BoundNode node)
	{
		return node?.HasErrors ?? false;
	}

	internal static BoundExpression SkipConversion(this BoundExpression argumentExpression)
	{
		if (argumentExpression.Kind == BoundKind.Conversion)
		{
			argumentExpression = ((BoundConversion)argumentExpression).Operand;
		}
		return argumentExpression;
	}
}
