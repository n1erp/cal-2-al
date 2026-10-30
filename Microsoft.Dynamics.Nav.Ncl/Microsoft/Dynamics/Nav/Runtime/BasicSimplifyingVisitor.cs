namespace Microsoft.Dynamics.Nav.Runtime;

internal class BasicSimplifyingVisitor : ModifyingExpressionVisitor<object>
{
	public static readonly BasicSimplifyingVisitor Instance = new BasicSimplifyingVisitor();

	private BasicSimplifyingVisitor()
	{
	}

	protected override FilterExpression VisitRange(RangeFilterExpression rangeFilterExpression, object input)
	{
		if (rangeFilterExpression.ExpressionType == FilterExpressionType.RangeBetweenInclusive && rangeFilterExpression.ExpressionContext.Compare(rangeFilterExpression.LowValue, rangeFilterExpression.HighValue) == 0)
		{
			return FilterExpression.Equal(rangeFilterExpression.HighValue, rangeFilterExpression.ExpressionContext);
		}
		return rangeFilterExpression;
	}

	protected override FilterExpression VisitBinary(BinaryFilterExpression binaryExpression, object input)
	{
		FilterExpression filterExpression = base.VisitBinary(binaryExpression, input);
		switch (filterExpression.ExpressionType)
		{
		case FilterExpressionType.And:
		{
			BinaryFilterExpression binaryFilterExpression2 = (BinaryFilterExpression)filterExpression;
			return TryTurnAndIntoRangeOrSingleValue(binaryFilterExpression2.Left, binaryFilterExpression2.Right) ?? TryTurnAndIntoRangeOrSingleValue(binaryFilterExpression2.Right, binaryFilterExpression2.Left) ?? binaryFilterExpression2;
		}
		case FilterExpressionType.Or:
		{
			BinaryFilterExpression binaryFilterExpression = (BinaryFilterExpression)filterExpression;
			return TryTurnOrIntoNotEqual(binaryFilterExpression.Left, binaryFilterExpression.Right) ?? TryTurnOrIntoNotEqual(binaryFilterExpression.Right, binaryFilterExpression.Left) ?? binaryFilterExpression;
		}
		default:
			return filterExpression;
		}
	}

	private static FilterExpression TryTurnAndIntoRangeOrSingleValue(FilterExpression left, FilterExpression right)
	{
		if (left.ExpressionType == FilterExpressionType.GreaterThanOrEqual && right.ExpressionType == FilterExpressionType.LessThanOrEqual)
		{
			UnaryFilterExpression unaryFilterExpression = (UnaryFilterExpression)left;
			UnaryFilterExpression unaryFilterExpression2 = (UnaryFilterExpression)right;
			if (!unaryFilterExpression.ExpressionContext.Equals(unaryFilterExpression2.ExpressionContext))
			{
				return null;
			}
			NavValue value = unaryFilterExpression.Value;
			NavValue value2 = unaryFilterExpression2.Value;
			int num = unaryFilterExpression.ExpressionContext.Compare(value, value2);
			if (num < 0)
			{
				return FilterExpression.RangeBetweenInclusive(value, value2, unaryFilterExpression.ExpressionContext);
			}
			if (num == 0)
			{
				return FilterExpression.Equal(value, unaryFilterExpression.ExpressionContext);
			}
		}
		return null;
	}

	private static FilterExpression TryTurnOrIntoNotEqual(FilterExpression left, FilterExpression right)
	{
		if (left.ExpressionType == FilterExpressionType.LessThan && right.ExpressionType == FilterExpressionType.GreaterThan)
		{
			UnaryFilterExpression unaryFilterExpression = (UnaryFilterExpression)left;
			UnaryFilterExpression unaryFilterExpression2 = (UnaryFilterExpression)right;
			if (!unaryFilterExpression.ExpressionContext.Equals(unaryFilterExpression2.ExpressionContext))
			{
				return null;
			}
			NavValue value = unaryFilterExpression.Value;
			NavValue value2 = unaryFilterExpression2.Value;
			if (unaryFilterExpression.ExpressionContext.Compare(value, value2) == 0)
			{
				return FilterExpression.NotEqual(value, unaryFilterExpression.ExpressionContext);
			}
		}
		return null;
	}
}
