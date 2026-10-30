using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DateDataProvider : RangeBasedComputedDataProvider
{
	private const DatePeriodType MinPeriodType = DatePeriodType.Date;

	private const DatePeriodType MaxPeriodType = DatePeriodType.Year;

	private const int PeriodTypeIndex = 1;

	public static readonly DateDataProvider Instance = new DateDataProvider();

	public override int TableId => 2000000007;

	private DateDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (keyField.FieldNo == 1)
		{
			NCLOptionMetadata fieldOptionMetadata = MetaTable.GetFieldByIndex(1).FieldOptionMetadata;
			return ComputedVirtualDataProvider.EnumerateOptionsInRange(fieldOptionMetadata, range, sortOrder, (NavOption option) => new ReadOnlyRecordBuffer(MetaTable, null, option));
		}
		DatePeriodType datePeriodType = (DatePeriodType)precedingKeyFieldValues[1].ToInt32();
		if (datePeriodType < DatePeriodType.Date || datePeriodType > DatePeriodType.Year)
		{
			return new ReadOnlyRecordBuffer[0];
		}
		return EnumeratePeriods(datePeriodType, range, sortOrder, nonPrimaryKeyFilters);
	}

	protected override int CountValuesWithinRange(ReadOnlyRecordBuffer precedingKeyFieldValues, Range range)
	{
		DateTime lowPeriodStart;
		DateTime highPeriodStart;
		return CountPeriodsWithinRange((DatePeriodType)precedingKeyFieldValues[1].ToInt32(), range, out lowPeriodStart, out highPeriodStart);
	}

	private IEnumerable<ReadOnlyRecordBuffer> EnumeratePeriods(DatePeriodType periodType, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		int periodsInRange = CountPeriodsWithinRange(periodType, range, out var lowPeriodStart, out var highPeriodStart);
		if (periodsInRange == 0)
		{
			yield break;
		}
		DateTime current = ((sortOrder == SortOrder.Ascending) ? lowPeriodStart : highPeriodStart);
		int i = 0;
		while (true)
		{
			ReadOnlyRecordBuffer readOnlyRecordBuffer = CreateRecordBufferIfPassesNonPrimaryKeyFilters(periodType, current, nonPrimaryKeyFilters);
			if (readOnlyRecordBuffer != null)
			{
				yield return readOnlyRecordBuffer;
			}
			int num = i + 1;
			i = num;
			if (num != periodsInRange)
			{
				try
				{
					current = ToNextPeriodStart(current, periodType, sortOrder);
				}
				catch (ArgumentOutOfRangeException)
				{
					break;
				}
				continue;
			}
			break;
		}
	}

	private ReadOnlyRecordBuffer CreateRecordBufferIfPassesNonPrimaryKeyFilters(DatePeriodType periodType, DateTime periodStart, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		FilterExpression filterExpression = null;
		FilterExpression filterExpression2 = null;
		FilterExpression filterExpression3 = null;
		if (nonPrimaryKeyFilters != null)
		{
			Tuple<INavFieldMetadata, FilterExpression>[] items = nonPrimaryKeyFilters.Items;
			foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
			{
				switch (((NCLMetaField)tuple.Item1).FieldNo)
				{
				case 3:
					filterExpression = tuple.Item2;
					break;
				case 4:
					filterExpression2 = tuple.Item2;
					break;
				case 5:
					filterExpression3 = tuple.Item2;
					break;
				}
			}
		}
		DateTimeHelper.DatePeriodRoundUp(periodStart, periodType, out var result);
		NavDate navDate = NavDate.CreateDate(result, closing: true);
		if (filterExpression != null && !filterExpression.Evaluate(navDate))
		{
			return null;
		}
		NavInteger navInteger = NavInteger.Create(CalculatePeriodNumber(periodType, periodStart));
		if (filterExpression2 != null && !filterExpression2.Evaluate(navInteger))
		{
			return null;
		}
		NavText truncatedTextValue = GetTruncatedTextValue(5, GetPeriodName(periodType, periodStart));
		if (filterExpression3 != null && !filterExpression3.Evaluate(truncatedTextValue))
		{
			return null;
		}
		return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, GetPeriodTypeOption(periodType), NavDate.CreateDate(periodStart, closing: false), navDate, navInteger, truncatedTextValue);
	}

	private static int CalculatePeriodNumber(DatePeriodType periodType, DateTime periodStart)
	{
		switch (periodType)
		{
		case DatePeriodType.Date:
		{
			DayOfWeek dayOfWeek = periodStart.DayOfWeek;
			if (dayOfWeek != DayOfWeek.Sunday)
			{
				return (int)dayOfWeek;
			}
			return 7;
		}
		case DatePeriodType.Week:
		{
			DateTimeHelper.DateTimeToIsoYearWeek(periodStart, out var _, out var weekNo);
			return weekNo;
		}
		case DatePeriodType.Month:
			return periodStart.Month;
		case DatePeriodType.Quarter:
			return (periodStart.Month - 1) / 3 + 1;
		case DatePeriodType.Year:
			return periodStart.Year;
		default:
			throw new NotSupportedException();
		}
	}

	private static string GetPeriodName(DatePeriodType periodType, DateTime periodStart)
	{
		return periodType switch
		{
			DatePeriodType.Date => NavCurrentThread.Session.FormatSettings.DateTextWeekdayNames[(int)periodStart.DayOfWeek], 
			DatePeriodType.Month => NavCurrentThread.Session.FormatSettings.DateTextMonthNames[periodStart.Month - 1], 
			_ => CalculatePeriodNumber(periodType, periodStart).ToString(CultureInfo.CurrentCulture), 
		};
	}

	private static int CountPeriodsWithinRange(DatePeriodType periodType, Range range, out DateTime lowPeriodStart, out DateTime highPeriodStart)
	{
		try
		{
			if (range.IsLowIsMinimum)
			{
				lowPeriodStart = DateTimeHelper.DatePeriodStartMinimumDate(periodType);
			}
			else
			{
				lowPeriodStart = range.LowValue.ToDateTime();
				if (lowPeriodStart < DateTimeHelper.DatePeriodStartMinimumDate(periodType))
				{
					lowPeriodStart = DateTimeHelper.DatePeriodStartMinimumDate(periodType);
				}
				else if (DateTimeHelper.IsDateAtStartOfPeriod(lowPeriodStart, periodType))
				{
					if (range.IsLowExcluded)
					{
						lowPeriodStart = ToNextPeriodStart(lowPeriodStart, periodType, SortOrder.Ascending);
					}
				}
				else
				{
					DateTimeHelper.DatePeriodRoundUp(lowPeriodStart, periodType, out lowPeriodStart);
					lowPeriodStart = ToNextPeriodStart(lowPeriodStart, DatePeriodType.Date, SortOrder.Ascending);
				}
			}
			if (range.IsHighMaximum)
			{
				highPeriodStart = DateTimeHelper.DatePeriodStartMaximumDate(periodType);
			}
			else
			{
				highPeriodStart = range.HighValue.ToDateTime();
				if (highPeriodStart > DateTimeHelper.DatePeriodStartMaximumDate(periodType))
				{
					highPeriodStart = DateTimeHelper.DatePeriodStartMaximumDate(periodType);
				}
				else if (DateTimeHelper.IsDateAtStartOfPeriod(highPeriodStart, periodType))
				{
					if (range.IsHighExcluded)
					{
						highPeriodStart = ToNextPeriodStart(highPeriodStart, periodType, SortOrder.Descending);
					}
				}
				else
				{
					DateTimeHelper.DatePeriodRoundDown(highPeriodStart, periodType, out highPeriodStart);
				}
			}
			return Math.Max(0, DifferenceBetweenPeriods(periodType, lowPeriodStart, highPeriodStart) + 1);
		}
		catch (ArgumentOutOfRangeException)
		{
			lowPeriodStart = DateTime.MaxValue;
			highPeriodStart = DateTime.MinValue;
			return 0;
		}
	}

	private static int DifferenceBetweenPeriods(DatePeriodType periodType, DateTime lowPeriodStart, DateTime highPeriodStart)
	{
		return periodType switch
		{
			DatePeriodType.Date => (int)((highPeriodStart - lowPeriodStart).Ticks / 864000000000L), 
			DatePeriodType.Week => (int)((highPeriodStart - lowPeriodStart).Ticks / 864000000000L) / 7, 
			DatePeriodType.Month => (highPeriodStart.Year - lowPeriodStart.Year) * 12 + (highPeriodStart.Month - lowPeriodStart.Month), 
			DatePeriodType.Quarter => ((highPeriodStart.Year - lowPeriodStart.Year) * 12 + (highPeriodStart.Month - lowPeriodStart.Month)) / 3, 
			DatePeriodType.Year => highPeriodStart.Year - lowPeriodStart.Year, 
			_ => throw new NotSupportedException(), 
		};
	}

	public static DateTime ToNextPeriodStart(DateTime inputPeriodStart, DatePeriodType periodType, SortOrder sortOrder)
	{
		return periodType switch
		{
			DatePeriodType.Date => inputPeriodStart.AddTicks((sortOrder == SortOrder.Ascending) ? 864000000000L : (-864000000000L)), 
			DatePeriodType.Week => inputPeriodStart.AddTicks((sortOrder == SortOrder.Ascending) ? 6048000000000L : (-6048000000000L)), 
			DatePeriodType.Month => inputPeriodStart.AddMonths((sortOrder == SortOrder.Ascending) ? 1 : (-1)), 
			DatePeriodType.Quarter => inputPeriodStart.AddMonths((sortOrder == SortOrder.Ascending) ? 3 : (-3)), 
			DatePeriodType.Year => new DateTime(inputPeriodStart.Year + ((sortOrder == SortOrder.Ascending) ? 1 : (-1)), 1, 1), 
			_ => throw new NotSupportedException(), 
		};
	}

	private NavOption GetPeriodTypeOption(DatePeriodType periodType)
	{
		return GetOptionValue(1, (int)periodType);
	}
}
