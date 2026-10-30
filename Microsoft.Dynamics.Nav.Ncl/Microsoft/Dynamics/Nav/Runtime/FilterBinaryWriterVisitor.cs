using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FilterBinaryWriterVisitor : FilterExpressionVisitor<BinaryWriter, BinaryWriter>
{
	private static readonly FilterBinaryWriterVisitor instance = new FilterBinaryWriterVisitor();

	internal static void WriteToBinaryWriter(FilterExpression filterExpression, BinaryWriter writer)
	{
		filterExpression.Visit(instance, writer);
		writer.Write((byte)0);
	}

	protected override BinaryWriter VisitBinary(BinaryFilterExpression binaryExpression, BinaryWriter input)
	{
		return binaryExpression.ExpressionType switch
		{
			FilterExpressionType.And => VisitAnd(binaryExpression, input), 
			FilterExpressionType.Or => VisitOr(binaryExpression, input), 
			_ => throw new NotSupportedException(), 
		};
	}

	private BinaryWriter VisitOr(BinaryFilterExpression binaryExpression, BinaryWriter input)
	{
		Visit(binaryExpression.Left, input);
		input.Write((byte)11);
		if (binaryExpression.Right.ExpressionType == FilterExpressionType.Or)
		{
			return Parenthesize(binaryExpression.Right, input);
		}
		return Visit(binaryExpression.Right, input);
	}

	private BinaryWriter VisitAnd(BinaryFilterExpression binaryExpression, BinaryWriter input)
	{
		if (binaryExpression.Left.ExpressionType == FilterExpressionType.Or)
		{
			Parenthesize(binaryExpression.Left, input);
		}
		else
		{
			Visit(binaryExpression.Left, input);
		}
		input.Write((byte)10);
		if (binaryExpression.Right.ExpressionType == FilterExpressionType.Or || binaryExpression.Right.ExpressionType == FilterExpressionType.And)
		{
			Parenthesize(binaryExpression.Right, input);
		}
		else
		{
			Visit(binaryExpression.Right, input);
		}
		return input;
	}

	private BinaryWriter Parenthesize(FilterExpression filterExpression, BinaryWriter binaryWriter)
	{
		binaryWriter.Write((byte)12);
		Visit(filterExpression, binaryWriter);
		binaryWriter.Write((byte)13);
		return binaryWriter;
	}

	protected override BinaryWriter VisitUnary(UnaryFilterExpression unaryExpression, BinaryWriter input)
	{
		switch (unaryExpression.ExpressionType)
		{
		case FilterExpressionType.Equal:
			if (unaryExpression.ExpressionContext.IsStringBased && FilterExpressionParser.IsWildcard(unaryExpression.ToString()))
			{
				input.Write((byte)2);
			}
			else
			{
				input.Write((byte)FilterExpressionUnmanagedToken.Equal.MakeSpecial());
			}
			break;
		case FilterExpressionType.NotEqual:
			input.Write((byte)3);
			break;
		case FilterExpressionType.GreaterThan:
			input.Write((byte)7);
			break;
		case FilterExpressionType.LessThan:
			input.Write((byte)5);
			break;
		case FilterExpressionType.GreaterThanOrEqual:
			input.Write((byte)4);
			break;
		case FilterExpressionType.LessThanOrEqual:
			input.Write((byte)6);
			break;
		default:
			throw new NotSupportedException();
		}
		return WriteValue(unaryExpression.Value, input, unaryExpression.ExpressionContext);
	}

	protected override BinaryWriter VisitRange(RangeFilterExpression rangeFilterExpression, BinaryWriter input)
	{
		switch (rangeFilterExpression.ExpressionType)
		{
		case FilterExpressionType.RangeBetweenInclusive:
			input.Write((byte)FilterExpressionUnmanagedToken.GreaterOrEqual.MakeSpecial());
			WriteValue(rangeFilterExpression.LowValue, input, rangeFilterExpression.ExpressionContext);
			input.Write((byte)FilterExpressionUnmanagedToken.And.MakeSpecial());
			input.Write((byte)FilterExpressionUnmanagedToken.LessThanOrEqual.MakeSpecial());
			WriteValue(rangeFilterExpression.HighValue, input, rangeFilterExpression.ExpressionContext);
			break;
		case FilterExpressionType.RangeFromInclusive:
			input.Write((byte)FilterExpressionUnmanagedToken.GreaterOrEqual.MakeSpecial());
			WriteValue(rangeFilterExpression.LowValue, input, rangeFilterExpression.ExpressionContext);
			break;
		case FilterExpressionType.RangeToInclusive:
			input.Write((byte)FilterExpressionUnmanagedToken.LessThanOrEqual.MakeSpecial());
			WriteValue(rangeFilterExpression.HighValue, input, rangeFilterExpression.ExpressionContext);
			break;
		default:
			throw new NotSupportedException();
		}
		return input;
	}

	protected override BinaryWriter VisitWildcard(WildcardFilterExpression wildcardExpression, BinaryWriter input)
	{
		input.Write((byte)(wildcardExpression.IsNegated ? 9 : 8));
		string input2 = (wildcardExpression.IsCaseAndAccentInsensitive ? ("@" + wildcardExpression.Pattern) : wildcardExpression.Pattern);
		NavValue navValue = wildcardExpression.ExpressionContext.Parse(input2);
		return WriteValue(navValue, input, wildcardExpression.ExpressionContext);
	}

	protected override BinaryWriter VisitBooleanConstant(BooleanConstantFilterExpression booleanConstantExpression, BinaryWriter input)
	{
		if (!booleanConstantExpression.Value)
		{
			throw new NotSupportedException();
		}
		return input;
	}

	protected override BinaryWriter VisitFieldEqualsField(FieldEqualsFieldFilterExpression fieldEqualsFieldExpression, BinaryWriter input)
	{
		throw new NotSupportedException();
	}

	private static BinaryWriter WriteValue(NavValue navValue, BinaryWriter input, FilterExpressionContext expressionContext)
	{
		byte[] bytes = navValue.GetBytes();
		int num = expressionContext.LogicalLengthFromByteLength(bytes.Length);
		input.Write((byte)num);
		input.Write(bytes);
		return input;
	}
}
