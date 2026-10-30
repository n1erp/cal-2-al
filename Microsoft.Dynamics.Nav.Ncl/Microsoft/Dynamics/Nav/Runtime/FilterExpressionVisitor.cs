using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class FilterExpressionVisitor<TInput, TReturn>
{
	public TReturn Visit(FilterExpression expression, TInput input)
	{
		switch (expression.ExpressionType)
		{
		case FilterExpressionType.Equal:
		case FilterExpressionType.NotEqual:
		case FilterExpressionType.GreaterThan:
		case FilterExpressionType.LessThan:
		case FilterExpressionType.GreaterThanOrEqual:
		case FilterExpressionType.LessThanOrEqual:
			return VisitUnary((UnaryFilterExpression)expression, input);
		case FilterExpressionType.And:
		case FilterExpressionType.Or:
			return VisitBinary((BinaryFilterExpression)expression, input);
		case FilterExpressionType.RangeBetweenInclusive:
		case FilterExpressionType.RangeFromInclusive:
		case FilterExpressionType.RangeToInclusive:
			return VisitRange((RangeFilterExpression)expression, input);
		case FilterExpressionType.Wildcard:
			return VisitWildcard((WildcardFilterExpression)expression, input);
		case FilterExpressionType.BooleanConstant:
			return VisitBooleanConstant((BooleanConstantFilterExpression)expression, input);
		case FilterExpressionType.FieldEqualsField:
			return VisitFieldEqualsField((FieldEqualsFieldFilterExpression)expression, input);
		default:
			throw new NotSupportedException();
		}
	}

	protected abstract TReturn VisitBinary(BinaryFilterExpression binaryExpression, TInput input);

	protected abstract TReturn VisitUnary(UnaryFilterExpression unaryExpression, TInput input);

	protected virtual TReturn VisitRange(RangeFilterExpression rangeFilterExpression, TInput input)
	{
		switch (rangeFilterExpression.ExpressionType)
		{
		case FilterExpressionType.RangeBetweenInclusive:
		{
			UnaryFilterExpression left = new UnaryFilterExpression(FilterExpressionType.GreaterThanOrEqual, rangeFilterExpression.LowValue, rangeFilterExpression.ExpressionContext, rangeFilterExpression.LowValueToken);
			UnaryFilterExpression right = new UnaryFilterExpression(FilterExpressionType.LessThanOrEqual, rangeFilterExpression.HighValue, rangeFilterExpression.ExpressionContext, rangeFilterExpression.HighValueToken);
			return Visit(FilterExpression.And(left, right), input);
		}
		case FilterExpressionType.RangeFromInclusive:
			return Visit(new UnaryFilterExpression(FilterExpressionType.GreaterThanOrEqual, rangeFilterExpression.LowValue, rangeFilterExpression.ExpressionContext, rangeFilterExpression.LowValueToken), input);
		case FilterExpressionType.RangeToInclusive:
			return Visit(new UnaryFilterExpression(FilterExpressionType.LessThanOrEqual, rangeFilterExpression.HighValue, rangeFilterExpression.ExpressionContext, rangeFilterExpression.HighValueToken), input);
		default:
			throw new NotSupportedException();
		}
	}

	protected abstract TReturn VisitWildcard(WildcardFilterExpression wildcardExpression, TInput input);

	protected abstract TReturn VisitBooleanConstant(BooleanConstantFilterExpression booleanConstantExpression, TInput input);

	protected abstract TReturn VisitFieldEqualsField(FieldEqualsFieldFilterExpression fieldEqualsFieldExpression, TInput input);
}
