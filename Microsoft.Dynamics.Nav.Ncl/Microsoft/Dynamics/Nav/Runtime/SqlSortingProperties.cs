using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

public class SqlSortingProperties
{
	public const int IgnoreCase = 1;

	public const int IgnoreNonSpace = 2;

	public const int IgnoreKanaType = 65536;

	public const int IgnoreWidth = 131072;

	private readonly byte collationVersion;

	private readonly int databaseCollationCultureId;

	private readonly string collation;

	private readonly int comparisonStyleFlags;

	private CompareOptions? databaseComparisonStyle;

	private bool? isCollationCompatibleWithWindows;

	private string accentAndCaseInsensitiveCollation;

	private CultureInfo databaseCollationCulture;

	public string AccentAndCaseInsensitiveCollation
	{
		get
		{
			if (IsAccentAndCaseInsensitive)
			{
				accentAndCaseInsensitiveCollation = collation;
			}
			else
			{
				accentAndCaseInsensitiveCollation = collation.Replace("_AS", "_AI");
				accentAndCaseInsensitiveCollation = accentAndCaseInsensitiveCollation.Replace("_CS", "_CI");
			}
			return accentAndCaseInsensitiveCollation;
		}
	}

	public int CollationVersion => collationVersion;

	public bool IsCollationCompatibleWithWindows
	{
		get
		{
			if (!isCollationCompatibleWithWindows.HasValue)
			{
				int major = Environment.OSVersion.Version.Major;
				isCollationCompatibleWithWindows = collationVersion == 2 && (major == 6 || major == 10);
			}
			return isCollationCompatibleWithWindows.Value;
		}
	}

	public CompareOptions DatabaseComparisonStyle
	{
		get
		{
			if (!databaseComparisonStyle.HasValue)
			{
				CompareOptions compareOptions = CompareOptions.None;
				if ((comparisonStyleFlags & 1) != 0)
				{
					compareOptions |= CompareOptions.IgnoreCase;
				}
				if ((comparisonStyleFlags & 2) != 0)
				{
					compareOptions |= CompareOptions.IgnoreNonSpace;
				}
				if ((comparisonStyleFlags & 0x10000) != 0)
				{
					compareOptions |= CompareOptions.IgnoreKanaType;
				}
				if ((comparisonStyleFlags & 0x20000) != 0)
				{
					compareOptions |= CompareOptions.IgnoreWidth;
				}
				databaseComparisonStyle = compareOptions;
			}
			return databaseComparisonStyle.Value;
		}
	}

	public CultureInfo DatabaseCollationCulture
	{
		get
		{
			if (databaseCollationCulture == null)
			{
				databaseCollationCulture = new CultureInfo(databaseCollationCultureId);
			}
			return databaseCollationCulture;
		}
	}

	public string Collation => collation;

	public bool IsAccentAndCaseInsensitive
	{
		get
		{
			if (((uint?)databaseComparisonStyle & 1u) == 1)
			{
				return ((uint?)databaseComparisonStyle & 2u) == 2;
			}
			return false;
		}
	}

	public bool IsCaseInsensitive => (DatabaseComparisonStyle & CompareOptions.IgnoreCase) == CompareOptions.IgnoreCase;

	public bool IsAccentInsensitive => (DatabaseComparisonStyle & CompareOptions.IgnoreNonSpace) == CompareOptions.IgnoreNonSpace;

	public SqlSortingProperties(byte collationVersion, int comparisonStyleFlags, int databaseCollationCultureId, string collation)
	{
		this.collationVersion = collationVersion;
		this.comparisonStyleFlags = comparisonStyleFlags;
		this.databaseCollationCultureId = databaseCollationCultureId;
		this.collation = collation;
	}

	public bool IsSameSorting(SqlSortingProperties otherSortingProperties)
	{
		return string.Equals(collation, otherSortingProperties.collation, StringComparison.Ordinal);
	}
}
