using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.OpenXml.Spreadsheet;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal class NavValueOfficeExcelConverter
{
	private static Dictionary<NavType, string> formatMap = new Dictionary<NavType, string>
	{
		{
			NavType.Decimal,
			NumberingFormatManager.NumberFormatsIndexedById(4u)
		},
		{
			NavType.Integer,
			NumberingFormatManager.NumberFormatsIndexedById(1u)
		},
		{
			NavType.BigInteger,
			NumberingFormatManager.NumberFormatsIndexedById(1u)
		},
		{
			NavType.Byte,
			NumberingFormatManager.NumberFormatsIndexedById(1u)
		},
		{
			NavType.Duration,
			NumberingFormatManager.NumberFormatsIndexedById(1u)
		},
		{
			NavType.Boolean,
			NumberingFormatManager.NumberFormatsIndexedById(1u)
		},
		{
			NavType.Date,
			NumberingFormatManager.NumberFormatsIndexedById(14u)
		},
		{
			NavType.DateTime,
			NumberingFormatManager.NumberFormatsIndexedById(168u)
		},
		{
			NavType.Time,
			NumberingFormatManager.NumberFormatsIndexedById(101u)
		}
	};

	private static string ConvertFieldTypeToFormat(NavValue navValue)
	{
		if (!formatMap.TryGetValue(navValue.NavType, out var value))
		{
			return NumberingFormatManager.NumberFormatsIndexedById(49u);
		}
		return value;
	}

	internal static CellValueTypeFormula NavValueToExcelCellValue(NavValue cellValue, string numberFormatString)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected Obj, but got Unknown
		CellValueTypeFormula val = new CellValueTypeFormula
		{
			DecimalValue = null
		};
		switch (cellValue.NclType)
		{
		case NavNclType.NavDuration:
			val.Value = NavFormatEvaluateHelper.Format(((NavDuration)cellValue).Value, 0, 9);
			break;
		case NavNclType.NavOption:
			val.Value = NavFormatEvaluateHelper.Format(cellValue, 0, 0);
			break;
		case NavNclType.NavDecimal:
			try
			{
				val.DecimalValue = (decimal)cellValue.ClientObject;
			}
			catch (InvalidCastException exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00006BJ", Category.Runtime, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata("Failed to cast NavDecimal to Decimal"), null, forceLog: true);
				val.Value = NavFormatEvaluateHelper.Format(cellValue, 0, "<Precision,2:10><Standard Format,0>");
			}
			break;
		case NavNclType.NavBoolean:
		{
			bool value = ((NavBoolean)cellValue).Value;
			val.Value = (value ? "1" : "0");
			val.Formula = (value ? "TRUE()" : "FALSE()");
			break;
		}
		default:
			val.Value = NavValueToExcelString(cellValue);
			break;
		}
		val.NumberFormat = (string.IsNullOrWhiteSpace(numberFormatString) ? ConvertFieldTypeToFormat(cellValue) : numberFormatString);
		return val;
	}

	private static string NavValueToExcelString(NavValue value)
	{
		DateTime dateTime;
		switch (value.NclType)
		{
		case NavNclType.NavDateTime:
			dateTime = ((NavDateTime)value).Value;
			if (dateTime.Ticks != 0L)
			{
				dateTime = dateTime.ToLocalTime();
			}
			break;
		case NavNclType.NavTime:
			dateTime = DateTime.MinValue.Add(((NavTime)value).Value.TimeOfDay);
			break;
		case NavNclType.NavDate:
			dateTime = ((NavDate)value).Value;
			break;
		default:
			return NavFormatEvaluateHelper.Format(value, 0, 9);
		}
		return dateTime.ToOADate().ToString(CultureInfo.InvariantCulture);
	}
}
