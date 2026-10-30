using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NegatingExpressionVisitor : FilterExpressionVisitor<object, FilterExpression>
{
	public static readonly NegatingExpressionVisitor Instance = new NegatingExpressionVisitor();

	private NegatingExpressionVisitor()
	{
	}

	protected override FilterExpression VisitBinary(BinaryFilterExpression binaryExpression, object input)
	{
		return binaryExpression.ExpressionType switch
		{
			FilterExpressionType.And => FilterExpression.Or(Visit(binaryExpression.Left, input), Visit(binaryExpression.Right, input)), 
			FilterExpressionType.Or => FilterExpression.And(Visit(binaryExpression.Left, input), Visit(binaryExpression.Right, input)), 
			_ => throw new NotSupportedException(), 
		};
	}

	protected override FilterExpression VisitUnary(UnaryFilterExpression unaryExpression, object input)
	{
		return unaryExpression.ExpressionType switch
		{
			FilterExpressionType.Equal => (FilterExpression)FilterExpression.NotEqual(unaryExpression.Value, unaryExpression.ExpressionContext), 
			FilterExpressionType.NotEqual => FilterExpression.Equal(unaryExpression.Value, unaryExpression.ExpressionContext), 
			FilterExpressionType.GreaterThan => FilterExpression.LessThanOrEqual(unaryExpression.Value, unaryExpression.ExpressionContext), 
			FilterExpressionType.LessThan => FilterExpression.GreaterThanOrEqual(unaryExpression.Value, unaryExpression.ExpressionContext), 
			FilterExpressionType.GreaterThanOrEqual => FilterExpression.LessThan(unaryExpression.Value, unaryExpression.ExpressionContext), 
			FilterExpressionType.LessThanOrEqual => FilterExpression.GreaterThan(unaryExpression.Value, unaryExpression.ExpressionContext), 
			_ => throw new NotSupportedException(), 
		};
	}

	protected override FilterExpression VisitWildcard(WildcardFilterExpression wildcardExpression, object input)
	{
		return FilterExpression.Wildcard(!wildcardExpression.IsNegated, wildcardExpression.Pattern, wildcardExpression.IsCaseAndAccentInsensitive, wildcardExpression.ExpressionContext);
	}

	protected override FilterExpression VisitBooleanConstant(BooleanConstantFilterExpression booleanConstantExpression, object input)
	{
		return FilterExpression.BooleanConstant(!booleanConstantExpression.Value);
	}

	protected override FilterExpression VisitFieldEqualsField(FieldEqualsFieldFilterExpression fieldEqualsFieldExpression, object input)
	{
		throw new NotSupportedException();
	}
}
