using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal class DateFormulaEvaluator
{
	private static class DateQuantor
	{
		internal const byte Current = 1;

		internal const byte Day = 2;

		internal const byte WeekDay = 3;

		internal const byte Week = 4;

		internal const byte Month = 5;

		internal const byte Quarter = 6;

		internal const byte Year = 7;
	}

	private enum TokenType
	{
		Sign,
		Number,
		PeriodQuantor,
		CurrentQuantor,
		EOS
	}

	[SuppressMessage("Nav.Reliability", "DN0005:DontDeriveFromException", MessageId = "DN0005")]
	[SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	[SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	private class ErrorCodeDiagnosticsException : Exception
	{
		public ErrorCode ErrorCode { get; }

		public object[] Arguments { get; }

		public ErrorCodeDiagnosticsException(ErrorCode errorCode, params object[] arguments)
		{
			ErrorCode = errorCode;
			Arguments = arguments;
		}
	}

	private const int MaxCharSize = 32;

	private const int Undefined = -1;

	private const int DateYearMin = 0;

	private const int DateYearMax = 9999;

	private static readonly int[] LowBound = new int[6] { 1, 1, 1, 1, 1, 0 };

	private static readonly int[] HighBound = new int[6] { 31, 7, 53, 12, 4, 99 };

	private string[] dateQuantor;

	private StringBuilder result;

	private string source;

	private int sourcePos;

	private TokenType tokenType;

	private int tokenValue;

	private static string[] GlobalDateQuantor { get; } = new string[7] { "C", "D", "WD", "W", "M", "Q", "Y" };

	private string DisplaySource => FormatDateFormula(source);

	public DateFormulaEvaluator(string source)
	{
		this.source = source;
	}

	internal NavDiagnosticInfo Parse()
	{
		try
		{
			if (source.Length > 32)
			{
				return new NavDiagnosticInfo(ErrorCode.ERR_DateFormulaValueTooLong, 32);
			}
			source = source.ToUpperInvariant();
			result = new StringBuilder();
			if (IsInvariant(source))
			{
				source = source.Substring(1, source.Length - 2);
			}
			dateQuantor = GlobalDateQuantor;
			sourcePos = 0;
			NextToken();
			Statement();
		}
		catch (ErrorCodeDiagnosticsException ex)
		{
			return new NavDiagnosticInfo(ex.ErrorCode, ex.Arguments);
		}
		return null;
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

	private void Statement()
	{
		if (tokenType != TokenType.Sign && tokenType != TokenType.EOS)
		{
			Term();
		}
		ExprList();
	}

	private void ExprList()
	{
		while (tokenType != TokenType.EOS)
		{
			if (!Match(TokenType.Sign))
			{
				throw new ErrorCodeDiagnosticsException(ErrorCode.ERR_DateFormulaSignMissing, DisplaySource);
			}
			Term();
			ExprList();
		}
	}

	private void Term()
	{
		int quantor = tokenValue;
		switch (tokenType)
		{
		case TokenType.Number:
			if (!Match(TokenType.Number))
			{
				throw new ErrorCodeDiagnosticsException(ErrorCode.ERR_DateFormulaNumberOutOfRange, DisplaySource, 0, 9999);
			}
			_ = tokenValue;
			if (!Match(TokenType.PeriodQuantor))
			{
				throw new ErrorCodeDiagnosticsException(ErrorCode.ERR_DateFormulaShouldIncludeQuantor, DisplaySource);
			}
			break;
		case TokenType.PeriodQuantor:
		{
			if (!Match(TokenType.PeriodQuantor))
			{
				throw new ErrorCodeDiagnosticsException(ErrorCode.ERR_DateFormulaShouldIncludeQuantor, DisplaySource);
			}
			int number = tokenValue;
			if (!Match(TokenType.Number))
			{
				throw new ErrorCodeDiagnosticsException(ErrorCode.ERR_DateFormulaShouldIncludeANumber, DisplaySource);
			}
			QuantorNumberEvaluate(quantor, number);
			break;
		}
		default:
			if (!Match(TokenType.CurrentQuantor))
			{
				throw new ErrorCodeDiagnosticsException(ErrorCode.ERR_DateFormulaShouldIncludeANumber, DisplaySource);
			}
			break;
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	private void QuantorNumberEvaluate(int quantor, int number)
	{
		if (number < LowBound[quantor - 2] || number > HighBound[quantor - 2])
		{
			throw new ErrorCodeDiagnosticsException(ErrorCode.ERR_DateFormulaNumberOutOfRange, DisplaySource, LowBound[quantor - 2], HighBound[quantor - 2]);
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

	private static string FormatDateFormula(string dateFormula)
	{
		string[] globalDateQuantor = GlobalDateQuantor;
		StringBuilder stringBuilder = new StringBuilder(dateFormula.Length + 10);
		foreach (char c in dateFormula)
		{
			if (c < '\u0014')
			{
				stringBuilder.Append(globalDateQuantor[c - 1]);
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}
}
