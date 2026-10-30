using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavRecordIdFormatter : NavValueFormatter
{
	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		NavRecordId navRecordId = value as NavRecordId;
		if (navRecordId == null)
		{
			return string.Empty;
		}
		switch (formatNumber)
		{
		case 0:
		case 1:
		case 2:
		case 9:
		{
			StringBuilder stringBuilder = new StringBuilder(50);
			if (navRecordId.TableNo != 0)
			{
				string tableName;
				if (formatNumber == 1)
				{
					if (!TryGetTableCaption(navRecordId.TableNo, out tableName))
					{
						return tableName;
					}
				}
				else if (!TryGetTableName(navRecordId.TableNo, out tableName))
				{
					return tableName;
				}
				NavValueFormatter.AppendValueQuoteIfNecessary(stringBuilder, tableName, ':', quoteEmpty: true);
			}
			bool flag = true;
			foreach (NavValue field in navRecordId.Fields)
			{
				stringBuilder.Append(flag ? ": " : ",");
				flag = false;
				string value2 = NavFormatEvaluateHelper.Format(field, 0, formatNumber);
				NavValueFormatter.AppendValueQuoteIfNecessary(stringBuilder, value2, ',', quoteEmpty: true);
			}
			return stringBuilder.ToString();
		}
		case 10:
		{
			byte[] bytes = navRecordId.GetBytes();
			return NavBookmarkHelper.ConvertBookmarkToString(bytes);
		}
		default:
			throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.RecordID);
		}
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatSettings.GenericStdFormatStrings[formatNumber];
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.RecordID);
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		return Format(value, options.Length, format[pos++], settings);
	}

	private static bool TryGetTableName(int tableNumber, out string tableName)
	{
		if (NavGlobal.NCLMetadata.TryGetMetaTableById(tableNumber, out var metaTable))
		{
			tableName = metaTable.TableName;
			return true;
		}
		tableName = string.Format(CultureInfo.CurrentCulture, Lang.NavRecordIdFormatValueForNonExistingTableMetadata, tableNumber);
		return false;
	}

	private static bool TryGetTableCaption(int tableNumber, out string tableName)
	{
		if (NavGlobal.NCLMetadata.TryGetMetaTableById(tableNumber, out var metaTable))
		{
			tableName = metaTable.GetTableCaption(CultureInfo.CurrentCulture.LCID);
			return true;
		}
		tableName = string.Format(CultureInfo.CurrentCulture, Lang.NavRecordIdFormatValueForNonExistingTableMetadata, tableNumber);
		return false;
	}
}
