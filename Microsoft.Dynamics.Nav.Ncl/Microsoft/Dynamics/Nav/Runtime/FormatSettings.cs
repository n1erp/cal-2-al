using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class FormatSettings
{
	private const char LengthZero = '\0';

	private const char LengthTwo = '\u0002';

	private const char LengthThree = '\u0003';

	public const char StdComma = '\u0001';

	public const char StdThousand = '\u0002';

	public const char FormatZThousand = '\u0019';

	public const char FormatZComma = '\u001a';

	public const char FormatZOverflow = '\u001b';

	public const char FormatZPad = '\u001c';

	public const char FormatZDecimalPlaces = '\u001d';

	public const char TextField = '\u0003';

	public const char CharField = '\u0004';

	public const char SignField = '\u0003';

	public const char IntegerField = '\u0004';

	public const char IntegerThousandField = '\u0005';

	public const char OptionTextField = '\u0003';

	public const char OptionNumberField = '\u0004';

	public const char OptionXMLField = '\u0005';

	public const char DateDayField = '\u0003';

	public const char DateMonthField = '\u0004';

	public const char DateMonthTxtField = '\u0005';

	public const char DateQuarterField = '\u0006';

	public const char DateYear2Field = '\a';

	public const char DateYear4Field = '\b';

	public const char DateWeekField = '\t';

	public const char DateWeekYear2Field = '\n';

	public const char DateWeekYear4Field = '\v';

	public const char DateWeekdayField = '\f';

	public const char DateWeekdayTxtField = '\r';

	public const char DateUltimoField = '\u000e';

	public const char TimeHour24Field = '\u0003';

	public const char TimeHour12Field = '\u0004';

	public const char TimeMinuteField = '\u0005';

	public const char TimeSecondField = '\u0006';

	public const char TimeThousandField = '\a';

	public const char Time12HourField = '\b';

	public const char TimeSecFractionField = '\t';

	public const char TimeHourZero24Field = '\n';

	public const char TimeHourZero12Field = '\v';

	public const char DateTimeDayField = '\u0003';

	public const char DateTimeMonthField = '\u0004';

	public const char DateTimeMonthTxtField = '\u0005';

	public const char DateTimeQuarterField = '\u0006';

	public const char DateTimeYear2Field = '\a';

	public const char DateTimeYear4Field = '\b';

	public const char DateTimeWeekField = '\t';

	public const char DateTimeWeekYear2Field = '\n';

	public const char DateTimeWeekYear4Field = '\v';

	public const char DateTimeWeekdayField = '\f';

	public const char DateTimeWeekdayTxtField = '\r';

	public const char DateTimeHour24Field = '\u000e';

	public const char DateTimeHour12Field = '\u000f';

	public const char DateTimeMinuteField = '\u0010';

	public const char DateTimeSecondField = '\u0011';

	public const char DateTimeThousandField = '\u0012';

	public const char DateTime12HourField = '\u0013';

	public const char DateTimeSecFractionField = '\u0014';

	public const char DecimalSignField = '\u0003';

	public const char DecimalIntegerField = '\u0004';

	public const char DecimalDecimalsField = '\u0005';

	public const char DecimalThousandField = '\u0006';

	public static readonly string DefaultStandardFormatString = BuildFormat(default(char), default(char));

	public static readonly int DefaultTwoDigitYearMax = new GregorianCalendar().TwoDigitYearMax;

	private static readonly FormatOptions[] dateTypeFmt = new FormatOptions[12]
	{
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('\0', '\0', '\0', '\0', leftJustify: false, 0, 0),
		new FormatOptions('\0', '\0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('\0', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('\0', '\0', '\0', '\0', leftJustify: false, 0, 0),
		new FormatOptions('\0', '\0', '\0', '\0', leftJustify: true, 0, 0)
	};

	private static readonly FormatOptions[] timeTypeFmt = new FormatOptions[9]
	{
		new FormatOptions('*', '\0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '\0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '\0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('\0', '0', '\0', '\0', leftJustify: false, 0, 0),
		new FormatOptions('*', '0', '\u0001', '\0', leftJustify: false, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0)
	};

	private static readonly FormatOptions[] datetimeTypeFmt = new FormatOptions[18]
	{
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('\0', '\0', '\0', '\0', leftJustify: false, 0, 0),
		new FormatOptions('\0', '\0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('\0', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('\0', '\0', '\0', '\0', leftJustify: false, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('\0', '0', '\0', '\0', leftJustify: false, 0, 0),
		new FormatOptions('*', '0', '\u0001', '\0', leftJustify: false, 0, 0)
	};

	private static readonly FormatOptions[] decimalTypeFmt = new FormatOptions[4]
	{
		new FormatOptions('\0', '\0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '\0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '0', '\u0001', '\0', leftJustify: false, 0, 0),
		new FormatOptions('*', '\0', '\0', '\u0002', leftJustify: true, 0, 0)
	};

	private static readonly FormatOptions[] integerTypeFmt = new FormatOptions[3]
	{
		new FormatOptions('\0', '\0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '\0', '\0', '\0', leftJustify: true, 0, 0),
		new FormatOptions('*', '\0', '\0', '\u0002', leftJustify: true, 0, 0)
	};

	private string[] dateQuantor;

	private string[] durationUnits;

	private bool durationUnitsInitialized;

	internal static FormatOptions[] DateTypeFmt => dateTypeFmt;

	internal static FormatOptions[] TimeTypeFmt => timeTypeFmt;

	internal static FormatOptions[] DatetimeTypeFmt => datetimeTypeFmt;

	internal static FormatOptions[] DecimalTypeFmt => decimalTypeFmt;

	internal static FormatOptions[] IntegerTypeFmt => integerTypeFmt;

	internal string[] DateStdFormatStrings { get; }

	internal string[] TimeStdFormatStrings { get; }

	internal string[] DatetimeStdFormatStrings { get; }

	internal static string[] DecimalStdFormatStrings { get; } = BuildDecimalStdFormatStrings();

	internal static string[] OptionStdFormatStrings { get; } = BuildOptionStdFormatStrings();

	internal static string CharStdFormatStrings { get; } = BuildFormat('\u0004', '\0');

	internal static string TextStdFormatStrings { get; } = BuildFormat('\u0003', '\0');

	internal static string IntegerStdFormatStrings { get; } = BuildFormat('\u0003', '\0', '\u0004', '\0');

	internal string DateTextEvalOrder { get; private set; }

	internal string ClosingDateCharacter { get; private set; }

	internal string WorkDate { get; private set; }

	internal string ToDay { get; private set; }

	internal string Time { get; private set; }

	internal int DateRangeCentury { get; private set; }

	internal CultureInfo ServerCultureInfo { get; private set; }

	internal string[] DateTextMonthNames => ServerCultureInfo.DateTimeFormat.MonthNames;

	internal string[] DateTextWeekdayNames => ServerCultureInfo.DateTimeFormat.DayNames;

	internal string DateSeparator { get; private set; }

	internal string TimeSeparator { get; private set; }

	internal string DecimalSeparator { get; private set; }

	internal string ShortDatePattern { get; private set; }

	internal string LongTimePattern { get; private set; }

	internal string ShortTimePattern { get; private set; }

	internal char FieldFormatComma { get; private set; }

	internal char FieldFormatThousand { get; private set; }

	internal string TimeAMDesignator { get; private set; }

	internal string TimePMDesignator { get; private set; }

	internal string[] BooleanOptions { get; private set; }

	internal static string[] DurationStdFormatStrings => GenericStdFormatStrings;

	internal static string[] DateFormulaStdFormatStrings => GenericStdFormatStrings;

	internal static string[] GenericStdFormatStrings { get; } = BuildGenericStdFormatStrings();

	internal static string[] GlobalDateQuantor { get; } = new string[7] { "C", "D", "WD", "W", "M", "Q", "Y" };

	internal string[] DateQuantor
	{
		get
		{
			if (dateQuantor == null)
			{
				dateQuantor = GetLanguageResource("DateQuantor").Replace(" ", string.Empty).Split(',');
			}
			return dateQuantor;
		}
	}

	public FormatSettings()
	{
		DateStdFormatStrings = new string[10];
		TimeStdFormatStrings = new string[10];
		DatetimeStdFormatStrings = new string[10];
	}

	private string GetLanguageResource(string name)
	{
		string text = Lang.ResourceManager.GetString(name, ServerCultureInfo);
		if (text != null)
		{
			return text;
		}
		return Lang.ResourceManager.GetString(name);
	}

	private static int GetDateCenturyRange()
	{
		int value = ServerUserSettings.Instance.CalendarTwoDigitYearMax.Value;
		int num = ((value < 0) ? DefaultTwoDigitYearMax : value);
		num -= 99;
		if (num < 1 || num > 9999)
		{
			throw new NavNCLOutsidePermittedRangeException("DefaultCentury", 3, num, 1, 9999);
		}
		return num;
	}

	internal static FormatSettings Create(int lcid, ClientSettings clientSettings)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001UO", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "CreateFormatSettings session:{0}, lcid:{1}, DecimalSeparator:'{2}', GroupSeparator:'{3}'", NavCurrentThread.Session?.Id, lcid, clientSettings.DecimalSeparator, clientSettings.GroupSeparator);
		clientSettings.LCID = lcid;
		FormatSettings formatSettings = new FormatSettings();
		formatSettings.ServerCultureInfo = CultureInfo.GetCultureInfo(lcid);
		formatSettings.ClosingDateCharacter = formatSettings.GetLanguageResource("ClosingDate_Character");
		formatSettings.WorkDate = formatSettings.GetLanguageResource("Workdate");
		formatSettings.ToDay = formatSettings.GetLanguageResource("Today");
		formatSettings.Time = formatSettings.GetLanguageResource("Time");
		formatSettings.DateRangeCentury = GetDateCenturyRange();
		formatSettings.FieldFormatComma = FirstCharOrZero(clientSettings.DecimalSeparator);
		formatSettings.FieldFormatThousand = FirstCharOrZero(clientSettings.GroupSeparator);
		formatSettings.DateTextEvalOrder = DateTimeHelper.GetDayMonthYearOrder(clientSettings.ShortDatePattern);
		formatSettings.DateSeparator = clientSettings.DateSeparator;
		formatSettings.TimeSeparator = clientSettings.TimeSeparator;
		formatSettings.DecimalSeparator = clientSettings.DecimalSeparator;
		formatSettings.ShortDatePattern = GetShortDatePattern(clientSettings);
		formatSettings.ShortTimePattern = clientSettings.ShortTimeFormat;
		formatSettings.LongTimePattern = clientSettings.LongTimePattern;
		formatSettings.DateUpdateFmt(clientSettings);
		formatSettings.TimeUpdateFmt(clientSettings);
		formatSettings.DatetimeUpdateFmt(clientSettings);
		formatSettings.durationUnitsInitialized = false;
		formatSettings.durationUnits = null;
		formatSettings.dateQuantor = null;
		formatSettings.BooleanOptions = formatSettings.GetLanguageResource("BooleanTypeOptions").Split(',');
		return formatSettings;
	}

	internal string GetDurationUnit(DurationComponent durationComponent)
	{
		if (durationUnits == null)
		{
			durationUnits = new string[10];
		}
		string text = durationUnits[(int)durationComponent];
		if (text == null)
		{
			text = durationComponent switch
			{
				DurationComponent.Day => GetLanguageResource("day"), 
				DurationComponent.Plural => GetLanguageResource("days"), 
				DurationComponent.Hour => GetLanguageResource("hour"), 
				DurationComponent.Hour | DurationComponent.Plural => GetLanguageResource("hours"), 
				DurationComponent.Minute => GetLanguageResource("minute"), 
				DurationComponent.Minute | DurationComponent.Plural => GetLanguageResource("minutes"), 
				DurationComponent.Second => GetLanguageResource("second"), 
				DurationComponent.Second | DurationComponent.Plural => GetLanguageResource("seconds"), 
				DurationComponent.Millisecond => GetLanguageResource("millisecond"), 
				DurationComponent.Millisecond | DurationComponent.Plural => GetLanguageResource("milliseconds"), 
				_ => throw new NotSupportedException(), 
			};
			durationUnits[(int)durationComponent] = text;
		}
		return text;
	}

	internal string[] GetDurationUnits()
	{
		if (!durationUnitsInitialized)
		{
			GetDurationUnit(DurationComponent.Day);
			GetDurationUnit(DurationComponent.Plural);
			GetDurationUnit(DurationComponent.Hour);
			GetDurationUnit(DurationComponent.Hour | DurationComponent.Plural);
			GetDurationUnit(DurationComponent.Minute);
			GetDurationUnit(DurationComponent.Minute | DurationComponent.Plural);
			GetDurationUnit(DurationComponent.Second);
			GetDurationUnit(DurationComponent.Second | DurationComponent.Plural);
			GetDurationUnit(DurationComponent.Millisecond);
			GetDurationUnit(DurationComponent.Millisecond | DurationComponent.Plural);
			durationUnitsInitialized = true;
		}
		return durationUnits;
	}

	private void DateUpdateFmt(ClientSettings clientSettings)
	{
		char c = (string.IsNullOrEmpty(clientSettings.DateSeparator) ? ' ' : clientSettings.DateSeparator[0]);
		SetDMYFormat(DateTextEvalOrder[0], out var fmt, out var fmt2, '\u0004', '\u0002', '\a', '\u0002');
		SetDMYFormat(DateTextEvalOrder[1], out var fmt3, out var fmt4, '\u0004', '\u0002', '\a', '\u0002');
		SetDMYFormat(DateTextEvalOrder[2], out var fmt5, out var fmt6, '\u0004', '\u0002', '\a', '\u0002');
		DateStdFormatStrings[0] = BuildFormat('\u000e', '\0', fmt, fmt2, c, fmt3, fmt4, c, fmt5, fmt6);
		DateStdFormatStrings[1] = BuildFormat('\u000e', '\0', fmt, fmt2, c, fmt3, fmt4, c, fmt5, fmt6);
		DateStdFormatStrings[2] = BuildFormat(fmt, fmt2, fmt3, fmt4, fmt5, fmt6, '\u000e', '\0', 'D');
		string[] dateStdFormatStrings = DateStdFormatStrings;
		char[] array = new char[10] { '\u000e', '\0', '\a', '\0', '\0', '\u0004', '\u0002', '\0', '\u0003', '\u0002' };
		array[4] = c;
		array[7] = c;
		dateStdFormatStrings[3] = BuildFormat(array);
		DateStdFormatStrings[5] = BuildFormat('\u000e', '\0', fmt, fmt2, fmt3, fmt4, fmt5, fmt6);
		DateStdFormatStrings[6] = BuildFormat('\u000e', '\0', '\a', '\0', '\u0004', '\u0002', '\u0003', '\u0002');
		SetDMYFormat(DateTextEvalOrder[0], out fmt, out fmt2, '\u0005', '\0', '\b', '\0');
		SetDMYFormat(DateTextEvalOrder[1], out fmt3, out fmt4, '\u0005', '\0', '\b', '\0');
		SetDMYFormat(DateTextEvalOrder[2], out fmt5, out fmt6, '\u0005', '\0', '\b', '\0');
		if (DateTextEvalOrder[2] == 'D')
		{
			string[] dateStdFormatStrings2 = DateStdFormatStrings;
			char[] array2 = new char[11]
			{
				'\u000e', '\0', '\0', '\0', ' ', '\0', '\0', ' ', '\0', '\0',
				'.'
			};
			array2[2] = fmt;
			array2[3] = fmt2;
			array2[5] = fmt3;
			array2[6] = fmt4;
			array2[8] = fmt5;
			array2[9] = fmt6;
			dateStdFormatStrings2[4] = BuildFormat(array2);
		}
		else if (DateTextEvalOrder[1] == 'D')
		{
			string[] dateStdFormatStrings3 = DateStdFormatStrings;
			char[] array3 = new char[11]
			{
				'\0', '\0', ' ', '\u000e', '\0', '\0', '\0', ',', ' ', '\0',
				'\0'
			};
			array3[0] = fmt;
			array3[1] = fmt2;
			array3[5] = fmt3;
			array3[6] = fmt4;
			array3[9] = fmt5;
			array3[10] = fmt6;
			dateStdFormatStrings3[4] = BuildFormat(array3);
		}
		else
		{
			string[] dateStdFormatStrings4 = DateStdFormatStrings;
			char[] array4 = new char[11]
			{
				'\u000e', '\0', '\0', '\0', '.', ' ', '\0', '\0', ' ', '\0',
				'\0'
			};
			array4[2] = fmt;
			array4[3] = fmt2;
			array4[6] = fmt3;
			array4[7] = fmt4;
			array4[9] = fmt5;
			array4[10] = fmt6;
			dateStdFormatStrings4[4] = BuildFormat(array4);
		}
		DateStdFormatStrings[7] = BuildFormat('\u0003', '\u0002', '\u001c', ' ', '.', ' ', '\u0005', '\u0003', ' ', '\b', '\0');
		DateStdFormatStrings[9] = BuildFormat('\b', '\0', '-', '\u0004', '\u0002', '-', '\u0003', '\u0002');
	}

	private void TimeUpdateFmt(ClientSettings clientSettings)
	{
		bool flag = !string.IsNullOrEmpty(clientSettings.ShortTimeFormat) && clientSettings.ShortTimeFormat.IndexOf('H') != -1;
		bool flag2 = !string.IsNullOrEmpty(clientSettings.ShortTimeZero) && clientSettings.ShortTimeZero.IndexOf("HH", StringComparison.OrdinalIgnoreCase) != -1;
		char c = (string.IsNullOrEmpty(clientSettings.TimeSeparator) ? ' ' : clientSettings.TimeSeparator[0]);
		char c2;
		if (flag2)
		{
			c2 = (flag ? '\n' : '\v');
		}
		else
		{
			c2 = (flag ? '\u0003' : '\u0004');
		}
		TimeAMDesignator = clientSettings.TimeAMDesignator;
		TimePMDesignator = clientSettings.TimePMDesignator;
		if (flag)
		{
			string[] timeStdFormatStrings = TimeStdFormatStrings;
			char[] array = new char[8] { '\0', '\u0002', '\0', '\u0005', '\u0002', '\0', '\u0006', '\u0002' };
			array[0] = c2;
			array[2] = c;
			array[5] = c;
			timeStdFormatStrings[0] = BuildFormat(array);
			string[] timeStdFormatStrings2 = TimeStdFormatStrings;
			char[] array2 = new char[10] { '\0', '\u0002', '\0', '\u0005', '\u0002', '\0', '\u0006', '\u0002', '\t', '\0' };
			array2[0] = c2;
			array2[2] = c;
			array2[5] = c;
			timeStdFormatStrings2[1] = BuildFormat(array2);
		}
		else
		{
			string[] timeStdFormatStrings3 = TimeStdFormatStrings;
			char[] array3 = new char[11]
			{
				'\0', '\u0002', '\0', '\u0005', '\u0002', '\0', '\u0006', '\u0002', ' ', '\b',
				'\0'
			};
			array3[0] = c2;
			array3[2] = c;
			array3[5] = c;
			timeStdFormatStrings3[0] = BuildFormat(array3);
			string[] timeStdFormatStrings4 = TimeStdFormatStrings;
			char[] array4 = new char[13]
			{
				'\0', '\u0002', '\0', '\u0005', '\u0002', '\0', '\u0006', '\u0002', '\t', '\0',
				' ', '\b', '\0'
			};
			array4[0] = c2;
			array4[2] = c;
			array4[5] = c;
			timeStdFormatStrings4[1] = BuildFormat(array4);
		}
		TimeStdFormatStrings[2] = BuildFormat('\u0003', '\u0002', '\u001c', '0', '\u0005', '\u0002', '\u0006', '\u0002', '\t', '\0', '\u001a', '.', 'T');
		TimeStdFormatStrings[9] = BuildFormat('\u0003', '\u0002', '\u001c', '0', ':', '\u0005', '\u0002', ':', '\u0006', '\u0002', '\t', '\0', '\u001a', '.');
	}

	private void DatetimeUpdateFmt(ClientSettings clientSettings)
	{
		char c = (string.IsNullOrEmpty(clientSettings.DateSeparator) ? ' ' : clientSettings.DateSeparator[0]);
		SetDMYFormat(DateTextEvalOrder[0], out var fmt, out var fmt2, '\u0004', '\u0002', '\a', '\u0002');
		SetDMYFormat(DateTextEvalOrder[1], out var fmt3, out var fmt4, '\u0004', '\u0002', '\a', '\u0002');
		SetDMYFormat(DateTextEvalOrder[2], out var fmt5, out var fmt6, '\u0004', '\u0002', '\a', '\u0002');
		bool flag = !string.IsNullOrEmpty(clientSettings.ShortTimeFormat) && clientSettings.ShortTimeFormat.IndexOf('H') != -1;
		char c2 = (string.IsNullOrEmpty(clientSettings.TimeSeparator) ? ' ' : clientSettings.TimeSeparator[0]);
		char c3 = (flag ? '\u000e' : '\u000f');
		TimeAMDesignator = clientSettings.TimeAMDesignator;
		TimePMDesignator = clientSettings.TimePMDesignator;
		if (flag)
		{
			string[] datetimeStdFormatStrings = DatetimeStdFormatStrings;
			char[] array = new char[14]
			{
				'\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', ' ', '\0',
				'\u0002', '\0', '\u0010', '\u0002'
			};
			array[0] = fmt;
			array[1] = fmt2;
			array[2] = c;
			array[3] = fmt3;
			array[4] = fmt4;
			array[5] = c;
			array[6] = fmt5;
			array[7] = fmt6;
			array[9] = c3;
			array[11] = c2;
			datetimeStdFormatStrings[0] = BuildFormat(array);
			string[] datetimeStdFormatStrings2 = DatetimeStdFormatStrings;
			char[] array2 = new char[19]
			{
				'\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', ' ', '\0',
				'\u0002', '\0', '\u0010', '\u0002', '\0', '\u0011', '\u0002', '\u0014', '\0'
			};
			array2[0] = fmt;
			array2[1] = fmt2;
			array2[2] = c;
			array2[3] = fmt3;
			array2[4] = fmt4;
			array2[5] = c;
			array2[6] = fmt5;
			array2[7] = fmt6;
			array2[9] = c3;
			array2[11] = c2;
			array2[14] = c2;
			datetimeStdFormatStrings2[1] = BuildFormat(array2);
			string[] datetimeStdFormatStrings3 = DatetimeStdFormatStrings;
			char[] array3 = new char[14]
			{
				'\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', ' ', '\0',
				'\u0002', '\0', '\u0010', '\u0002'
			};
			array3[0] = fmt;
			array3[1] = fmt2;
			array3[2] = c;
			array3[3] = fmt3;
			array3[4] = fmt4;
			array3[5] = c;
			array3[6] = fmt5;
			array3[7] = fmt6;
			array3[9] = c3;
			array3[11] = c2;
			datetimeStdFormatStrings3[2] = BuildFormat(array3);
			string[] datetimeStdFormatStrings4 = DatetimeStdFormatStrings;
			char[] array4 = new char[17]
			{
				'\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', ' ', '\0',
				'\u0002', '\0', '\u0010', '\u0002', '\0', '\u0011', '\u0002'
			};
			array4[0] = fmt;
			array4[1] = fmt2;
			array4[2] = c;
			array4[3] = fmt3;
			array4[4] = fmt4;
			array4[5] = c;
			array4[6] = fmt5;
			array4[7] = fmt6;
			array4[9] = c3;
			array4[11] = c2;
			array4[14] = c2;
			datetimeStdFormatStrings4[3] = BuildFormat(array4);
		}
		else
		{
			string[] datetimeStdFormatStrings5 = DatetimeStdFormatStrings;
			char[] array5 = new char[17]
			{
				'\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', ' ', '\0',
				'\u0002', '\0', '\u0010', '\u0002', ' ', '\u0013', '\0'
			};
			array5[0] = fmt;
			array5[1] = fmt2;
			array5[2] = c;
			array5[3] = fmt3;
			array5[4] = fmt4;
			array5[5] = c;
			array5[6] = fmt5;
			array5[7] = fmt6;
			array5[9] = c3;
			array5[11] = c2;
			datetimeStdFormatStrings5[0] = BuildFormat(array5);
			string[] datetimeStdFormatStrings6 = DatetimeStdFormatStrings;
			char[] array6 = new char[22]
			{
				'\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', ' ', '\0',
				'\u0002', '\0', '\u0010', '\u0002', '\0', '\u0011', '\u0002', '\u0014', '\0', ' ',
				'\u0013', '\0'
			};
			array6[0] = fmt;
			array6[1] = fmt2;
			array6[2] = c;
			array6[3] = fmt3;
			array6[4] = fmt4;
			array6[5] = c;
			array6[6] = fmt5;
			array6[7] = fmt6;
			array6[9] = c3;
			array6[11] = c2;
			array6[14] = c2;
			datetimeStdFormatStrings6[1] = BuildFormat(array6);
			string[] datetimeStdFormatStrings7 = DatetimeStdFormatStrings;
			char[] array7 = new char[17]
			{
				'\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', ' ', '\0',
				'\u0002', '\0', '\u0010', '\u0002', ' ', '\u0013', '\0'
			};
			array7[0] = fmt;
			array7[1] = fmt2;
			array7[2] = c;
			array7[3] = fmt3;
			array7[4] = fmt4;
			array7[5] = c;
			array7[6] = fmt5;
			array7[7] = fmt6;
			array7[9] = c3;
			array7[11] = c2;
			datetimeStdFormatStrings7[2] = BuildFormat(array7);
			string[] datetimeStdFormatStrings8 = DatetimeStdFormatStrings;
			char[] array8 = new char[20]
			{
				'\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', ' ', '\0',
				'\u0002', '\0', '\u0010', '\u0002', '\0', '\u0011', '\u0002', ' ', '\u0013', '\0'
			};
			array8[0] = fmt;
			array8[1] = fmt2;
			array8[2] = c;
			array8[3] = fmt3;
			array8[4] = fmt4;
			array8[5] = c;
			array8[6] = fmt5;
			array8[7] = fmt6;
			array8[9] = c3;
			array8[11] = c2;
			array8[14] = c2;
			datetimeStdFormatStrings8[3] = BuildFormat(array8);
		}
		DatetimeStdFormatStrings[9] = BuildFormat('\b', '\0', '-', '\u0004', '\u0002', '-', '\u0003', '\u0002', 'T', '\u000e', '\u0002', ':', '\u0010', '\u0002', ':', '\u0011', '\u0002', '\u0014', '\0', '\u001a', '.', 'Z');
	}

	private static string BuildFormat(params char[] formatArgs)
	{
		return new string(formatArgs);
	}

	private static string[] BuildGenericStdFormatStrings()
	{
		string[] array = new string[10];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = BuildFormat('\0', '\0', (char)i);
		}
		return array;
	}

	private static string[] BuildDecimalStdFormatStrings()
	{
		return new string[10]
		{
			BuildFormat('\u0003', '\0', '\u0006', '\0', '\u0005', '\0'),
			BuildFormat('\u0003', '\0', '\u0004', '\0', '\u0005', '\0'),
			BuildFormat('\u0003', '\0', '\u0004', '\0', '\u0005', '\0', '\u001a', '.'),
			BuildFormat('\u0006', '\0', '\u0005', '\0', '\u0003', '\u0001'),
			BuildFormat('\u0004', '\0', '\u0005', '\0', '\u0003', '\u0001'),
			null,
			null,
			null,
			null,
			BuildFormat('\u0003', '\0', '\u0004', '\0', '\u0005', '\0', '\u001a', '.')
		};
	}

	private static string[] BuildOptionStdFormatStrings()
	{
		return new string[10]
		{
			BuildFormat('\u0003', '\0'),
			BuildFormat('\u0003', '\0'),
			BuildFormat('\u0004', '\0'),
			null,
			null,
			null,
			null,
			null,
			null,
			BuildFormat('\u0005', '\0')
		};
	}

	private static void SetDMYFormat(char dmy, out char fmt1, out char fmt2, char month0, char month1, char year0, char day1)
	{
		switch (dmy)
		{
		case 'D':
			fmt1 = '\u0003';
			fmt2 = day1;
			break;
		case 'Y':
			fmt1 = year0;
			fmt2 = '\0';
			break;
		case 'M':
			fmt1 = month0;
			fmt2 = month1;
			break;
		default:
			throw new NavNCLArgumentException("dmy");
		}
	}

	private static char FirstCharOrZero(string clientProperty)
	{
		if (!string.IsNullOrEmpty(clientProperty))
		{
			return clientProperty[0];
		}
		return '\0';
	}

	private static string GetShortDatePattern(ClientSettings clientSettings)
	{
		if (!string.IsNullOrEmpty(clientSettings.ShortDatePattern))
		{
			return clientSettings.ShortDatePattern;
		}
		int culture = ((clientSettings.WindowsLCID > 0) ? clientSettings.WindowsLCID : clientSettings.LCID);
		CultureInfo cultureInfo = CultureInfo.GetCultureInfo(culture);
		return cultureInfo.DateTimeFormat.ShortDatePattern;
	}
}
