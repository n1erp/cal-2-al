using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CollationAwareStringComparer : IEqualityComparer<string>, IComparer<string>
{
	private readonly SqlSortingProperties sortingProperties;

	private readonly CompareInfo compareInfo;

	private const char Space = ' ';

	private static readonly char[] SpaceArray = new char[1] { ' ' };

	internal CollationAwareStringComparer(SqlSortingProperties properties = null)
	{
		sortingProperties = properties ?? NavGlobal.AppDatabase.SqlSortingProperties;
		compareInfo = sortingProperties.DatabaseCollationCulture.CompareInfo;
	}

	public bool Equals(string x, string y)
	{
		if ((object)x == y)
		{
			return true;
		}
		if (x == null)
		{
			return y == null;
		}
		if (y == null)
		{
			return false;
		}
		return Compare(compareInfo, sortingProperties.DatabaseComparisonStyle, x, y) == 0;
	}

	public int Compare(string x, string y)
	{
		if (y == null)
		{
			if (x != null)
			{
				return 1;
			}
			return 0;
		}
		if (x == null)
		{
			return -1;
		}
		if ((object)x == y)
		{
			return 0;
		}
		return Compare(compareInfo, sortingProperties.DatabaseComparisonStyle, x, y);
	}

	public int GetHashCode(string source)
	{
		if (source == null)
		{
			return 0;
		}
		SortKey sortKey = compareInfo.GetSortKey(source, sortingProperties.DatabaseComparisonStyle);
		return sortKey.GetHashCode();
	}

	internal static int Compare(CompareInfo compareInfo, CompareOptions options, string left, string right)
	{
		string @string = TrimSpacesFromEnd(left);
		string string2 = TrimSpacesFromEnd(right);
		return compareInfo.Compare(@string, string2, options);
	}

	internal static int Compare(CompareInfo compareInfo, CompareOptions options, string left, string right, string emptyValue)
	{
		string text = TrimSpacesFromEnd(left);
		string text2 = TrimSpacesFromEnd(right);
		if (emptyValue != null && IsEmpty(text, emptyValue) && IsEmpty(text2, emptyValue))
		{
			return 0;
		}
		return Compare(compareInfo, options, text, text2);
	}

	private static bool IsEmpty(string stringValue, string emptyValue)
	{
		if (!string.IsNullOrEmpty(stringValue))
		{
			return stringValue.Equals(emptyValue);
		}
		return true;
	}

	private static string TrimSpacesFromEnd(string value)
	{
		if (string.IsNullOrEmpty(value) || value[value.Length - 1] != ' ')
		{
			return value;
		}
		return value.TrimEnd(SpaceArray);
	}
}
