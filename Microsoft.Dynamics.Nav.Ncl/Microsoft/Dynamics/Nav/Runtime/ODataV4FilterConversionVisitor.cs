using System;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ODataV4FilterConversionVisitor : FilterExpressionVisitor<NCLMetaField, StringBuilder>
{
	private static Func<NCLMetaField, string> defaultFieldNameTransform = (NCLMetaField fieldMeta) => MetaBase.ExternalizeName(fieldMeta.FieldName);

	private Func<NCLMetaField, string> fieldNameTransform;

	internal ODataV4FilterConversionVisitor()
	{
		fieldNameTransform = defaultFieldNameTransform;
	}

	internal ODataV4FilterConversionVisitor(Func<NCLMetaField, string> fieldNameTransformFunction)
	{
		fieldNameTransform = fieldNameTransformFunction;
	}

	protected override StringBuilder VisitBinary(BinaryFilterExpression binaryExpression, NCLMetaField input)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('(');
		stringBuilder.Append(Visit(binaryExpression.Left, input).ToString());
		stringBuilder.Append(' ');
		stringBuilder.AppendBinaryExpressionOperator(binaryExpression.ExpressionType);
		stringBuilder.Append(' ');
		stringBuilder.Append(Visit(binaryExpression.Right, input).ToString());
		stringBuilder.Append(')');
		return stringBuilder;
	}

	protected override StringBuilder VisitBooleanConstant(BooleanConstantFilterExpression booleanConstantExpression, NCLMetaField input)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(booleanConstantExpression.Value);
		return stringBuilder;
	}

	protected override StringBuilder VisitFieldEqualsField(FieldEqualsFieldFilterExpression fieldEqualsFieldExpression, NCLMetaField input)
	{
		throw new NotSupportedException();
	}

	protected override StringBuilder VisitUnary(UnaryFilterExpression unaryExpression, NCLMetaField input)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendODatav4Filter(unaryExpression.ExpressionType, input, unaryExpression.Value, fieldNameTransform);
		return stringBuilder;
	}

	protected override StringBuilder VisitWildcard(WildcardFilterExpression wildcardExpression, NCLMetaField input)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (wildcardExpression.IsCaseAndAccentInsensitive ? "@" : string.Empty);
		NavText value = NavText.Create(text + wildcardExpression.Pattern);
		stringBuilder.AppendODatav4Filter(FilterExpressionType.Equal, input, value, fieldNameTransform);
		return stringBuilder;
	}

	protected override StringBuilder VisitRange(RangeFilterExpression rangeFilterExpression, NCLMetaField input)
	{
		StringBuilder stringBuilder = new StringBuilder();
		switch (rangeFilterExpression.ExpressionType)
		{
		case FilterExpressionType.RangeToInclusive:
			stringBuilder.AppendODatav4Filter(rangeFilterExpression.ExpressionType, input, rangeFilterExpression.HighValue, fieldNameTransform);
			break;
		case FilterExpressionType.RangeFromInclusive:
			stringBuilder.AppendODatav4Filter(rangeFilterExpression.ExpressionType, input, rangeFilterExpression.LowValue, fieldNameTransform);
			break;
		case FilterExpressionType.RangeBetweenInclusive:
			if (input.FieldNavType == NavType.Option)
			{
				NCLOptionMetadata fieldOptionMetadata = input.FieldOptionMetadata;
				int indexFromOption = fieldOptionMetadata.GetIndexFromOption(rangeFilterExpression.LowValue.ToString());
				int indexFromOption2 = fieldOptionMetadata.GetIndexFromOption(rangeFilterExpression.HighValue.ToString());
				UnaryFilterExpression unaryFilterExpression = null;
				BinaryFilterExpression binaryFilterExpression = null;
				for (int i = indexFromOption; i <= indexFromOption2; i++)
				{
					NavOption value = NavOption.Create(fieldOptionMetadata, i);
					FilterExpression filterExpression = (FilterExpression)(((object)binaryFilterExpression) ?? ((object)unaryFilterExpression));
					unaryFilterExpression = new UnaryFilterExpression(FilterExpressionType.Equal, value, rangeFilterExpression.ExpressionContext);
					if (filterExpression != null)
					{
						binaryFilterExpression = new BinaryFilterExpression(FilterExpressionType.Or, filterExpression, unaryFilterExpression);
					}
				}
				stringBuilder.Append(Visit(binaryFilterExpression, input));
			}
			else
			{
				stringBuilder.AppendODatav4Filter(FilterExpressionType.GreaterThanOrEqual, input, rangeFilterExpression.LowValue, fieldNameTransform);
				stringBuilder.Append(' ');
				stringBuilder.AppendBinaryExpressionOperator(FilterExpressionType.And);
				stringBuilder.Append(' ');
				stringBuilder.AppendODatav4Filter(FilterExpressionType.LessThanOrEqual, input, rangeFilterExpression.HighValue, fieldNameTransform);
			}
			break;
		}
		return stringBuilder;
	}
}
