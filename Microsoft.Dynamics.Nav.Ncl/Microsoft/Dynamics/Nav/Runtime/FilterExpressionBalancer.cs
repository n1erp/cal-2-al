using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class FilterExpressionBalancer
{
	public static FilterExpression Balance(this FilterExpression expression)
	{
		Func<FilterExpression, FilterExpression, FilterExpression> combiningFunc;
		switch (expression.ExpressionType)
		{
		case FilterExpressionType.Or:
			combiningFunc = FilterExpression.Or;
			break;
		case FilterExpressionType.And:
			combiningFunc = FilterExpression.And;
			break;
		default:
			return expression;
		}
		return FilterExpression.Balanced(SplitByBinaryType(expression, expression.ExpressionType).Select(Balance), combiningFunc);
	}

	private static IEnumerable<FilterExpression> SplitByBinaryType(FilterExpression current, FilterExpressionType binaryType)
	{
		Stack<BinaryFilterExpression> stack = new Stack<BinaryFilterExpression>();
		while (current != null || stack.Count > 0)
		{
			if (current != null)
			{
				if (current.ExpressionType == binaryType)
				{
					BinaryFilterExpression binaryFilterExpression = (BinaryFilterExpression)current;
					stack.Push(binaryFilterExpression);
					current = binaryFilterExpression.Left;
				}
				else
				{
					yield return current;
					current = null;
				}
			}
			else
			{
				current = stack.Pop().Right;
			}
		}
	}
}
