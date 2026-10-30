namespace Microsoft.Dynamics.Nav.Runtime;

public class ModifyingExpressionVisitor<TInput> : FilterExpressionVisitor<TInput, FilterExpression>
{
	protected override FilterExpression VisitBinary(BinaryFilterExpression binaryExpression, TInput input)
	{
		return binaryExpression.Update(binaryExpression.ExpressionType, Visit(binaryExpression.Left, input), Visit(binaryExpression.Right, input));
	}

	protected override FilterExpression VisitUnary(UnaryFilterExpression unaryExpression, TInput input)
	{
		return unaryExpression;
	}

	protected override FilterExpression VisitWildcard(WildcardFilterExpression wildcardExpression, TInput input)
	{
		return wildcardExpression;
	}

	protected override FilterExpression VisitBooleanConstant(BooleanConstantFilterExpression booleanConstantExpression, TInput input)
	{
		return booleanConstantExpression;
	}

	protected override FilterExpression VisitFieldEqualsField(FieldEqualsFieldFilterExpression fieldEqualsFieldExpression, TInput input)
	{
		return fieldEqualsFieldExpression;
	}
}
