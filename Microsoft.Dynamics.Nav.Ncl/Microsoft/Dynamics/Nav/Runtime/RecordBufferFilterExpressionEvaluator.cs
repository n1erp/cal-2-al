using System;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class RecordBufferFilterExpressionEvaluator : FilterExpressionVisitor<RecordBufferFilterExpressionEvaluatorInput, bool>
{
	public static readonly RecordBufferFilterExpressionEvaluator Instance = new RecordBufferFilterExpressionEvaluator();

	private RecordBufferFilterExpressionEvaluator()
	{
	}

	public static bool EvaluateFieldFilter(INavFieldMetadata field, FilterExpression filter, RecordBufferFilterExpressionEvaluatorInput input)
	{
		int columnIndex = field.ColumnIndex;
		switch (field.FieldClass)
		{
		case FieldClass.FlowField:
			if (input.RecordBuffer[columnIndex] == null)
			{
				throw new InvalidOperationException("FlowField content is being evaluated but is not calculated yet");
			}
			break;
		case FieldClass.FlowFilter:
			return true;
		}
		if (input.CheckAgainstOriginalAndModified)
		{
			if (input.BothShouldPass)
			{
				return BothChangedAndOriginalValuesPassFilter(field, filter, input.RecordBuffer as MutableRecordBuffer);
			}
			return EitherChangedOrOriginalValuePassFilter(field, filter, input.RecordBuffer);
		}
		NavValue input2 = input.RecordBuffer[columnIndex] ?? field.InitValue;
		return filter.Evaluate(input2);
	}

	private static bool BothChangedAndOriginalValuesPassFilter(INavFieldMetadata field, FilterExpression filter, MutableRecordBuffer recordBuffer)
	{
		int columnIndex = field.ColumnIndex;
		NavValue changedFieldValue = recordBuffer.GetChangedFieldValue(columnIndex);
		if (changedFieldValue != null && !filter.Evaluate(changedFieldValue))
		{
			return false;
		}
		if (recordBuffer.Timestamp == null || recordBuffer.Timestamp.ToInt64() == 0L || recordBuffer.ReadOnlyBuffer.Timestamp == null)
		{
			return true;
		}
		NavValue input = recordBuffer.GetOriginalValue(columnIndex) ?? field.InitValue;
		if (!filter.Evaluate(input))
		{
			return false;
		}
		return true;
	}

	private static bool EitherChangedOrOriginalValuePassFilter(INavFieldMetadata field, FilterExpression filter, IRecordBuffer recordBuffer)
	{
		int columnIndex = field.ColumnIndex;
		NavValue changedFieldValue = recordBuffer.GetChangedFieldValue(columnIndex);
		if (changedFieldValue == null || !filter.Evaluate(changedFieldValue))
		{
			NavValue input = recordBuffer.GetOriginalValue(columnIndex) ?? field.InitValue;
			if (!filter.Evaluate(input))
			{
				return false;
			}
		}
		return true;
	}

	protected override bool VisitBinary(BinaryFilterExpression binaryExpression, RecordBufferFilterExpressionEvaluatorInput input)
	{
		switch (binaryExpression.ExpressionType)
		{
		case FilterExpressionType.And:
			if (Visit(binaryExpression.Left, input))
			{
				return Visit(binaryExpression.Right, input);
			}
			return false;
		case FilterExpressionType.Or:
			if (!Visit(binaryExpression.Left, input))
			{
				return Visit(binaryExpression.Right, input);
			}
			return true;
		default:
			throw new NotSupportedException();
		}
	}

	protected override bool VisitUnary(UnaryFilterExpression unaryExpression, RecordBufferFilterExpressionEvaluatorInput input)
	{
		return EvaluateFieldFilter(unaryExpression.ExpressionContext.Metadata, unaryExpression, input);
	}

	protected override bool VisitWildcard(WildcardFilterExpression wildcardExpression, RecordBufferFilterExpressionEvaluatorInput input)
	{
		return EvaluateFieldFilter(wildcardExpression.ExpressionContext.Metadata, wildcardExpression, input);
	}

	protected override bool VisitBooleanConstant(BooleanConstantFilterExpression booleanConstantExpression, RecordBufferFilterExpressionEvaluatorInput input)
	{
		return booleanConstantExpression.Value;
	}

	protected override bool VisitFieldEqualsField(FieldEqualsFieldFilterExpression fieldEqualsFieldExpression, RecordBufferFilterExpressionEvaluatorInput input)
	{
		throw new NotSupportedException();
	}
}
