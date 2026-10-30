using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class StringBuilderExtensions
{
	internal static void AppendODatav4Filter(this StringBuilder sb, FilterExpressionType expressionType, NCLMetaField fieldMetadata, NavValue value, Func<NCLMetaField, string> fieldNameTransform)
	{
		AppendODataFilter(sb, expressionType, fieldMetadata, value, useODatav3: false, fieldNameTransform);
	}

	internal static void AppendODatav3Filter(this StringBuilder sb, FilterExpressionType expressionType, NCLMetaField fieldMetadata, NavValue value, Func<NCLMetaField, string> fieldNameTransform)
	{
		AppendODataFilter(sb, expressionType, fieldMetadata, value, useODatav3: true, fieldNameTransform);
	}

	internal static void AppendBinaryExpressionOperator(this StringBuilder sb, FilterExpressionType expressionType)
	{
		sb.Append(convertToOperator(expressionType));
	}

	private static void AppendODataFilter(StringBuilder sb, FilterExpressionType expressionType, NCLMetaField fieldMetadata, NavValue value, bool useODatav3, Func<NCLMetaField, string> fieldNameTransform)
	{
		sb.Append(fieldNameTransform(fieldMetadata));
		sb.Append(' ');
		sb.Append(convertToOperator(expressionType));
		sb.Append(' ');
		sb.Append(ConvertNavValueToODataValue(value, useODatav3));
	}

	private static string ConvertNavValueToODataValue(NavValue value, bool useV3Conversion = false)
	{
		string result = string.Empty;
		switch (value.NavType)
		{
		case NavType.Date:
		{
			DateTime value3 = ((NavDate)value).Value;
			DateTime datetime2 = new DateTime(value3.Year, value3.Month, value3.Day, 0, 0, 0, 0, DateTimeKind.Utc);
			result = GetUtcFormattedDateTime(useV3Conversion, datetime2);
			break;
		}
		case NavType.Time:
		{
			DateTime value2 = ((NavTime)value).Value;
			DateTime datetime = new DateTime(0, 0, 0, value2.Hour, value2.Minute, value2.Second, value2.Millisecond, DateTimeKind.Local);
			result = GetUtcFormattedDateTime(useV3Conversion, datetime);
			break;
		}
		case NavType.DateTime:
		{
			DateTime value3 = (DateTime)((NavDateTimeValue)value).ClientObject;
			result = GetUtcFormattedDateTime(useV3Conversion, value3);
			break;
		}
		case NavType.RecordID:
		case NavType.OemText:
		case NavType.DateFormula:
		case NavType.TextConst:
		case NavType.Char:
		case NavType.Text:
		case NavType.Code:
		case NavType.OemCode:
		case NavType.Option:
		case NavType.GUID:
		case NavType.BigText:
		case NavType.String:
			result = string.Format(CultureInfo.InvariantCulture, "'{0}'", value.ToString());
			break;
		case NavType.Boolean:
			result = value.ToBoolean().ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
			break;
		case NavType.Integer:
		case NavType.BigInteger:
			result = value.ToString();
			break;
		case NavType.Decimal:
			result = (useV3Conversion ? string.Format(CultureInfo.InvariantCulture, "{0}M", value.ClientObject.ToString()) : value.ClientObject.ToString());
			break;
		}
		return result;
	}

	private static string GetUtcFormattedDateTime(bool useV3Conversion, DateTime datetime)
	{
		string format = (useV3Conversion ? "DateTime'{0}'" : "{0}Z");
		return string.Format(CultureInfo.InvariantCulture, format, GetODataFormattedOffsetString(datetime));
	}

	private static string GetODataFormattedOffsetString(DateTime datetime)
	{
		string text = datetime.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
		if (!text.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
		{
			return text;
		}
		return text.Remove(text.Length - 1);
	}

	private static string convertToOperator(FilterExpressionType expressionOperator)
	{
		switch (expressionOperator)
		{
		case FilterExpressionType.Equal:
			return "eq";
		case FilterExpressionType.LessThan:
			return "lt";
		case FilterExpressionType.GreaterThan:
			return "gt";
		case FilterExpressionType.LessThanOrEqual:
		case FilterExpressionType.RangeToInclusive:
			return "le";
		case FilterExpressionType.GreaterThanOrEqual:
		case FilterExpressionType.RangeFromInclusive:
			return "ge";
		case FilterExpressionType.And:
			return "and";
		case FilterExpressionType.Or:
			return "or";
		case FilterExpressionType.NotEqual:
			return "ne";
		default:
			return string.Empty;
		}
	}

	public unsafe static StringBuilder AppendJoin(this StringBuilder builder, string separator, IEnumerable<string> values)
	{
		separator = separator ?? string.Empty;
		fixed (char* separator2 = separator)
		{
			return AppendJoinCore(builder, separator2, separator.Length, values);
		}
	}

	private unsafe static StringBuilder AppendJoinCore<T>(StringBuilder builder, char* separator, int separatorLength, IEnumerable<T> values)
	{
		if (values == null)
		{
			return builder;
		}
		using IEnumerator<T> enumerator = values.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return builder;
		}
		T current = enumerator.Current;
		if (current != null)
		{
			builder.Append(current);
		}
		while (enumerator.MoveNext())
		{
			builder.Append(separator, separatorLength);
			current = enumerator.Current;
			if (current != null)
			{
				builder.Append(current);
			}
		}
		return builder;
	}
}
