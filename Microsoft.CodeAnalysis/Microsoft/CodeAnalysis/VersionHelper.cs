using System;
using System.Globalization;
using System.Numerics;

namespace Microsoft.CodeAnalysis;

internal static class VersionHelper
{
	internal static bool TryParse(string s, out Version version)
	{
		return TryParse(s, allowWildcard: false, ushort.MaxValue, allowPartialParse: true, out version);
	}

	internal static bool TryParseAssemblyVersion(string s, bool allowWildcard, out Version version)
	{
		return TryParse(s, allowWildcard, 65534, allowPartialParse: false, out version);
	}

	private static bool TryParse(string s, bool allowWildcard, ushort maxValue, bool allowPartialParse, out Version version)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			version = AssemblyIdentity.NullVersion;
			return false;
		}
		string[] array = s.Split(new char[1] { '.' });
		bool flag = allowWildcard && array[^1] == "*";
		if ((flag && array.Length < 3) || array.Length > 4)
		{
			version = AssemblyIdentity.NullVersion;
			return false;
		}
		ushort[] array2 = new ushort[4];
		int num = (flag ? (array.Length - 1) : array.Length);
		bool flag2 = false;
		for (int i = 0; i < num; i++)
		{
			if (ushort.TryParse(array[i], NumberStyles.None, CultureInfo.InvariantCulture, out array2[i]) && array2[i] <= maxValue)
			{
				continue;
			}
			if (!allowPartialParse)
			{
				version = AssemblyIdentity.NullVersion;
				return false;
			}
			flag2 = true;
			if (string.IsNullOrWhiteSpace(array[i]))
			{
				array2[i] = 0;
				break;
			}
			if (array2[i] > maxValue)
			{
				array2[i] = 0;
				continue;
			}
			bool flag3 = false;
			_ = (BigInteger)0;
			for (int j = 0; j < array[i].Length; j++)
			{
				if (!char.IsDigit(array[i][j]))
				{
					flag3 = true;
					TryGetValue(array[i].Substring(0, j), out array2[i]);
					break;
				}
			}
			if (flag3 || !TryGetValue(array[i], out array2[i]))
			{
				break;
			}
		}
		if (flag)
		{
			for (int k = num; k < array2.Length; k++)
			{
				array2[k] = ushort.MaxValue;
			}
		}
		version = new Version(array2[0], array2[1], array2[2], array2[3]);
		return !flag2;
	}

	private static bool TryGetValue(string s, out ushort value)
	{
		if (BigInteger.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out var result))
		{
			value = (ushort)(result % 65536);
			return true;
		}
		value = 0;
		return false;
	}

	public static Version GenerateVersionFromPatternAndCurrentTime(DateTime time, Version pattern)
	{
		if (pattern == null || pattern.Revision != 65535)
		{
			return pattern;
		}
		if (time == default(DateTime))
		{
			time = DateTime.Now;
		}
		int num = (int)time.TimeOfDay.TotalSeconds / 2;
		if (pattern.Build == 65535)
		{
			int num2 = Math.Min(65535, (int)(time.Date - new DateTime(2000, 1, 1)).TotalDays);
			return new Version(pattern.Major, pattern.Minor, (ushort)num2, (ushort)num);
		}
		return new Version(pattern.Major, pattern.Minor, pattern.Build, (ushort)num);
	}
}
