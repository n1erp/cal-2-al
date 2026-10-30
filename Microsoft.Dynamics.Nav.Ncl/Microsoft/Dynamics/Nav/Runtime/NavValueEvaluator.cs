using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class NavValueEvaluator
{
	internal abstract bool Evaluate(DataError dataError, out NavValue value, INavValueMetadata metadata, string source, int number);

	internal static NavValueEvaluator GetEvaluator(NavNclType type)
	{
		return type switch
		{
			NavNclType.NavBigInteger => (NavValueEvaluator)NavBigIntegerEvaluator.Instance, 
			NavNclType.NavBigText => NavBigTextEvaluator.Instance, 
			NavNclType.NavBlob => NavBLOBEvaluator.Instance, 
			NavNclType.NavBoolean => NavBooleanEvaluator.Instance, 
			NavNclType.NavByte => NavByteEvaluator.Instance, 
			NavNclType.NavChar => NavCharEvaluator.Instance, 
			NavNclType.NavCode => NavCodeEvaluator.Instance, 
			NavNclType.NavDate => NavDateEvaluator.Instance, 
			NavNclType.NavDateTime => NavDateTimeEvaluator.Instance, 
			NavNclType.NavDateFormula => NavDateFormulaEvaluator.Instance, 
			NavNclType.NavDecimal => NavDecimalEvaluator.Instance, 
			NavNclType.NavDuration => NavDurationEvaluator.Instance, 
			NavNclType.NavGuid => NavGuidEvaluator.Instance, 
			NavNclType.NavInteger => NavIntegerEvaluator.Instance, 
			NavNclType.NavOemCode => NavOemCodeEvaluator.Instance, 
			NavNclType.NavOemText => NavOemTextEvaluator.Instance, 
			NavNclType.NavOption => NavOptionEvaluator.Instance, 
			NavNclType.NavRecordId => NavRecordIdEvaluator.Instance, 
			NavNclType.NavTableFilter => NavTableFilterEvaluator.Instance, 
			NavNclType.NavText => NavTextEvaluator.Instance, 
			NavNclType.NavTextBuilder => NavTextBuilderEvaluator.Instance, 
			NavNclType.NavTime => NavTimeEvaluator.Instance, 
			NavNclType.NavFormResult => NavFormResultEvaluator.Instance, 
			NavNclType.NavSecurityFiltering => NavSecurityFilteringEvaluator.Instance, 
			NavNclType.NavExecutionMode => NavExecutionModeEvaluator.Instance, 
			NavNclType.NavExecutionContext => NavExecutionContextEvaluator.Instance, 
			NavNclType.NavTransactionType => NavTransactionTypeEvaluator.Instance, 
			NavNclType.NavClientType => NavEnumClientTypeEvaluator.Instance, 
			NavNclType.NavObjectType => NavEnumObjectTypeEvaluator.Instance, 
			NavNclType.NavTextEncoding => NavEnumTextEncodingEvaluator.Instance, 
			NavNclType.NavReportFormat => NavEnumReportFormatEvaluator.Instance, 
			NavNclType.NavDefaultLayout => NavEnumDefaultLayoutEvaluator.Instance, 
			NavNclType.NavTableConnectionType => NavTableConnectionTypeEvaluator.Instance, 
			NavNclType.NavMedia => NavMediaEvaluator.Instance, 
			NavNclType.NavMediaSet => NavMediaSetEvaluator.Instance, 
			NavNclType.NavTestPermissions => NavEnumTestPermissionsEvaluator.Instance, 
			NavNclType.NavDataClassification => NavDataClassificationEvaluator.Instance, 
			NavNclType.NavVerbosity => NavVerbosityEvaluator.Instance, 
			NavNclType.NavTestTransactionModel => NavTestTransactionModelEvaluator.Instance, 
			NavNclType.NavNotification => NavNotificationEvaluator.Instance, 
			NavNclType.NavNotificationScope => NavEnumNotificationScopeEnumEvaluator.Instance, 
			NavNclType.NavJsonToken => NavJsonTokenEvaluator<NavJsonToken>.Instance, 
			NavNclType.NavJsonArray => NavJsonTokenEvaluator<NavJsonArray>.Instance, 
			NavNclType.NavJsonObject => NavJsonTokenEvaluator<NavJsonObject>.Instance, 
			NavNclType.NavJsonValue => NavJsonTokenEvaluator<NavJsonValue>.Instance, 
			NavNclType.NavVersion => NavVersionEvaluator.Instance, 
			NavNclType.NavFieldClass => NavFieldClassEvaluator.Instance, 
			NavNclType.NavWebServiceActionResultCode => NavWebServiceActionResultCodeEvaluator.Instance, 
			NavNclType.NavDataScope => NavDataScopeEvaluator.Instance, 
			NavNclType.NavErrorInfo => NavALErrorInfoEvaluator.Instance, 
			NavNclType.NavALErrorType => NavALErrorTypeEvaluator.Instance, 
			_ => null, 
		};
	}

	internal static NumberStyles GetNumberStyles(int number)
	{
		if (number == 9)
		{
			return NumberStyles.Integer | NumberStyles.AllowDecimalPoint;
		}
		return NumberStyles.Any;
	}

	internal static IFormatProvider GetFormatProvider(int number)
	{
		if (number == 9)
		{
			return CultureInfo.InvariantCulture;
		}
		return NavCurrentSessionWrapper.WindowsCulture;
	}

	protected static string RemoveWhiteSpaceUntilFirstDigit(string source)
	{
		StringBuilder stringBuilder = new StringBuilder(source.Length);
		for (int i = 0; i < source.Length; i++)
		{
			char c = source[i];
			if (char.IsDigit(c))
			{
				return stringBuilder.Append(source, i, source.Length - i).ToString();
			}
			if (!char.IsWhiteSpace(c))
			{
				stringBuilder.Append(c);
			}
		}
		return string.Empty;
	}

	protected static bool TryReadTokenUntilDelimiter(string input, ref int position, char delimiter, bool delimiterRequired, out string result)
	{
		ConsumeWhitespace(input, ref position);
		if (position >= input.Length)
		{
			result = string.Empty;
			return !delimiterRequired;
		}
		int num = position++;
		int num2 = -1;
		if (input[num] == delimiter)
		{
			result = string.Empty;
			return true;
		}
		if (input[num] != '"')
		{
			num2 = input.IndexOf(delimiter, position);
			if (num2 < 0)
			{
				if (delimiterRequired)
				{
					result = null;
					return false;
				}
				num2 = input.Length - 1;
				position = input.Length;
			}
			else
			{
				position = num2 + 1;
				num2--;
			}
			while (char.IsWhiteSpace(input, num2))
			{
				num2--;
			}
			result = input.Substring(num, num2 - num + 1);
			return true;
		}
		num++;
		bool flag = false;
		while (position < input.Length)
		{
			if (input[position] == '"')
			{
				if (position + 1 >= input.Length || input[position + 1] != '"')
				{
					num2 = position - 1;
					position++;
					break;
				}
				position++;
				flag = true;
			}
			position++;
		}
		if (num2 < 0)
		{
			result = null;
			return false;
		}
		result = input.Substring(num, num2 - num + 1);
		if (flag)
		{
			result = result.Replace("\"\"", "\"");
		}
		ConsumeWhitespace(input, ref position);
		if (position == input.Length - 1 && input[position] == '\'')
		{
			result += "'";
			position++;
		}
		if (position < input.Length)
		{
			return input[position++] == delimiter;
		}
		return !delimiterRequired;
	}

	protected static void ConsumeWhitespace(string input, ref int position)
	{
		while (position < input.Length && char.IsWhiteSpace(input, position))
		{
			position++;
		}
	}

	protected static int EvaluateTableIdentifierString(string tableIdentifier, bool needsSingleQuoting = false)
	{
		if (string.IsNullOrEmpty(tableIdentifier))
		{
			return -1;
		}
		string s;
		if (tableIdentifier.StartsWith("Table", StringComparison.OrdinalIgnoreCase))
		{
			s = tableIdentifier.Substring(5);
		}
		else
		{
			s = ((!tableIdentifier.StartsWith("#", StringComparison.Ordinal)) ? tableIdentifier : tableIdentifier.Substring(1));
		}
		if (int.TryParse(s, out var result))
		{
			return result;
		}
		NavSession session = NavCurrentThread.Session;
		NavInteger fromValue = NavInteger.Create(1);
		string text;
		if (needsSingleQuoting)
		{
			text = tableIdentifier;
			if (text[0] == '\'' && text[text.Length - 1] != '\'')
			{
				text += "'";
			}
		}
		else
		{
			text = "@" + tableIdentifier;
		}
		using (NavRecord navRecord = new NavRecord(session, 2000000038))
		{
			navRecord.ALSetRange(1, fromValue);
			navRecord.ALSetFilter(4, text);
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				return navRecord.GetFieldValue(3).ToInt32();
			}
			using NavRecord navRecord2 = new NavRecord(session, 2000000029);
			navRecord2.ALSetRange(1, fromValue);
			navRecord2.ALSetFilter(4, text);
			if (navRecord2.ALFindFirst(DataError.TrapError))
			{
				return navRecord2.GetFieldValue(3).ToInt32();
			}
			text += "*";
			navRecord.ALSetFilter(4, text);
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				return navRecord.GetFieldValue(3).ToInt32();
			}
			navRecord2.ALSetFilter(4, text);
			if (navRecord2.ALFindFirst(DataError.TrapError))
			{
				return navRecord2.GetFieldValue(3).ToInt32();
			}
		}
		return -1;
	}

	protected static int[] EvaluateNumberSequence(string source, ref int position, int numberOfNumbers, int longNumberIndex, int maxNumberOfDigitsInLongNumber, out int numberOfDigitsInLongNumber)
	{
		numberOfDigitsInLongNumber = 0;
		string[] array = new string[numberOfNumbers];
		for (int i = 0; i < array.Length; i++)
		{
			if (i > 0)
			{
				int num = position;
				ConsumeUntilDigit(source, ref position);
				if (position == source.Length)
				{
					position = num;
					break;
				}
			}
			if (!TryReadDigitToken(source, ref position, out var result))
			{
				break;
			}
			array[i] = result;
		}
		int num2 = 0;
		int[] array2 = new int[numberOfNumbers];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = -1;
		}
		foreach (string text in array)
		{
			if (text == null)
			{
				break;
			}
			int num3 = 0;
			while (num3 < text.Length)
			{
				if (num2 >= numberOfNumbers)
				{
					return null;
				}
				if (num2 == longNumberIndex)
				{
					int num4 = Math.Min(maxNumberOfDigitsInLongNumber, text.Length - num3);
					if (!int.TryParse(text.Substring(num3, num4), NumberStyles.Integer, CultureInfo.InvariantCulture, out array2[num2++]))
					{
						return null;
					}
					numberOfDigitsInLongNumber = num4;
					num3 += num4;
				}
				else
				{
					int num5 = Math.Min(2, text.Length - num3);
					if (!int.TryParse(text.Substring(num3, num5), NumberStyles.Integer, CultureInfo.InvariantCulture, out array2[num2++]))
					{
						return null;
					}
					num3 += num5;
				}
			}
		}
		return array2;
	}

	private static void ConsumeUntilDigit(string input, ref int position)
	{
		while (position < input.Length && !char.IsDigit(input, position))
		{
			position++;
		}
	}

	private static bool TryReadDigitToken(string input, ref int position, out string result)
	{
		result = null;
		int num = position;
		while (position < input.Length && char.IsDigit(input, position))
		{
			position++;
		}
		if (num == position)
		{
			return false;
		}
		result = input.Substring(num, position - num);
		return true;
	}

	protected static bool StartsWith(string input, int inputPosition, string value, int valuePosition = 0)
	{
		return string.Compare(input, inputPosition, value, valuePosition, value.Length - valuePosition, StringComparison.OrdinalIgnoreCase) == 0;
	}
}
internal abstract class NavValueEvaluator<T> : NavValueEvaluator where T : NavValue
{
	protected delegate bool TryGetNumericValue<U>(DataError dataError, string source, out U value);

	internal override bool Evaluate(DataError dataError, out NavValue value, INavValueMetadata metadata, string source, int number)
	{
		bool result = Evaluate(dataError, out var value2, metadata, source, number);
		value = value2;
		return result;
	}

	internal bool Evaluate(DataError dataError, out T value, INavValueMetadata metadata, string source, int number)
	{
		try
		{
			bool flag = InternalEvaluate(dataError, out value, metadata, source, number);
			if (!flag && dataError == DataError.ThrowError)
			{
				throw CreateEvaluateException(metadata, source);
			}
			return flag;
		}
		catch (NavNCLEvaluateException) when (dataError == DataError.TrapError)
		{
			value = GetDefaultValue(metadata);
			return false;
		}
	}

	protected abstract bool InternalEvaluate(DataError dataError, out T value, INavValueMetadata metadata, string source, int number);

	protected virtual T GetDefaultValue(INavValueMetadata metadata)
	{
		return (T)NavValue.GetDefaultNavValue(metadata);
	}

	protected static NavNCLEvaluateException CreateEvaluateException(INavValueMetadata metadata, string source)
	{
		return NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, metadata?.NavType ?? NavNclTypeHelper.GetNavTypeFromType(typeof(T)), source);
	}

	protected static bool TryEvaluateToCharOrByte<U>(DataError dataError, string source, TryGetNumericValue<U> tryGetNumericValue, Func<char, U> getValueForLength1Source, out U processedValue)
	{
		processedValue = default;
		string text = source;
		Type typeFromHandle = typeof(U);
		if (typeFromHandle != typeof(char) && typeFromHandle != typeof(byte))
		{
			return ThrowErrorOrReturnFalse(dataError, text);
		}
		if (string.IsNullOrEmpty(text))
		{
			return ThrowErrorOrReturnFalse(dataError, text);
		}
		if (text[0] != ' ')
		{
			text = text.TrimEnd(' ');
		}
		int length = text.Length;
		if (length <= 1)
		{
			processedValue = getValueForLength1Source(text[0]);
			return true;
		}
		if (source[0] == '<')
		{
			if (source[length - 1] != '>' || !tryGetNumericValue(dataError, text.Substring(1, length - 2), out var value))
			{
				return ThrowErrorOrReturnFalse(dataError, text);
			}
			processedValue = value;
			return true;
		}
		return ThrowErrorOrReturnFalse(dataError, text);
	}

	protected static bool ThrowErrorOrReturnFalse(DataError dataError, string source, INavValueMetadata metadata = null)
	{
		if (dataError == DataError.TrapError)
		{
			return false;
		}
		throw CreateEvaluateException(metadata, source);
	}
}
