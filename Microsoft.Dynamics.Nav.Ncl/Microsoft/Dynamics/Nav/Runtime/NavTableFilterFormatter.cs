using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTableFilterFormatter : NavValueFormatter
{
	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatImpl((NavTableFilter)value);
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Option);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		return FormatSettings.DefaultStandardFormatString;
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		return FormatImpl((NavTableFilter)value);
	}

	private static string FormatImpl(NavTableFilter tableFilter)
	{
		if (tableFilter.IsZeroOrEmpty)
		{
			return string.Empty;
		}
		NCLMetaTable metaTable = tableFilter.MetaTable;
		FilterFieldDictionary fieldFiltersWithUnknownFields = tableFilter.FieldFiltersWithUnknownFields;
		StringBuilder stringBuilder = new StringBuilder();
		if (metaTable == null)
		{
			stringBuilder.Append(ObjectType.Table).Append(tableFilter.TableNo);
		}
		else
		{
			NavValueFormatter.AppendValueQuoteIfNecessary(stringBuilder, metaTable.TableName, ':', quoteEmpty: false);
		}
		stringBuilder.Append(':').Append(' ');
		Tuple<INavFieldMetadata, FilterExpression>[] items = fieldFiltersWithUnknownFields.Items;
		for (int i = 0; i < items.Length; i++)
		{
			Tuple<INavFieldMetadata, FilterExpression> tuple = items[i];
			if (tuple.Item1 is NCLMetaField nCLMetaField)
			{
				NavValueFormatter.AppendValueQuoteIfNecessary(stringBuilder, nCLMetaField.FieldCaption, '=', quoteEmpty: false);
			}
			else
			{
				UnkownFieldMetadata unkownFieldMetadata = (UnkownFieldMetadata)tuple.Item1;
				stringBuilder.Append("Field").Append(unkownFieldMetadata.FieldNo);
			}
			stringBuilder.Append('=');
			NavValueFormatter.AppendValueQuoteIfNecessary(stringBuilder, tuple.Item2.ToString(FilterExpressionStringOptions.BooleanConstantTrueAsEmpty), ',', quoteEmpty: false);
			if (i < items.Length - 1)
			{
				stringBuilder.Append(',');
			}
		}
		return stringBuilder.ToString();
	}
}
