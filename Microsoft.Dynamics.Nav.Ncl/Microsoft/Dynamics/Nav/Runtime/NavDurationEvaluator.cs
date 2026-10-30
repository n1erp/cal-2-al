using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDurationEvaluator : NavValueEvaluator<NavDuration>
{
	private static readonly NavDurationEvaluator instance = new NavDurationEvaluator();

	private readonly int[] xmlDaysInMonth = new int[13]
	{
		0, 30, 60, 90, 121, 151, 182, 213, 243, 274,
		304, 335, 365
	};

	internal static NavDurationEvaluator Instance => instance;

	private NavDurationEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavDuration value, INavValueMetadata metadata, string source, int number)
	{
		value = NavDuration.Default;
		if ((number != 9) ? TryParseDuration(source, number, out var duration) : TryParseDurationXml(source, out duration))
		{
			value = new NavDuration(duration);
			return true;
		}
		return false;
	}

	private bool TryParseDurationXml(string source, out long duration)
	{
		duration = 0L;
		bool flag = false;
		bool flag2 = false;
		long num = 0L;
		int length = source.Length;
		int num2 = SkipBlanks(source, 0, length);
		if (num2 < length && source[num2] == '-')
		{
			num2++;
			flag = true;
		}
		if (num2 >= length || source[num2++] != 'P')
		{
			return false;
		}
		while (num2 < length)
		{
			num2 = SkipBlanks(source, num2, length);
			if (num2 < length && source[num2] == 'T')
			{
				if (flag2)
				{
					return false;
				}
				num2++;
				flag2 = true;
				num2 = SkipBlanks(source, num2, length);
			}
			int num3 = num2;
			long num4 = 0L;
			while (num2 < length && char.IsDigit(source[num2]))
			{
				num4 *= 10;
				num4 += source[num2++] - 48;
			}
			if (num3 == num2)
			{
				return false;
			}
			if (flag)
			{
				num4 = -num4;
			}
			int num5 = -1;
			if (flag2 && num2 < length && source[num2] == '.')
			{
				num2++;
				num5 = 0;
				int num6 = 100;
				while (num2 < length && char.IsDigit(source[num2]) && num6 >= 1)
				{
					num5 += num6 * (source[num2++] - 48);
					num6 /= 10;
				}
				if (flag)
				{
					num5 = -num5;
				}
			}
			num2 = SkipBlanks(source, num2, length);
			char c = '\0';
			if (num2 < length)
			{
				c = source[num2++];
			}
			if (flag2)
			{
				switch (c)
				{
				case 'H':
					if (num5 != -1)
					{
						return false;
					}
					num += num4 * 3600000;
					break;
				case 'M':
					if (num5 != -1)
					{
						return false;
					}
					num += num4 * 60000;
					break;
				case 'S':
					num += num4 * 1000;
					if (num5 != -1)
					{
						num += num5;
					}
					break;
				default:
					return false;
				}
			}
			else
			{
				switch (c)
				{
				case 'Y':
				{
					long num9 = num4 * xmlDaysInMonth[12];
					num += num9 * 86400000;
					break;
				}
				case 'M':
				{
					long num7 = num4 / 12;
					long num8 = num4 - num7 * 12;
					long num9 = num7 * xmlDaysInMonth[12] + xmlDaysInMonth[num8];
					num += num9 * 86400000;
					break;
				}
				case 'D':
					num += num4 * 86400000;
					break;
				default:
					return false;
				}
			}
		}
		duration = num;
		return true;
	}

	private static bool TryParseDuration(string source, int number, out long duration)
	{
		duration = 0L;
		string[] durationUnits = NavCurrentSessionWrapper.FormatSettings.GetDurationUnits();
		bool[] array = new bool[5];
		bool flag = false;
		long num = 0L;
		int length = source.Length;
		int i = SkipBlanks(source, 0, length);
		if (i < length && source[i] == '-')
		{
			flag = true;
			i++;
		}
		while (i < length)
		{
			i = SkipBlanks(source, i, length);
			int num2 = i;
			long num3 = 0L;
			while (i < length && char.IsDigit(source[i]))
			{
				num3 *= 10;
				num3 += source[i++] - 48;
			}
			if (flag)
			{
				num3 = -num3;
			}
			if (num2 == i)
			{
				return false;
			}
			int num4 = -1;
			int num5 = 1;
			if (i < length && source[i] == NavCurrentSessionWrapper.FormatSettings.FieldFormatComma)
			{
				i++;
				num4 = 0;
				while (i < length && char.IsDigit(source[i]))
				{
					num4 = num4 * 10 + (source[i++] - 48);
					num5 *= 10;
				}
				if (flag)
				{
					num4 = -num4;
				}
			}
			i = SkipBlanks(source, i, length);
			num2 = i;
			int num6 = 2;
			if (i < length)
			{
				if (number == 2)
				{
					num6 = 0;
					while (i < length && char.IsDigit(source[i]))
					{
						num6 *= 10;
						num6 += source[i++] - 48;
					}
				}
				else
				{
					for (; i < length && char.IsLetter(source[i]); i++)
					{
					}
					num6 = LookupUnitName(durationUnits, source.Substring(num2, i - num2));
				}
			}
			if (num6 < 0 || num6 > 9 || array[num6 / 2])
			{
				return false;
			}
			array[num6 / 2] = true;
			if (num4 != -1 && (num6 == 8 || num6 == 9))
			{
				return false;
			}
			int durationUnitfactor = GetDurationUnitfactor((DurationComponent)num6);
			num += num3 * durationUnitfactor;
			if (num4 != -1)
			{
				num += num4 * (durationUnitfactor / num5);
			}
		}
		duration = num;
		return true;
	}

	private static int GetDurationUnitfactor(DurationComponent unit)
	{
		switch (unit)
		{
		case DurationComponent.Day:
		case DurationComponent.Plural:
			return 86400000;
		case DurationComponent.Hour:
		case DurationComponent.Hour | DurationComponent.Plural:
			return 3600000;
		case DurationComponent.Minute:
		case DurationComponent.Minute | DurationComponent.Plural:
			return 60000;
		case DurationComponent.Second:
		case DurationComponent.Second | DurationComponent.Plural:
			return 1000;
		case DurationComponent.Millisecond:
		case DurationComponent.Millisecond | DurationComponent.Plural:
			return 1;
		default:
			return 1;
		}
	}

	private static int LookupUnitName(string[] unitNames, string name)
	{
		for (int i = 0; i < unitNames.Length; i++)
		{
			if (name.Equals(unitNames[i], StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		for (int j = 0; j < unitNames.Length; j++)
		{
			if (unitNames[j].StartsWith(name, StringComparison.OrdinalIgnoreCase))
			{
				return j;
			}
		}
		return -1;
	}

	private static int SkipBlanks(string source, int i, int len)
	{
		while (i < len && char.IsWhiteSpace(source[i]))
		{
			i++;
		}
		return i;
	}

	protected override NavDuration GetDefaultValue(INavValueMetadata metadata)
	{
		return NavDuration.Default;
	}
}
