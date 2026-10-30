namespace Microsoft.Dynamics.Nav.Runtime;

internal class TransferExpressionContextVisitor : ModifyingExpressionVisitor<FilterExpressionContext>
{
	public static readonly TransferExpressionContextVisitor Instance = new TransferExpressionContextVisitor();

	private TransferExpressionContextVisitor()
	{
	}

	private static NavValue CoerceValue(NavValue sourceValue, INavFieldMetadata destMetadata)
	{
		if (sourceValue == null)
		{
			return null;
		}
		return NavValue.CreateNavValueFromObject(destMetadata, sourceValue);
	}

	protected override FilterExpression VisitFieldEqualsField(FieldEqualsFieldFilterExpression fieldEqualsFieldExpression, FilterExpressionContext input)
	{
		return FilterExpression.FieldEqualsField(fieldEqualsFieldExpression.FieldToCompareTo, input);
	}

	protected override FilterExpression VisitRange(RangeFilterExpression rangeFilterExpression, FilterExpressionContext input)
	{
		return new RangeFilterExpression(rangeFilterExpression.ExpressionType, CoerceValue(rangeFilterExpression.LowValue, input.Metadata), CoerceValue(rangeFilterExpression.HighValue, input.Metadata), input);
	}

	protected override FilterExpression VisitUnary(UnaryFilterExpression unaryExpression, FilterExpressionContext input)
	{
		return new UnaryFilterExpression(unaryExpression.ExpressionType, CoerceValue(unaryExpression.Value, input.Metadata), input);
	}

	protected override FilterExpression VisitWildcard(WildcardFilterExpression wildcardExpression, FilterExpressionContext input)
	{
		return new WildcardFilterExpression(wildcardExpression.IsNegated, wildcardExpression.Pattern, wildcardExpression.IsCaseAndAccentInsensitive, input);
	}
}
