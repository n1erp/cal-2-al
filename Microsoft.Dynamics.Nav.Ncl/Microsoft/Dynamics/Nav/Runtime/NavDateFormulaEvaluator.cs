using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDateFormulaEvaluator : NavValueEvaluator<NavDateFormula>
{
	private sealed class InternalEvaluator
	{
		private enum TokenType
		{
			Sign,
			Number,
			PeriodQuantor,
			CurrentQuantor,
			EOS
		}

		private static readonly int[] NQrange = new int[6] { 1, 1, 7, 1, 3, 12 };

		private static readonly int[] LowBound = new int[6] { 1, 1, 1, 1, 1, 0 };

		private static readonly int[] HighBound = new int[6] { 31, 7, 53, 12, 4, 99 };

		private string source;

		private string[] dateQuantor;

		private int sourcePos;

		private TokenType tokenType;

		private int tokenValue;

		private const int Undefined = -1;

		private const int DateYearMin = 0;

		private const int DateYearMax = 9999;

		private NavDate inputDate;

		private DateTime calcDate;

		private StringBuilder result;

		private string DisplaySource => NavDateFormulaFormatter.Format(source, NavCurrentSessionWrapper.FormatSettings, 0);

		internal InternalEvaluator(string value)
		{
			source = value;
		}

		private static bool IsInvariant(string value)
		{
			if (value.Length > 1 && value[0] == '<')
			{
				return value[value.Length - 1] == '>';
			}
			return false;
		}

		private void NextToken()
		{
			int num = 0;
			int num2 = 0;
			int num3 = sourcePos;
			while (sourcePos < source.Length)
			{
				char c = source[sourcePos];
				if ((c >= 'A' || c < ' ') && c != '\r' && c != '\n' && c != '\t')
				{
					int i;
					if (c >= 'A')
					{
						while (sourcePos < source.Length && source[sourcePos] >= 'A')
						{
							sourcePos++;
							num2++;
						}
						num = dateQuantor[0].Length;
						if (string.CompareOrdinal(dateQuantor[0], 0, source, num3, num) != 0)
						{
							num = 0;
						}
						else
						{
							result.Append('\u0001');
						}
						for (i = 2; i <= 7; i++)
						{
							if (dateQuantor[i - 1].Length == num2 - num && string.CompareOrdinal(dateQuantor[i - 1], 0, source, num3 + num, num2 - num) == 0)
							{
								result.Append((char)i);
								break;
							}
						}
					}
					else
					{
						if (c == '\u0001')
						{
							num = 1;
							sourcePos++;
							result.Append(c);
						}
						result.Append(i = source[sourcePos++]);
					}
					tokenValue = ((i > 7) ? (-1) : i);
					tokenType = ((num == 0) ? TokenType.PeriodQuantor : TokenType.CurrentQuantor);
					return;
				}
				if (char.IsDigit(c))
				{
					result.Append(c);
					int num4 = c - 48;
					while (++sourcePos < source.Length && char.IsDigit(source[sourcePos]))
					{
						c = source[sourcePos];
						result.Append(c);
						num4 = num4 * 10 + c - 48;
						if (num4 > 9999)
						{
							num4 = -1;
							break;
						}
					}
					tokenType = TokenType.Number;
					tokenValue = num4;
					return;
				}
				if (c == '+' || c == '-')
				{
					result.Append(c);
					tokenType = TokenType.Sign;
					tokenValue = c;
					sourcePos++;
					return;
				}
				sourcePos++;
				num3++;
			}
			tokenType = TokenType.EOS;
			tokenValue = -1;
		}

		private void ExprList()
		{
			while (tokenType != TokenType.EOS)
			{
				int num = tokenValue;
				if (!Match(TokenType.Sign))
				{
					throw NavNCLEvaluateException.CreateDateFormulaSignMissing(CultureInfo.CurrentCulture, DisplaySource);
				}
				Term((char)num);
				ExprList();
			}
		}

		private bool Match(TokenType token)
		{
			if (token != tokenType || tokenValue == -1)
			{
				return false;
			}
			NextToken();
			return true;
		}

		private void NumberQuantorEvaluate(int sign, int number, int quantor)
		{
			if (inputDate != null)
			{
				number *= NQrange[quantor - 2];
				if (sign == 45)
				{
					number = -number;
				}
				if (quantor <= 4)
				{
					calcDate = calcDate.AddDays(number);
				}
				else
				{
					calcDate = calcDate.AddMonths(number);
				}
			}
		}

		private void QuantorNumberEvaluate(int sign, int quantor, int number)
		{
			if (number < LowBound[quantor - 2] || number > HighBound[quantor - 2])
			{
				throw NavNCLEvaluateException.CreateDateFormulaNumberOutOfRange(CultureInfo.CurrentCulture, DisplaySource, LowBound[quantor - 2], HighBound[quantor - 2]);
			}
			if (inputDate == null)
			{
				return;
			}
			int day = calcDate.Day;
			int month = calcDate.Month;
			int year = calcDate.Year;
			int months = 0;
			switch (quantor)
			{
			case 2:
				if (sign == 43)
				{
					if (day >= number)
					{
						months = 1;
					}
				}
				else if (day <= number)
				{
					months = -1;
				}
				calcDate = new DateTime(year, month, 1).AddMonths(months);
				number = Math.Min(DateTime.DaysInMonth(calcDate.Year, calcDate.Month), number);
				calcDate = new DateTime(calcDate.Year, calcDate.Month, number);
				break;
			case 3:
				months = number - DateTimeHelper.DayOfWeekToNavDayNo(calcDate.DayOfWeek);
				if (sign == 43)
				{
					if (months <= 0)
					{
						months += 7;
					}
				}
				else if (months >= 0)
				{
					months -= 7;
				}
				calcDate = calcDate.AddDays(months);
				break;
			case 4:
			{
				DateTimeHelper.DateTimeToIsoYearWeek(calcDate, out year, out var weekNo);
				if (sign == 43)
				{
					if (weekNo >= number)
					{
						year++;
					}
				}
				else if (weekNo <= number)
				{
					year--;
				}
				if (year < 0 || year > 9999)
				{
					throw new NavNCLDateInvalidException(string.Format(CultureInfo.CurrentCulture, Lang.CalcDateResultOverflow, DisplaySource));
				}
				number = Math.Max(1, Math.Min(number, DateTimeHelper.MaxWeekNoOfYear(year)));
				if (year == 0 && number == 1)
				{
					throw new NavNCLDateInvalidException(string.Format(CultureInfo.CurrentCulture, Lang.CalcDateResultOverflow, source));
				}
				calcDate = DateTimeHelper.IsoYearWeekToDateTime(year, number);
				break;
			}
			case 5:
				months = number - month;
				if (sign == 43)
				{
					if (months <= 0)
					{
						months += 12;
					}
				}
				else if (months >= 0)
				{
					months -= 12;
				}
				calcDate = new DateTime(year, month, 1).AddMonths(months);
				break;
			case 6:
				months = 3 * (number - 1) - month + 1;
				if (sign == 43)
				{
					if (months <= 0)
					{
						months += 12;
					}
				}
				else if (months >= 0)
				{
					months -= 12;
				}
				calcDate = new DateTime(year, month, 1).AddMonths(months);
				break;
			case 7:
				number -= year % 100;
				if (number <= 0)
				{
					number += 100;
				}
				calcDate = new DateTime(year, 1, 1).AddYears(number);
				break;
			}
		}

		private void CurrentQuantorEvaluate(int sign, int quantor)
		{
			if (inputDate == null)
			{
				return;
			}
			int day = calcDate.Day;
			int num = calcDate.Month;
			int year = calcDate.Year;
			switch (quantor)
			{
			case 4:
			{
				int num2 = -(DateTimeHelper.DayOfWeekToNavDayNo(calcDate.DayOfWeek) - 1);
				if (sign == 43)
				{
					num2 += 6;
				}
				calcDate = calcDate.AddDays(num2);
				return;
			}
			case 5:
				day = ((sign != 43) ? 1 : DateTime.DaysInMonth(year, num));
				break;
			case 6:
			{
				int num3 = (num - 1) / 3;
				if (sign == 43)
				{
					num = num3 * 3 + 3;
					day = DateTime.DaysInMonth(year, num);
				}
				else
				{
					num = num3 * 3 + 1;
					day = 1;
				}
				break;
			}
			case 7:
				if (sign == 43)
				{
					num = 12;
					day = 31;
				}
				else
				{
					num = 1;
					day = 1;
				}
				break;
			}
			calcDate = new DateTime(year, num, day);
		}

		private void Term(char sign)
		{
			try
			{
				int num = tokenValue;
				switch (tokenType)
				{
				case TokenType.Number:
				{
					if (!Match(TokenType.Number))
					{
						throw NavNCLEvaluateException.CreateDateFormulaNumberOutOfRange(CultureInfo.CurrentCulture, DisplaySource, 0, 9999);
					}
					int number = tokenValue;
					if (!Match(TokenType.PeriodQuantor))
					{
						throw NavNCLEvaluateException.CreateDateFormulaShouldIncludeQuantor(CultureInfo.CurrentCulture, DisplaySource);
					}
					NumberQuantorEvaluate(sign, num, number);
					break;
				}
				case TokenType.PeriodQuantor:
				{
					if (!Match(TokenType.PeriodQuantor))
					{
						throw NavNCLEvaluateException.CreateDateFormulaShouldIncludeQuantor(CultureInfo.CurrentCulture, DisplaySource);
					}
					int number = tokenValue;
					if (!Match(TokenType.Number))
					{
						throw NavNCLEvaluateException.CreateDateFormulaShouldIncludeANumber(CultureInfo.CurrentCulture, DisplaySource);
					}
					QuantorNumberEvaluate(sign, num, number);
					break;
				}
				default:
					if (!Match(TokenType.CurrentQuantor))
					{
						throw NavNCLEvaluateException.CreateDateFormulaShouldIncludeANumber(CultureInfo.CurrentCulture, DisplaySource);
					}
					CurrentQuantorEvaluate(sign, num);
					break;
				}
				if (inputDate != null && calcDate < NavDateTimeHelper.DateTimeMinimum)
				{
					throw new NavNCLDateInvalidException();
				}
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new NavNCLDateInvalidException();
			}
		}

		private void Statement()
		{
			if (tokenType != TokenType.Sign && tokenType != TokenType.EOS)
			{
				Term('+');
			}
			ExprList();
		}

		internal string Parse()
		{
			return Parse(0);
		}

		internal string Parse(int formatNumber)
		{
			if (source.Length > 32)
			{
				throw NavNCLEvaluateException.CreateDateFormulaExceedMaxLength(CultureInfo.CurrentCulture, DisplaySource, 32);
			}
			source = source.ToUpper(CultureInfo.CurrentCulture);
			result = new StringBuilder();
			bool flag = IsInvariant(source);
			if (flag)
			{
				source = source.Substring(1, source.Length - 2);
			}
			dateQuantor = ((flag || formatNumber == 9) ? FormatSettings.GlobalDateQuantor : NavCurrentSessionWrapper.FormatSettings.DateQuantor);
			sourcePos = 0;
			NextToken();
			Statement();
			return result.ToString();
		}

		internal NavDate CalcDate(NavDate date)
		{
			if (date == NavDate.Default)
			{
				throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.CalcDateOnUndefinedDate, DisplaySource));
			}
			inputDate = date;
			calcDate = date.Value;
			Parse();
			return NavDate.CreateDate(calcDate, NavDate.IsClosingDate(inputDate));
		}
	}

	private static readonly NavDateFormulaEvaluator instance = new NavDateFormulaEvaluator();

	internal static NavDateFormulaEvaluator Instance => instance;

	private NavDateFormulaEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavDateFormula value, INavValueMetadata metadata, string source, int number)
	{
		value = NavDateFormula.Create(Parse(source, number), isTokenString: true);
		return true;
	}

	protected override NavDateFormula GetDefaultValue(INavValueMetadata metadata)
	{
		return NavDateFormula.Default;
	}

	internal static string Parse(string value, int formatNumber)
	{
		return new InternalEvaluator(value).Parse(formatNumber);
	}

	internal static NavDate CalcDate(string value, NavDate dateValue)
	{
		return new InternalEvaluator(value).CalcDate(dateValue);
	}
}
