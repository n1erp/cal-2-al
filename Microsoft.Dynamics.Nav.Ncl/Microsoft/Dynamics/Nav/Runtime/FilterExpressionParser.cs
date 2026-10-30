using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class FilterExpressionParser
{
	private class TokenStream
	{
		private readonly List<FilterToken> tokens;

		private int currentIndex = -1;

		private FilterToken next;

		public FilterToken Next => next;

		public TokenStream(List<FilterToken> tokens)
		{
			this.tokens = tokens;
			Consume();
		}

		public FilterToken Consume()
		{
			FilterToken result = next;
			currentIndex++;
			next = ((currentIndex < tokens.Count) ? tokens[currentIndex] : null);
			return result;
		}
	}

	private class ValueFilterExpression : FilterExpression
	{
		public const FilterExpressionType ValueExpressionType = (FilterExpressionType)int.MaxValue;

		private const int MaxUnknownVariableResolverDepth = 5;

		private readonly FilterToken filterToken;

		private readonly FilterExpressionParser parent;

		public FilterToken FilterToken => filterToken;

		public override int ApproximateByteSize => base.ApproximateByteSize + SizableObject.GetApproximateByteSize(filterToken) + SizableObject.GetApproximateByteSize(2) + 24;

		public ValueFilterExpression(FilterToken filterToken, FilterExpressionParser parent)
			: base((FilterExpressionType)int.MaxValue)
		{
			this.filterToken = filterToken;
			this.parent = parent;
		}

		public override bool Evaluate(NavValue navValue)
		{
			throw new NotSupportedException();
		}

		public override RangeList ToRangeList()
		{
			throw new NotSupportedException();
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public virtual NavValue ToNavValue(bool autoExpandToClosingValue = false)
		{
			if (TryGetValue(out var parsedValue, out var exception))
			{
				if (parent.expressionContext.SupportsWildcards && !filterToken.Quoted && IsWildcard(filterToken.Value))
				{
					throw new NavInvalidFilterExpressionException(string.Format(CultureInfo.CurrentCulture, Lang.WilcardCannotBeUsedWithOperator, filterToken.Value));
				}
				if (autoExpandToClosingValue && parsedValue is NavDateTime fromNavDateTime && !NavDateTimeEvaluator.HasTimePart(filterToken.Value))
				{
					return NavDateTimeHelper.ToClosingValueIfTimeIsDateTimeMinimum(fromNavDateTime);
				}
				return parsedValue;
			}
			if (TryInvokeResolverAndParse(out var resolvedExpression))
			{
				if (resolvedExpression.ExpressionType == FilterExpressionType.Equal)
				{
					return ((UnaryFilterExpression)resolvedExpression).Value;
				}
				throw new NavInvalidFilterExpressionException(string.Format(CultureInfo.CurrentCulture, Lang.FilterExpressionVariableMustResolveToAValue, filterToken.Value));
			}
			throw new NavInvalidFilterExpressionException(exception.Message, exception);
		}

		public FilterExpression ToExpression(bool negated, bool useOnlyVariableResolver = false, DataError dataError = DataError.ThrowError)
		{
			NavBaseException exception = null;
			NavValue parsedValue;
			FilterExpression resolvedExpression;
			if (parent.expressionContext.SupportsWildcards)
			{
				if (parent.TryGetWildcard(filterToken.Value, negated, out var wildcardFilterExpression))
				{
					return wildcardFilterExpression;
				}
				if (filterToken.Value.Length > 1 && filterToken.Value[0] == '%')
				{
					if (TryGetValue(out parsedValue, out exception, dataError))
					{
						string text = parsedValue.ToString();
						if (text.Length > 1 && text[0] != '\'' && parent.TryGetWildcard(text, negated, out wildcardFilterExpression))
						{
							return wildcardFilterExpression;
						}
						parent.leafCount++;
						if (!negated)
						{
							return FilterExpression.Equal(parsedValue, parent.expressionContext);
						}
						return FilterExpression.NotEqual(parsedValue, parent.expressionContext);
					}
					TryInvokeResolverAndParse(out resolvedExpression);
					if (!negated)
					{
						return resolvedExpression;
					}
					return resolvedExpression.Negate();
				}
			}
			if (!useOnlyVariableResolver && TryGetValue(out parsedValue, out exception, dataError))
			{
				parent.leafCount++;
				if (!negated)
				{
					return FilterExpression.Equal(parsedValue, parent.expressionContext);
				}
				return FilterExpression.NotEqual(parsedValue, parent.expressionContext);
			}
			if (TryInvokeResolverAndParse(out resolvedExpression))
			{
				if (!negated)
				{
					return resolvedExpression;
				}
				return resolvedExpression.Negate();
			}
			if (dataError == DataError.TrapError)
			{
				return null;
			}
			if (exception == null)
			{
				throw new NavInvalidFilterExpressionException();
			}
			throw new NavInvalidFilterExpressionException(exception.Message, exception);
		}

		private bool TryGetValue(out NavValue parsedValue, out NavBaseException exception, DataError dataError = DataError.ThrowError)
		{
			string value = filterToken.Value;
			exception = null;
			if (value.Length > 1 && value[0] == '%')
			{
				if (int.TryParse(value.Substring(1), out var result))
				{
					int num = result - 1;
					if (num >= 0 && num < parent.values.Length)
					{
						NavValue navValue = parent.values[num];
						if (navValue.NclType != parent.expressionContext.Metadata.NclType)
						{
							navValue = NavValue.CreateNavValueFromObject(parent.expressionContext.Metadata, navValue);
							parent.values[num] = navValue;
						}
						parsedValue = navValue;
						return true;
					}
				}
				else if (!filterToken.Quoted)
				{
					parsedValue = null;
					return false;
				}
			}
			try
			{
				parsedValue = parent.expressionContext.Parse(value, parent.CultureInvariant, dataError);
				return parsedValue != null;
			}
			catch (NavBaseException ex)
			{
				exception = ex;
				parsedValue = null;
				return false;
			}
		}

		private bool TryInvokeResolverAndParse(out FilterExpression resolvedExpression)
		{
			if (parent.unknownVariableResolverDepth > 5)
			{
				resolvedExpression = null;
				return false;
			}
			bool flag = false;
			string text = filterToken.Value;
			if (!filterToken.Quoted && text.Length > 1 && text[0] == '%')
			{
				flag = true;
				text = text.Substring(1);
			}
			string text2;
			try
			{
				text2 = ((parent.customVariableResolver == null) ? NavCurrentThread.Session.SystemCodeunitFactory.FilterHelperTriggers.InvokeMakeNavTypeFilter(text, parent.expressionContext.Metadata.NavType) : parent.customVariableResolver(text, parent.expressionContext.Metadata.NavType));
				parent.hasVariables = true;
			}
			catch (NavBaseException ex)
			{
				throw new NavInvalidFilterExpressionException(ex.Message, ex);
			}
			if (string.Equals(text2, text, StringComparison.Ordinal))
			{
				if (flag)
				{
					throw new NavInvalidFilterExpressionException(string.Format(CultureInfo.CurrentCulture, Lang.FilterExpressionVariableNotResolved, filterToken.Value));
				}
				resolvedExpression = null;
				return false;
			}
			try
			{
				resolvedExpression = ParseWithoutBalancing(text2, parent.expressionContext, parent.customVariableResolver, parent.unknownVariableResolverDepth + 1, new NavValue[0], out var expressionLeafCount, out var hasVariables);
				parent.leafCount += expressionLeafCount;
				parent.hasVariables |= hasVariables;
				return true;
			}
			catch (NavInvalidFilterExpressionException)
			{
				resolvedExpression = null;
				return false;
			}
		}
	}

	private class NavValueFilterExpression : ValueFilterExpression
	{
		private readonly NavValue navValue;

		public override int ApproximateByteSize => base.ApproximateByteSize + SizableObject.GetApproximateByteSize(navValue) + SizableObject.GetApproximateByteSize(1);

		public NavValueFilterExpression(NavValueFilterToken filterToken, FilterExpressionParser parent)
			: base(filterToken, parent)
		{
			navValue = filterToken.NavValue;
		}

		public override NavValue ToNavValue(bool autoExpandToClosingValue = false)
		{
			return navValue;
		}
	}

	private const int RecursionLimit = 50;

	private const int MaxLeafCountBeforeBalancing = 200;

	private const char CaseAndAccentInsensitiveMarker = '@';

	internal static readonly char[] WildcardChars = new char[2] { '*', '?' };

	private readonly FilterExpressionContext expressionContext;

	private readonly Func<string, NavType, string> customVariableResolver;

	private readonly int unknownVariableResolverDepth;

	private readonly TokenStream tokenStream;

	private readonly NavValue[] values;

	private int currentRecursionCount;

	private int leafCount;

	private bool hasVariables;

	private bool CultureInvariant { get; set; }

	private static string MockResolveVariables(string stringToResolve, NavType type)
	{
		return string.Empty;
	}

	private FilterExpressionParser(TokenStream tokenStream, FilterExpressionContext expressionContext, Func<string, NavType, string> customVariableResolver, int unknownVariableResolverDepth, params NavValue[] values)
	{
		this.tokenStream = tokenStream;
		this.expressionContext = expressionContext;
		this.customVariableResolver = customVariableResolver;
		this.unknownVariableResolverDepth = unknownVariableResolverDepth;
		this.values = values;
	}

	public static FilterExpression Parse(string expression, FilterExpressionContext expressionContext, bool allowExpressionEvaluation, out bool hasVariables, params NavValue[] values)
	{
		if (allowExpressionEvaluation)
		{
			return Parse(expression, expressionContext, null, 0, out hasVariables, values);
		}
		return Parse(expression, expressionContext, MockResolveVariables, 0, out hasVariables, values);
	}

	public static FilterExpression Parse(string expression, FilterExpressionContext expressionContext, params NavValue[] values)
	{
		bool flag;
		return Parse(expression, expressionContext, null, 0, out flag, values);
	}

	public static bool TryParse(string expression, FilterExpressionContext expressionContext, out FilterExpression filterExpression, params NavValue[] values)
	{
		bool flag;
		return TryParse(expression, expressionContext, null, 0, out flag, out filterExpression, values);
	}

	public static FilterExpression ParseInvariant(string expression, FilterExpressionContext expressionContext, params NavValue[] values)
	{
		bool flag;
		return Parse(expression, expressionContext, null, 0, out flag, values, cultureInvariant: true);
	}

	public static FilterExpression Parse(string expression, FilterExpressionContext expressionContext, Func<string, NavType, string> customVariableResolver, params NavValue[] values)
	{
		bool flag;
		return Parse(expression, expressionContext, customVariableResolver, 0, out flag, values);
	}

	private static FilterExpression Parse(string expression, FilterExpressionContext expressionContext, Func<string, NavType, string> customVariableResolver, int unknownVariableResolverDepth, out bool hasVariables, NavValue[] values, bool cultureInvariant = false, DataError dataError = DataError.ThrowError)
	{
		FilterExpression filterExpression = ParseWithoutBalancing(expression, expressionContext, customVariableResolver, unknownVariableResolverDepth, values, out var expressionLeafCount, out hasVariables, cultureInvariant, dataError);
		if (filterExpression != null && expressionLeafCount > 200)
		{
			filterExpression = filterExpression.Balance().Simplify();
		}
		return filterExpression;
	}

	private static bool TryParse(string expression, FilterExpressionContext expressionContext, Func<string, NavType, string> customVariableResolver, int unknownVariableResolverDepth, out bool hasVariables, out FilterExpression filterExpression, NavValue[] values, bool cultureInvariant = false)
	{
		filterExpression = Parse(expression, expressionContext, customVariableResolver, unknownVariableResolverDepth, out hasVariables, values, cultureInvariant, DataError.TrapError);
		return filterExpression != null;
	}

	private static FilterExpression ParseWithoutBalancing(string expression, FilterExpressionContext expressionContext, Func<string, NavType, string> customVariableResolver, int unknownVariableResolverDepth, NavValue[] values, out int expressionLeafCount, out bool hasVariables, bool cultureInvariant = false, DataError dataError = DataError.ThrowError)
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		List<FilterToken> tokens = FilterExpressionTokenizer.GetTokens(expression);
		try
		{
			FilterExpression filterExpression = ParseFilterTokensWithoutBalancing(tokens, expressionContext, customVariableResolver, unknownVariableResolverDepth, values, out expressionLeafCount, out hasVariables, useOnlyVariableResolver: false, cultureInvariant, dataError);
			if (dataError == DataError.TrapError && filterExpression == null && expressionContext.NclType == NavNclType.NavDate && tokens.Count != 1)
			{
				List<FilterToken> filterTokens = new List<FilterToken>
				{
					new FilterToken(FilterTokenType.Literal, expression)
				};
				filterExpression = ParseFilterTokensWithoutBalancing(filterTokens, expressionContext, customVariableResolver, unknownVariableResolverDepth, values, out expressionLeafCount, out hasVariables, useOnlyVariableResolver: true, cultureInvariant, dataError);
			}
			return filterExpression;
		}
		catch (NavInvalidFilterExpressionException exception)
		{
			if (expressionContext.NclType != NavNclType.NavDate || tokens.Count == 1)
			{
				mostSpecificInstance.SendExceptionTag("00000X4", Category.FilterParsing, exception, Verbosity.Normal, DiagnosticParameter.SystemMetadata("Parsing failed, can't proceed due to NCL type"));
				throw;
			}
			List<FilterToken> filterTokens2 = new List<FilterToken>
			{
				new FilterToken(FilterTokenType.Literal, expression)
			};
			try
			{
				return ParseFilterTokensWithoutBalancing(filterTokens2, expressionContext, customVariableResolver, unknownVariableResolverDepth, values, out expressionLeafCount, out hasVariables, useOnlyVariableResolver: true, cultureInvariant);
			}
			catch (NavInvalidFilterExpressionException exception2)
			{
				mostSpecificInstance.SendExceptionTag("00000X5", Category.FilterParsing, exception2);
			}
			mostSpecificInstance.SendExceptionTag("00000X6", Category.FilterParsing, exception, Verbosity.Normal);
			throw;
		}
	}

	private static FilterExpression ParseFilterTokensWithoutBalancing(List<FilterToken> filterTokens, FilterExpressionContext expressionContext, Func<string, NavType, string> customVariableResolver, int unknownVariableResolverDepth, NavValue[] values, out int expressionLeafCount, out bool hasVariables, bool useOnlyVariableResolver = false, bool cultureInvariant = false, DataError dataError = DataError.ThrowError)
	{
		FilterExpressionParser filterExpressionParser = new FilterExpressionParser(new TokenStream(filterTokens), expressionContext, customVariableResolver, unknownVariableResolverDepth, values)
		{
			CultureInvariant = cultureInvariant
		};
		FilterExpression result = filterExpressionParser.Parse(useOnlyVariableResolver, dataError);
		expressionLeafCount = filterExpressionParser.leafCount;
		hasVariables = filterExpressionParser.hasVariables;
		return result;
	}

	public static FilterExpression ParseFromBinary(BinaryReader binaryReader, FilterExpressionContext expressionContext)
	{
		List<FilterToken> tokens = FilterExpressionBinaryTokenizer.GetTokens(binaryReader, expressionContext);
		FilterExpressionParser filterExpressionParser = new FilterExpressionParser(new TokenStream(tokens), expressionContext, null, 0);
		return filterExpressionParser.Parse();
	}

	private FilterExpression Parse(bool useOnlyVariableResolver = false, DataError dataError = DataError.ThrowError)
	{
		try
		{
			FilterExpression expression = ParseSubexpression();
			if (tokenStream.Next != null)
			{
				if (dataError == DataError.ThrowError)
				{
					throw new NavInvalidFilterExpressionException(string.Format(CultureInfo.CurrentCulture, (tokenStream.Next.TokenType == FilterTokenType.Literal) ? Lang.FilterExpressionUnexpectedValue : Lang.FilterExpressionUnexpectedToken, tokenStream.Next.Value));
				}
				return null;
			}
			return ToEqualOrWildcardIfValue(expression, isNegated: false, useOnlyVariableResolver, dataError) ?? FilterExpression.BooleanConstant(value: true);
		}
		catch (NavBaseException) when (dataError == DataError.TrapError)
		{
			return null;
		}
	}

	private FilterExpression ParseSubexpression()
	{
		FilterExpression lhs = ParsePrimary();
		return ParseSubexpression(lhs, 0);
	}

	private FilterExpression ParseSubexpression(FilterExpression lhs, int minPrecedence)
	{
		while (tokenStream.Next != null && tokenStream.Next.OperatorPrecedence() >= minPrecedence)
		{
			FilterToken filterToken = tokenStream.Consume();
			int num = filterToken.OperatorPrecedence();
			FilterExpression filterExpression = ParsePrimary();
			while (tokenStream.Next != null && tokenStream.Next.OperatorPrecedence() > num)
			{
				filterExpression = ParseSubexpression(filterExpression, tokenStream.Next.OperatorPrecedence());
			}
			lhs = CombineExpressionsWithOperator(filterToken, lhs, filterExpression);
		}
		if (expressionContext.NclType == NavNclType.NavDateTime)
		{
			lhs = CreateDateTimeRangeExpressionFromValueFilter(lhs as ValueFilterExpression) ?? lhs;
		}
		return lhs;
	}

	private FilterExpression ParsePrimary()
	{
		FilterToken next = tokenStream.Next;
		if (next == null)
		{
			return null;
		}
		if (currentRecursionCount++ > 50)
		{
			throw new NavInvalidFilterExpressionException(Lang.FilterExpressionTooLarge);
		}
		try
		{
			switch (next.TokenType)
			{
			case FilterTokenType.BinaryNegatedWildcard:
			{
				tokenStream.Consume();
				TryGetWildcard(next.Value, negated: true, out var wildcardFilterExpression2);
				return wildcardFilterExpression2;
			}
			case FilterTokenType.BinaryWildcard:
			{
				tokenStream.Consume();
				TryGetWildcard(next.Value, negated: false, out var wildcardFilterExpression);
				return wildcardFilterExpression;
			}
			case FilterTokenType.BinaryValue:
				tokenStream.Consume();
				return new NavValueFilterExpression((NavValueFilterToken)next, this);
			case FilterTokenType.Literal:
				tokenStream.Consume();
				return new ValueFilterExpression(next, this);
			case FilterTokenType.OpenParenthesis:
			{
				tokenStream.Consume();
				FilterExpression result = ParseSubexpression();
				FilterToken filterToken = tokenStream.Consume();
				if (filterToken == null || filterToken.TokenType != FilterTokenType.CloseParenthesis)
				{
					throw new NavInvalidFilterExpressionException(Lang.FilterExpressionMissingClosingParenthesis);
				}
				return result;
			}
			case FilterTokenType.Equal:
			case FilterTokenType.NotEqual:
			case FilterTokenType.GreaterThan:
			case FilterTokenType.LessThan:
			case FilterTokenType.GreaterThanOrEqual:
			case FilterTokenType.LessThanOrEqual:
			{
				tokenStream.Consume();
				if ((next.TokenType == FilterTokenType.Equal || next.TokenType == FilterTokenType.NotEqual) && tokenStream.Next != null && tokenStream.Next.TokenType == next.TokenType)
				{
					tokenStream.Consume();
					if (tokenStream.Next == null)
					{
						leafCount++;
						return FilterExpression.BooleanConstant(next.TokenType == FilterTokenType.Equal);
					}
					throw new NavInvalidFilterExpressionException(string.Format(CultureInfo.CurrentCulture, Lang.FilterExpressionExpectedValueAfter, next.Value));
				}
				FilterExpression filterExpression = ParsePrimary();
				if (filterExpression == null || filterExpression.ExpressionType != (FilterExpressionType)int.MaxValue)
				{
					throw new NavInvalidFilterExpressionException(string.Format(CultureInfo.CurrentCulture, Lang.FilterExpressionExpectedValueAfter, next.Value));
				}
				if (next.TokenType == FilterTokenType.NotEqual)
				{
					return ToEqualOrWildcardIfValue(filterExpression, isNegated: true);
				}
				leafCount++;
				return new UnaryFilterExpression(next.TokenType.ToFilterExpressionType(), ToInnerValue(filterExpression), expressionContext);
			}
			default:
				return null;
			}
		}
		finally
		{
			currentRecursionCount--;
		}
	}

	private FilterExpression CombineExpressionsWithOperator(FilterToken op, FilterExpression lhs, FilterExpression rhs)
	{
		if (op.TokenType == FilterTokenType.DotDot)
		{
			leafCount++;
			if (lhs != null)
			{
				if (lhs.ExpressionType != (FilterExpressionType)int.MaxValue)
				{
					throw new NavInvalidFilterExpressionException(Lang.FilterExpressionRangeLhsMustBeAValue);
				}
				if (rhs != null)
				{
					if (rhs.ExpressionType != (FilterExpressionType)int.MaxValue)
					{
						throw new NavInvalidFilterExpressionException(Lang.FilterExpressionRangeRhsMustBeAValue);
					}
					return FilterExpression.RangeBetweenInclusive(ToInnerValue(lhs), ToInnerValue(rhs, autoExpandToClosingValue: true), expressionContext);
				}
				return FilterExpression.RangeFromInclusive(ToInnerValue(lhs), expressionContext);
			}
			if (rhs != null)
			{
				if (rhs.ExpressionType != (FilterExpressionType)int.MaxValue)
				{
					throw new NavInvalidFilterExpressionException(Lang.FilterExpressionRangeRhsMustBeAValue);
				}
				return FilterExpression.RangeToInclusive(ToInnerValue(rhs, autoExpandToClosingValue: true), expressionContext);
			}
			throw new NavInvalidFilterExpressionException(Lang.FilterExpressionRangeBothSidesOfRangeCannotBeEmpty);
		}
		if (expressionContext.NclType == NavNclType.NavDateTime)
		{
			lhs = CreateDateTimeRangeExpressionFromValueFilter(lhs as ValueFilterExpression) ?? lhs;
			rhs = CreateDateTimeRangeExpressionFromValueFilter(rhs as ValueFilterExpression) ?? rhs;
		}
		if (lhs == null)
		{
			throw new NavInvalidFilterExpressionException(string.Format(CultureInfo.CurrentCulture, Lang.FilterExpressionLhsCannotBeEmpty, op.Value));
		}
		if (rhs == null)
		{
			throw new NavInvalidFilterExpressionException(string.Format(CultureInfo.CurrentCulture, Lang.FilterExpressionRhsCannotBeEmpty, op.Value));
		}
		return new BinaryFilterExpression(op.TokenType.ToFilterExpressionType(), ToEqualOrWildcardIfValue(lhs, isNegated: false), ToEqualOrWildcardIfValue(rhs, isNegated: false));
	}

	private FilterExpression CreateDateTimeRangeExpressionFromValueFilter(ValueFilterExpression datetimeValueExpression)
	{
		if (expressionContext.NclType != NavNclType.NavDateTime || datetimeValueExpression == null)
		{
			return null;
		}
		if (datetimeValueExpression.FilterToken.Value.Contains("%"))
		{
			return null;
		}
		if (NavDateTimeEvaluator.HasTimePart(datetimeValueExpression.FilterToken.Value))
		{
			return null;
		}
		List<FilterToken> tokens = new List<FilterToken>(new FilterToken[3]
		{
			datetimeValueExpression.FilterToken,
			FilterToken.NonLiteral(FilterTokenType.DotDot),
			datetimeValueExpression.FilterToken
		});
		return new FilterExpressionParser(new TokenStream(tokens), expressionContext, customVariableResolver, unknownVariableResolverDepth, values).Parse();
	}

	private static NavValue ToInnerValue(FilterExpression expression, bool autoExpandToClosingValue = false)
	{
		return ((ValueFilterExpression)expression).ToNavValue(autoExpandToClosingValue);
	}

	private static FilterExpression ToEqualOrWildcardIfValue(FilterExpression expression, bool isNegated, bool useOnlyVariableResolver = false, DataError dataError = DataError.ThrowError)
	{
		if (expression != null && expression.ExpressionType == (FilterExpressionType)int.MaxValue)
		{
			return ((ValueFilterExpression)expression).ToExpression(isNegated, useOnlyVariableResolver, dataError);
		}
		return expression;
	}

	private bool TryGetWildcard(string pattern, bool negated, out WildcardFilterExpression wildcardFilterExpression)
	{
		if (!IsWildcard(pattern))
		{
			wildcardFilterExpression = null;
			return false;
		}
		leafCount++;
		if (pattern[0] == '@')
		{
			wildcardFilterExpression = FilterExpression.Wildcard(negated, pattern.Substring(1), caseAndAccentInsensitive: true, expressionContext);
		}
		else
		{
			wildcardFilterExpression = FilterExpression.Wildcard(negated, pattern, caseAndAccentInsensitive: false, expressionContext);
		}
		return true;
	}

	public static bool IsWildcard(string pattern)
	{
		if (pattern.Length > 0)
		{
			if (pattern[0] != '@')
			{
				return pattern.IndexOfAny(WildcardChars) >= 0;
			}
			return true;
		}
		return false;
	}
}
