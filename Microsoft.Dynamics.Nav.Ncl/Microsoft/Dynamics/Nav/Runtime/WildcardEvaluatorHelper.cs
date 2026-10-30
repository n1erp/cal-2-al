using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class WildcardEvaluatorHelper
{
	private static class NativeMethods
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern int FindNLSString(int locale, uint flags, char* sourceString, int sourceCount, [MarshalAs(UnmanagedType.LPWStr)] string findString, int findCount, out int found);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		public unsafe static extern int FindStringOrdinal(uint dwFindStringOrdinalFlags, char* lpStringSource, int cchSource, string lpStringValue, int cchValue, [MarshalAs(UnmanagedType.Bool)] bool bIgnoreCase);
	}

	private const int FIND_FROMSTART = 4194304;

	private const int FIND_STARTSWITH = 1048576;

	private const int FIND_ENDSWITH = 2097152;

	private const int NORM_IGNORECASE = 1;

	private const int NORM_IGNOREKANATYPE = 65536;

	private const int NORM_IGNORENONSPACE = 2;

	private const int NORM_IGNORESYMBOLS = 4;

	private const int NORM_IGNOREWIDTH = 131072;

	private const int SORT_STRINGSORT = 4096;

	private const int NORM_LINGUISTIC_CASING = 134217728;

	private const CompareOptions ValidIndexMaskOffFlags = ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);

	internal unsafe static int IndexOf(int locale, string source, string value, int startIndex, int count, CompareOptions options, out int matchLength)
	{
		if (source == null || value == null)
		{
			throw new ArgumentNullException((source == null) ? "source" : "value");
		}
		if (startIndex > source.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		matchLength = value.Length;
		if (source.Length == 0)
		{
			if (value.Length == 0)
			{
				return 0;
			}
			return -1;
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (count < 0 || startIndex > source.Length - count)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (options == CompareOptions.Ordinal || options == CompareOptions.OrdinalIgnoreCase)
		{
			return source.IndexOf(value, startIndex, count, (options == CompareOptions.Ordinal) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
		}
		fixed (char* ptr = source)
		{
			int num = NativeMethods.FindNLSString(locale, GetNativeCompareFlags(options) | 0x400000, ptr + startIndex, count, value, value.Length, out matchLength);
			if (num >= 0)
			{
				return num + startIndex;
			}
			return num;
		}
	}

	internal static bool IsSuffix(int locale, string source, string suffix, CompareOptions options, out int matchLength)
	{
		return IsSuffix(locale, source, source?.Length ?? 0, suffix, options, out matchLength);
	}

	internal static bool IsSuffix(int locale, string source, int sourceLength, string suffix, CompareOptions options, out int matchLength)
	{
		return IsPrefixSuffixInternal(2097152u, locale, source, 0, sourceLength, suffix, options, out matchLength);
	}

	internal static bool IsPrefix(int locale, string source, string prefix, int startIndex, CompareOptions options, out int matchLength)
	{
		return IsPrefixSuffixInternal(1048576u, locale, source, startIndex, (source?.Length ?? 0) - startIndex, prefix, options, out matchLength);
	}

	private unsafe static bool IsPrefixSuffixInternal(uint operation, int locale, string source, int startIndex, int sourceLength, string value, CompareOptions options, out int matchLength)
	{
		if (source == null || value == null)
		{
			throw new ArgumentNullException((source == null) ? "source" : "value");
		}
		int num = (matchLength = value.Length);
		if (num == 0)
		{
			return true;
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (sourceLength < 0 || startIndex > source.Length - sourceLength)
		{
			throw new ArgumentOutOfRangeException("sourceLength");
		}
		fixed (char* ptr = source)
		{
			if (options == CompareOptions.OrdinalIgnoreCase || options == CompareOptions.Ordinal)
			{
				return NativeMethods.FindStringOrdinal(operation, ptr + startIndex, sourceLength, value, num, options == CompareOptions.OrdinalIgnoreCase) >= 0;
			}
			return NativeMethods.FindNLSString(locale, GetNativeCompareFlags(options) | operation, ptr + startIndex, sourceLength, value, num, out matchLength) >= 0;
		}
	}

	private static uint GetNativeCompareFlags(CompareOptions options)
	{
		if ((options & ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth)) != 0)
		{
			throw new ArgumentException("Unsupported combination of flags", "options");
		}
		uint num = 134217728u;
		if ((options & CompareOptions.IgnoreCase) != 0)
		{
			num |= 1;
		}
		if ((options & CompareOptions.IgnoreKanaType) != 0)
		{
			num |= 0x10000;
		}
		if ((options & CompareOptions.IgnoreNonSpace) != 0)
		{
			num |= 2;
		}
		if ((options & CompareOptions.IgnoreSymbols) != 0)
		{
			num |= 4;
		}
		if ((options & CompareOptions.IgnoreWidth) != 0)
		{
			num |= 0x20000;
		}
		if ((options & CompareOptions.StringSort) != 0)
		{
			num |= 0x1000;
		}
		return num;
	}
}
