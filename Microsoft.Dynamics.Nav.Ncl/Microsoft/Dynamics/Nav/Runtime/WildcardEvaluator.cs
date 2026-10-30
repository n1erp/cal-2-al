using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

public class WildcardEvaluator
{
	private enum WildcardPatternType
	{
		Const,
		Question,
		Any,
		ConstStar,
		ConstQuestion,
		StarConst,
		QuestionConst,
		QuestionConstStar,
		StarConstQuestion,
		StarConstStar,
		QuestionConstQuestion,
		SomethingMoreComplicated
	}

	private const CompareOptions CaseInsensitiveCompareOptions = CompareOptions.IgnoreCase;

	private const CompareOptions AccentInsensitiveCompareOptions = CompareOptions.IgnoreNonSpace;

	private const CompareOptions AccentAndCaseInsensitiveOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

	private readonly CompareOptions compareOptions;

	private readonly bool isNegated;

	private readonly string originalPattern;

	private readonly int locale;

	private WildcardPatternType? wildcardPatternType;

	private string analyzedPattern;

	private string[] patternTokens;

	private WildcardPatternType PatternType
	{
		get
		{
			if (!wildcardPatternType.HasValue)
			{
				wildcardPatternType = DeterminePatternType();
			}
			return wildcardPatternType.Value;
		}
	}

	public WildcardEvaluator(WildcardFilterExpression wildcardFilterExpression)
	{
		originalPattern = wildcardFilterExpression.Pattern;
		isNegated = wildcardFilterExpression.IsNegated;
		compareOptions = (wildcardFilterExpression.IsCaseAndAccentInsensitive ? (CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) : wildcardFilterExpression.ExpressionContext.DefaultStringStringCompareOptions);
		locale = wildcardFilterExpression.ExpressionContext.SortCulture.LCID;
	}

	public bool Evaluate(string input, FilterExpressionContext expressionContext)
	{
		return isNegated ^ EvaluateImpl(input, expressionContext);
	}

	private bool EvaluateImpl(string input, FilterExpressionContext expressionContext)
	{
		CompareInfo compareInfo = expressionContext.SortCulture.CompareInfo;
		int matchLength;
		int matchLength2;
		switch (PatternType)
		{
		case WildcardPatternType.Const:
			return compareInfo.Compare(input, originalPattern, compareOptions) == 0;
		case WildcardPatternType.Question:
			return input.Length == 1;
		case WildcardPatternType.Any:
			return true;
		case WildcardPatternType.ConstStar:
			return compareInfo.IsPrefix(input, analyzedPattern, compareOptions);
		case WildcardPatternType.ConstQuestion:
			if (WildcardEvaluatorHelper.IsPrefix(locale, input, analyzedPattern, 0, compareOptions, out matchLength))
			{
				return matchLength == input.Length - 1;
			}
			return false;
		case WildcardPatternType.StarConst:
			return compareInfo.IsSuffix(input, analyzedPattern, compareOptions);
		case WildcardPatternType.QuestionConst:
			if (WildcardEvaluatorHelper.IsSuffix(locale, input, analyzedPattern, compareOptions, out matchLength))
			{
				return matchLength == input.Length - 1;
			}
			return false;
		case WildcardPatternType.QuestionConstStar:
			return WildcardEvaluatorHelper.IsPrefix(locale, input, analyzedPattern, 1, compareOptions, out matchLength2);
		case WildcardPatternType.StarConstQuestion:
			return WildcardEvaluatorHelper.IsSuffix(locale, input, input.Length - 1, analyzedPattern, compareOptions, out matchLength2);
		case WildcardPatternType.StarConstStar:
			return compareInfo.IndexOf(input, analyzedPattern, compareOptions) >= 0;
		case WildcardPatternType.QuestionConstQuestion:
			if (WildcardEvaluatorHelper.IsPrefix(locale, input, analyzedPattern, 1, compareOptions, out matchLength))
			{
				return matchLength + 2 == input.Length;
			}
			return false;
		case WildcardPatternType.SomethingMoreComplicated:
			return EvaluateComplexPattern(input, 0, mustStartHere: true, 0, compareInfo);
		default:
			throw new NotSupportedException();
		}
	}

	private WildcardPatternType DeterminePatternType()
	{
		if (originalPattern.Length == 0)
		{
			return WildcardPatternType.Any;
		}
		char c = originalPattern[0];
		if (originalPattern.Length == 1)
		{
			return c switch
			{
				'*' => WildcardPatternType.Any, 
				'?' => WildcardPatternType.Question, 
				_ => WildcardPatternType.Const, 
			};
		}
		if (originalPattern.IndexOfAny(FilterExpressionParser.WildcardChars, 1, originalPattern.Length - 2) >= 0)
		{
			List<string> list = new List<string>();
			int num = 0;
			for (int i = 0; i < originalPattern.Length; i++)
			{
				char c2 = originalPattern[i];
				if (c2 == '?' || c2 == '*')
				{
					if (i - num > 0)
					{
						list.Add(originalPattern.Substring(num, i - num));
					}
					list.Add((c2 == '?') ? "?" : "*");
					num = i + 1;
				}
			}
			if (originalPattern.Length - num > 0)
			{
				list.Add(originalPattern.Substring(num, originalPattern.Length - num));
			}
			patternTokens = list.ToArray();
			return WildcardPatternType.SomethingMoreComplicated;
		}
		char c3 = originalPattern[originalPattern.Length - 1];
		switch (c)
		{
		case '*':
			switch (c3)
			{
			case '*':
				analyzedPattern = originalPattern.Substring(1, originalPattern.Length - 2);
				return WildcardPatternType.StarConstStar;
			case '?':
				analyzedPattern = originalPattern.Substring(1, originalPattern.Length - 2);
				return WildcardPatternType.StarConstQuestion;
			default:
				analyzedPattern = originalPattern.Substring(1);
				return WildcardPatternType.StarConst;
			}
		case '?':
			switch (c3)
			{
			case '*':
				analyzedPattern = originalPattern.Substring(1, originalPattern.Length - 2);
				return WildcardPatternType.QuestionConstStar;
			case '?':
				analyzedPattern = originalPattern.Substring(1, originalPattern.Length - 2);
				return WildcardPatternType.QuestionConstQuestion;
			default:
				analyzedPattern = originalPattern.Substring(1);
				return WildcardPatternType.QuestionConst;
			}
		default:
			switch (c3)
			{
			case '*':
				analyzedPattern = originalPattern.Substring(0, originalPattern.Length - 1);
				return WildcardPatternType.ConstStar;
			case '?':
				analyzedPattern = originalPattern.Substring(0, originalPattern.Length - 1);
				return WildcardPatternType.ConstQuestion;
			default:
				analyzedPattern = originalPattern;
				return WildcardPatternType.Const;
			}
		}
	}

	private bool EvaluateComplexPattern(string input, int inputIndex, bool mustStartHere, int tokenIndex, CompareInfo compareInfo)
	{
		if (tokenIndex == patternTokens.Length)
		{
			if (!mustStartHere)
			{
				return true;
			}
			return inputIndex == input.Length;
		}
		string text = patternTokens[tokenIndex];
		if (text.Length == 1)
		{
			switch (text[0])
			{
			case '?':
			{
				if (mustStartHere)
				{
					if (inputIndex < input.Length)
					{
						return EvaluateComplexPattern(input, inputIndex + 1, mustStartHere: true, tokenIndex + 1, compareInfo);
					}
					return false;
				}
				for (int i = inputIndex; i < input.Length; i++)
				{
					if (EvaluateComplexPattern(input, i + 1, mustStartHere: true, tokenIndex + 1, compareInfo))
					{
						return true;
					}
				}
				return false;
			}
			case '*':
				return EvaluateComplexPattern(input, inputIndex, mustStartHere: false, tokenIndex + 1, compareInfo);
			}
		}
		if (mustStartHere)
		{
			if (inputIndex + 1 > input.Length)
			{
				return false;
			}
			if (WildcardEvaluatorHelper.IsPrefix(locale, input, text, inputIndex, compareOptions, out var matchLength))
			{
				return EvaluateComplexPattern(input, inputIndex + matchLength, mustStartHere: true, tokenIndex + 1, compareInfo);
			}
			return false;
		}
		int num = inputIndex;
		int count;
		while ((count = input.Length - num) >= 1)
		{
			int num2 = WildcardEvaluatorHelper.IndexOf(locale, input, text, num, count, compareOptions, out var matchLength2);
			if (num2 < 0)
			{
				return false;
			}
			if (EvaluateComplexPattern(input, num2 + matchLength2, mustStartHere: true, tokenIndex + 1, compareInfo))
			{
				return true;
			}
			num = num2 + 1;
		}
		return false;
	}
}
