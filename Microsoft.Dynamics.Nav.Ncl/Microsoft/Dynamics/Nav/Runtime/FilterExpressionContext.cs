using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class FilterExpressionContext : ISizableObject, IComparer<NavValue>, IEquatable<FilterExpressionContext>
{
	private readonly CompareOptions defaultStringCompareOptions;

	private readonly INavFieldMetadata navFieldMetadata;

	private readonly CultureInfo sortCulture;

	private readonly bool canBeOptimized;

	private readonly NavNclType nclType;

	private readonly int hashCode;

	private static readonly NavBLOB EmptyBlob = new NavBLOB();

	private static readonly NavBLOB NonEmptyBlob = new NavBLOB(1);

	private const string StartsWithSingleQuoteSubRegex = "^\\'";

	private const string IsEmptySubRegex = "^\\s*$";

	private const string StartsWithWhiteSpaceSubRegex = "^\\s";

	private const string StartsWithCaseInsensitivityOperatorRegex = "^@";

	private const string EndsWithWhiteSpaceSubRegex = "\\s$";

	private const string ContainsFilterExpressionTokensSubRegex = "[=<>()&|]|\\.\\.";

	private const string ContainsCloseParenthesisSubRegex = "\\)";

	private static readonly string ContainsWildcardCharactersSubRegex = "[" + new string(FilterExpressionParser.WildcardChars) + "]";

	private static readonly Regex TextLiteralNeedsQuotingRegex = BuildRegexFromSubexpressions("^\\'", "^\\s", "\\s$", "^\\s*$", ContainsWildcardCharactersSubRegex, "[=<>()&|]|\\.\\.", "^@");

	private static readonly Regex TextWildcardNeedsQuotingRegex = BuildRegexFromSubexpressions("^\\'", "^\\s", "\\s$", "^\\s*$", "[=<>()&|]|\\.\\.");

	private static readonly Regex NonTextLiteralNeedsQuotingRegex = BuildRegexFromSubexpressions("^\\'", "^\\s*$", "[=<>()&|]|\\.\\.");

	private static readonly Regex OnlyQuoteCloseParenthesisRegex = BuildRegexFromSubexpressions("^\\s", "\\s$", "\\)");

	private static readonly Regex BlobNeedsQuotingRegex = BuildRegexFromSubexpressions("^\\s*$");

	public int ApproximateByteSize => SizableObject.GetApproximateByteSize(6) + 24;

	public NavNclType NclType => nclType;

	public CultureInfo SortCulture => sortCulture;

	public CompareOptions DefaultStringStringCompareOptions => defaultStringCompareOptions;

	public bool CanBeOptimized => canBeOptimized;

	public INavFieldMetadata Metadata => navFieldMetadata;

	public bool CanCalculateMinMaxAndMid
	{
		get
		{
			switch (nclType)
			{
			case NavNclType.NavInteger:
			case NavNclType.NavBigInteger:
			case NavNclType.NavDecimal:
			case NavNclType.NavDuration:
			case NavNclType.NavTime:
			case NavNclType.NavDateTime:
				return true;
			default:
				return false;
			}
		}
	}

	public bool IsDiscrete
	{
		get
		{
			switch (nclType)
			{
			case NavNclType.NavInteger:
			case NavNclType.NavBigInteger:
			case NavNclType.NavOption:
			case NavNclType.NavDuration:
				return true;
			default:
				return false;
			}
		}
	}

	public NavValue MaxValue => nclType switch
	{
		NavNclType.NavInteger => (NavValue)NavInteger.MaxValue, 
		NavNclType.NavBigInteger => NavBigInteger.MaxValue, 
		NavNclType.NavDecimal => NavDecimal.MaxValue, 
		NavNclType.NavDuration => NavDuration.MaxValue, 
		NavNclType.NavTime => NavTime.MaxValue, 
		NavNclType.NavDateTime => NavDateTime.MaxValue, 
		_ => throw new NotSupportedException(), 
	};

	public NavValue MinValue => nclType switch
	{
		NavNclType.NavInteger => (NavValue)NavInteger.MinValue, 
		NavNclType.NavBigInteger => NavBigInteger.MinValue, 
		NavNclType.NavDecimal => NavDecimal.MinValue, 
		NavNclType.NavDuration => NavDuration.MinValue, 
		NavNclType.NavTime => NavTime.MinValue, 
		NavNclType.NavDateTime => NavDateTime.MinValue, 
		_ => throw new NotSupportedException(), 
	};

	public bool IsStringBased
	{
		get
		{
			switch (nclType)
			{
			case NavNclType.NavOemText:
			case NavNclType.NavOemCode:
			case NavNclType.NavText:
			case NavNclType.NavCode:
				return true;
			default:
				return false;
			}
		}
	}

	public bool SupportsWildcards => IsStringBased;

	private FilterExpressionContext(INavFieldMetadata navFieldMetadata, CultureInfo sortCulture, CompareOptions defaultStringCompareOptions, bool canBeOptimized)
	{
		if (navFieldMetadata == null)
		{
			throw new ArgumentNullException("navFieldMetadata");
		}
		this.navFieldMetadata = navFieldMetadata;
		this.sortCulture = sortCulture;
		this.defaultStringCompareOptions = defaultStringCompareOptions;
		this.canBeOptimized = canBeOptimized;
		nclType = this.navFieldMetadata.NclType;
		hashCode = ComputeHashCode();
	}

	public NavValue Midpoint(NavValue low, NavValue high)
	{
		switch (nclType)
		{
		case NavNclType.NavInteger:
		{
			int num7 = low.ToInt32();
			int num8 = high.ToInt32();
			int value4 = (((num7 >= 0 && num8 >= 0) || (num7 < 0 && num8 < 0)) ? (num7 + (num8 - num7) / 2) : ((num8 + num7) / 2));
			return NavInteger.Create(value4);
		}
		case NavNclType.NavBigInteger:
		{
			long num5 = low.ToInt64();
			long num6 = high.ToInt64();
			long value3 = (((num5 >= 0 && num6 >= 0) || (num5 < 0 && num6 < 0)) ? (num5 + (num6 - num5) / 2) : ((num6 + num5) / 2));
			return NavBigInteger.Create(value3);
		}
		case NavNclType.NavDecimal:
		{
			Decimal18 @decimal = low.ToDecimal();
			Decimal18 decimal2 = high.ToDecimal();
			Decimal18 value2 = (((@decimal >= 0 && decimal2 >= 0) || (@decimal < 0 && decimal2 < 0)) ? (@decimal + (decimal2 - @decimal) / 2) : ((decimal2 + @decimal) / 2));
			return NavDecimal.Create(value2);
		}
		case NavNclType.NavDuration:
		{
			long num3 = low.ToInt64();
			long num4 = high.ToInt64();
			long initValue = (((num3 >= 0 && num4 >= 0) || (num3 < 0 && num4 < 0)) ? (num3 + (num4 - num3) / 2) : ((num4 + num3) / 2));
			return new NavDuration(initValue);
		}
		case NavNclType.NavTime:
			return NavTime.Create(new DateTime((high.ToDateTime().Ticks + low.ToDateTime().Ticks) / 2, DateTimeKind.Local));
		case NavNclType.NavDateTime:
		{
			long ticks = low.ToDateTime().Ticks;
			long ticks2 = high.ToDateTime().Ticks;
			long ticks3 = ticks + (ticks2 - ticks) / 2;
			return NavDateTime.Create(new DateTime(ticks3, DateTimeKind.Utc));
		}
		case NavNclType.NavOption:
		{
			int num = low.ToInt32();
			int num2 = high.ToInt32();
			int value = (((num >= 0 && num2 >= 0) || (num < 0 && num2 < 0)) ? (num + (num2 - num) / 2) : ((num2 + num) / 2));
			return NavOption.Create(low.NavOptionMetadata, value);
		}
		default:
			throw new NotSupportedException();
		}
	}

	internal static double RelativeDistanceBetwenValues(NavValue lowValue, NavValue highValue)
	{
		switch (lowValue.NclType)
		{
		case NavNclType.NavInteger:
		case NavNclType.NavOption:
			return (double)highValue.ToInt32() - (double)lowValue.ToInt32();
		case NavNclType.NavBigInteger:
		case NavNclType.NavDuration:
			return (double)highValue.ToInt64() - (double)lowValue.ToInt64();
		case NavNclType.NavDecimal:
			return (double)(decimal)highValue.ToDecimal() - (double)(decimal)lowValue.ToDecimal();
		case NavNclType.NavTime:
		case NavNclType.NavDateTime:
			return highValue.ToDateTime().Ticks - lowValue.ToDateTime().Ticks;
		default:
			throw new NotSupportedException();
		}
	}

	public virtual NavValue Parse(string input)
	{
		return ALSystemVariable.EvaluateIntoNavValue(DataError.ThrowError, navFieldMetadata, input, 0);
	}

	public virtual NavValue Parse(string input, bool cultureInvariant, DataError dataError = DataError.ThrowError)
	{
		if (cultureInvariant && navFieldMetadata.NclType == NavNclType.NavDate && input.StartsWith(NavSession.InvariantFormatSettings.ClosingDateCharacter, StringComparison.Ordinal))
		{
			NavDate date = (NavDate)ALSystemVariable.EvaluateIntoNavValue(dataError, navFieldMetadata, input.Substring(NavSession.InvariantFormatSettings.ClosingDateCharacter.Length), 9);
			return ALSystemDate.ALClosingDate(date);
		}
		return ALSystemVariable.EvaluateIntoNavValue(dataError, navFieldMetadata, input, cultureInvariant ? 9 : 0);
	}

	public NavValue ParseBinary(byte[] buffer, int offset, int length)
	{
		if (navFieldMetadata.NclType == NavNclType.NavBlob)
		{
			if (length != 8)
			{
				throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, navFieldMetadata.NclType.ToString());
			}
			long num = BitConverter.ToInt64(buffer, offset);
			if (num != 0L)
			{
				return NonEmptyBlob;
			}
			return EmptyBlob;
		}
		return NavValue.CreateNavValueFromBytes(navFieldMetadata, buffer, offset, length);
	}

	public int ByteLengthFromLogicalLength(int logicalLength)
	{
		if (nclType != NavNclType.NavText && nclType != NavNclType.NavCode)
		{
			return logicalLength;
		}
		return logicalLength * 2 + ((nclType == NavNclType.NavCode) ? 1 : 0);
	}

	public int LogicalLengthFromByteLength(int byteLength)
	{
		if (nclType != NavNclType.NavText && nclType != NavNclType.NavCode)
		{
			return byteLength;
		}
		return byteLength / 2;
	}

	public int Compare(NavValue x, NavValue y)
	{
		if (x == null)
		{
			throw new ArgumentNullException("x");
		}
		if (y == null)
		{
			throw new ArgumentNullException("y");
		}
		switch (nclType)
		{
		case NavNclType.NavOemCode:
		case NavNclType.NavCode:
		{
			NavCodeBase navCodeBase = (NavCodeBase)x;
			NavCodeBase navCodeBase2 = (NavCodeBase)y;
			switch (Metadata.SqlDataType)
			{
			case SqlDataType.Integer:
			{
				int result = 0;
				int result2 = 0;
				if (navCodeBase.Value.Length > 0 && (navCodeBase.Value[0] == '0' || !int.TryParse(navCodeBase.Value, out result)))
				{
					result = int.MaxValue;
				}
				if (navCodeBase2.Value.Length > 0 && (navCodeBase2.Value[0] == '0' || !int.TryParse(navCodeBase2.Value, out result2)))
				{
					result2 = int.MaxValue;
				}
				return result.CompareTo(result2);
			}
			case SqlDataType.BigInt:
			{
				long result3 = 0L;
				long result4 = 0L;
				if (navCodeBase.Value.Length > 0 && (navCodeBase.Value[0] == '0' || !long.TryParse(navCodeBase.Value, out result3)))
				{
					result3 = long.MaxValue;
				}
				if (navCodeBase2.Value.Length > 0 && (navCodeBase2.Value[0] == '0' || !long.TryParse(navCodeBase2.Value, out result4)))
				{
					result4 = long.MaxValue;
				}
				return result3.CompareTo(result4);
			}
			case SqlDataType.Variant:
			{
				if (navCodeBase.AlphaIndex == 0 || navCodeBase2.AlphaIndex == 0)
				{
					if (navCodeBase.AlphaIndex == navCodeBase2.AlphaIndex)
					{
						return 0;
					}
					if (navCodeBase.AlphaIndex != 0)
					{
						return 1;
					}
					return -1;
				}
				int num = navCodeBase.AlphaIndex - navCodeBase2.AlphaIndex;
				if (num != 0)
				{
					if (navCodeBase.AlphaIndex == byte.MaxValue || navCodeBase2.AlphaIndex == byte.MaxValue)
					{
						return -num;
					}
					if (navCodeBase.Value[0] == '0' || navCodeBase2.Value[0] == '0')
					{
						return long.Parse(navCodeBase.Value, NavCurrentThread.Session.WindowsCulture).CompareTo(long.Parse(navCodeBase2.Value, NavCurrentThread.Session.WindowsCulture));
					}
					return num;
				}
				if (navCodeBase.AlphaIndex != byte.MaxValue)
				{
					return string.CompareOrdinal(navCodeBase.Value, navCodeBase2.Value);
				}
				break;
			}
			default:
				throw new NotSupportedException();
			case SqlDataType.VarChar:
			case SqlDataType.Default:
				break;
			}
			goto case NavNclType.NavOemText;
		}
		case NavNclType.NavOemText:
		case NavNclType.NavText:
			return CollationAwareStringComparer.Compare(SortCulture.CompareInfo, DefaultStringStringCompareOptions, x.ToString(), y.ToString(), "''");
		case NavNclType.NavBlob:
		{
			NavBLOB navBLOB = (NavBLOB)x;
			NavBLOB navBLOB2 = (NavBLOB)y;
			if (navBLOB.IsZeroOrEmpty)
			{
				if (!navBLOB2.IsZeroOrEmpty)
				{
					return -1;
				}
				return 0;
			}
			if (!navBLOB2.IsZeroOrEmpty)
			{
				return 0;
			}
			return 1;
		}
		default:
			return x.CompareTo(y);
		}
	}

	public static FilterExpressionContext CreateForTextField(INavFieldMetadata navFieldMetadata, CultureInfo sortCulture, CompareOptions defaultStringCompareOptions, bool canBeOptimized)
	{
		if (sortCulture == null)
		{
			throw new ArgumentNullException("sortCulture");
		}
		return new FilterExpressionContext(navFieldMetadata, sortCulture, defaultStringCompareOptions, canBeOptimized);
	}

	public static FilterExpressionContext CreateForNonTextField(INavFieldMetadata navFieldMetadata)
	{
		return new FilterExpressionContext(navFieldMetadata, CultureInfo.InvariantCulture, CompareOptions.None, canBeOptimized: true);
	}

	public static FilterExpressionContext CreateDefaultForType(NavNclType nclType)
	{
		return NavFieldMetadata.DefaultMetadata(NavValueMetadata.DefaultMetadata(nclType)).ExpressionContext;
	}

	internal static bool AreDiscreteValuesContiguous(NavValue lowValue, NavValue highValue)
	{
		switch (lowValue.NclType)
		{
		case NavNclType.NavInteger:
		case NavNclType.NavOption:
			return highValue.ToInt32() - lowValue.ToInt32() == 1;
		case NavNclType.NavBigInteger:
		case NavNclType.NavDuration:
			return highValue.ToInt64() - lowValue.ToInt64() == 1;
		default:
			throw new NotSupportedException();
		}
	}

	public bool Equals(FilterExpressionContext other)
	{
		if (other != null)
		{
			if (this != other)
			{
				if (hashCode == other.hashCode && nclType == other.nclType && navFieldMetadata.Equals(other.navFieldMetadata) && object.Equals(sortCulture, other.sortCulture))
				{
					return defaultStringCompareOptions == other.defaultStringCompareOptions;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return hashCode;
	}

	private int ComputeHashCode()
	{
		if (sortCulture == null)
		{
			return navFieldMetadata.GetHashCode();
		}
		return HashCodeHelper.CombineHashCodes(navFieldMetadata.GetHashCode(), sortCulture.GetHashCode(), defaultStringCompareOptions.GetHashCode());
	}

	public string ValueToString(NavValue value, FilterExpressionStringOptions stringOptions)
	{
		bool flag = (stringOptions & FilterExpressionStringOptions.FormatCultureInvariant) != 0;
		if (nclType == NavNclType.NavOption)
		{
			if ((stringOptions & FilterExpressionStringOptions.OptionsAsIntegers) != 0)
			{
				return value.ToInt32().ToString(CultureInfo.InvariantCulture);
			}
			string text = (((stringOptions & FilterExpressionStringOptions.OptionsNonLocalized) == 0) ? value.ToString() : value.NavOptionMetadata.GetOptionFromIndex(value.ToInt32()));
			if (string.IsNullOrEmpty(text))
			{
				return ValueToString(value, FilterExpressionStringOptions.OptionsAsIntegers);
			}
			return text;
		}
		if (nclType == NavNclType.NavDateTime || nclType == NavNclType.NavTime)
		{
			DateTime dateTime = value.ToDateTime();
			if (dateTime.Second != 0 || dateTime.Millisecond != 0)
			{
				return NavFormatEvaluateHelper.Format(value, 0, (!flag) ? 1 : 9);
			}
		}
		else if ((nclType == NavNclType.NavDate) & flag)
		{
			DateTime dateTime2 = value.ToDateTime();
			if (dateTime2 != NavDateTimeHelper.DateTimeUndefined && NavDate.IsClosingDate(dateTime2))
			{
				return NavSession.InvariantFormatSettings.ClosingDateCharacter + value.ToString(invariant: true);
			}
		}
		else if (nclType == NavNclType.NavBoolean && (stringOptions & FilterExpressionStringOptions.OptionsAsIntegers) != 0)
		{
			return value.ToInt32().ToString(CultureInfo.InvariantCulture);
		}
		return value.ToString(flag);
	}

	internal string QuoteLiteralIfNecessary(string literalValue, FilterExpressionStringOptions stringOptions = FilterExpressionStringOptions.None)
	{
		bool useDoubleQuotesForQuoting = false;
		Regex needsQuotingRegex;
		if (Metadata.NclType == NavNclType.NavBlob)
		{
			needsQuotingRegex = BlobNeedsQuotingRegex;
		}
		else if ((stringOptions & FilterExpressionStringOptions.ConstExpressionImplied) != FilterExpressionStringOptions.ConstExpressionImplied)
		{
			if (IsStringBased)
			{
				needsQuotingRegex = (((stringOptions & FilterExpressionStringOptions.DoNotQuoteWildcards) != FilterExpressionStringOptions.DoNotQuoteWildcards) ? TextLiteralNeedsQuotingRegex : TextWildcardNeedsQuotingRegex);
			}
			else
			{
				needsQuotingRegex = NonTextLiteralNeedsQuotingRegex;
			}
		}
		else
		{
			needsQuotingRegex = OnlyQuoteCloseParenthesisRegex;
			useDoubleQuotesForQuoting = true;
		}
		return QuoteIfNecessary(literalValue, needsQuotingRegex, useDoubleQuotesForQuoting);
	}

	private static string QuoteIfNecessary(string literalValue, Regex needsQuotingRegex, bool useDoubleQuotesForQuoting)
	{
		if (!needsQuotingRegex.IsMatch(literalValue))
		{
			return literalValue;
		}
		if (useDoubleQuotesForQuoting)
		{
			return new StringBuilder().Append('"').Append(literalValue).Replace("\"", "\"\"", 1, literalValue.Length)
				.Append('"')
				.ToString();
		}
		return new StringBuilder().Append('\'').Append(literalValue).Replace("'", "''", 1, literalValue.Length)
			.Append('\'')
			.ToString();
	}

	private static Regex BuildRegexFromSubexpressions(params string[] options)
	{
		return new Regex(string.Join("|", options), RegexOptions.Compiled | RegexOptions.CultureInvariant);
	}
}
