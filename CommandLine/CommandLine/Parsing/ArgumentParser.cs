using System;
using System.Collections.Generic;
using CommandLine.Extensions;

namespace CommandLine.Parsing;

internal abstract class ArgumentParser
{
	public List<ParsingError> PostParsingState { get; private set; }

	protected ArgumentParser()
	{
		PostParsingState = new List<ParsingError>();
	}

	public static bool CompareShort(string argument, char? option, bool caseSensitive)
	{
		return string.Compare(argument, ToOption(option), caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) == 0;
	}

	public static bool CompareLong(string argument, string option, bool caseSensitive)
	{
		return string.Compare(argument, ToOption(option), caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) == 0;
	}

	public static ArgumentParser Create(string argument, bool ignoreUnknownArguments = false)
	{
		if (argument.IsNumeric())
		{
			return null;
		}
		if (IsDash(argument))
		{
			return null;
		}
		if (IsLongOption(argument))
		{
			return new LongOptionParser(ignoreUnknownArguments);
		}
		if (IsShortOption(argument))
		{
			return new OptionGroupParser(ignoreUnknownArguments);
		}
		return null;
	}

	public static bool IsInputValue(string argument)
	{
		if (argument.IsNumeric())
		{
			return true;
		}
		if (argument.Length > 0)
		{
			if (!IsDash(argument))
			{
				return !IsShortOption(argument);
			}
			return true;
		}
		return true;
	}

	public abstract PresentParserState Parse(IArgumentEnumerator argumentEnumerator, OptionMap map, object options);

	internal static IList<string> InternalWrapperOfGetNextInputValues(IArgumentEnumerator ae)
	{
		return GetNextInputValues(ae);
	}

	protected static IList<string> GetNextInputValues(IArgumentEnumerator ae)
	{
		IList<string> list = new List<string>();
		while (ae.MoveNext() && IsInputValue(ae.Current))
		{
			list.Add(ae.Current);
		}
		if (!ae.MovePrevious())
		{
			throw new ParserException();
		}
		return list;
	}

	protected static PresentParserState BooleanToParserState(bool value)
	{
		return BooleanToParserState(value, addMoveNextIfTrue: false);
	}

	protected static PresentParserState BooleanToParserState(bool value, bool addMoveNextIfTrue)
	{
		if (value && !addMoveNextIfTrue)
		{
			return PresentParserState.Success;
		}
		if (value)
		{
			return PresentParserState.Success | PresentParserState.MoveOnNextElement;
		}
		return PresentParserState.Failure;
	}

	protected static void EnsureOptionAttributeIsArrayCompatible(OptionInfo option)
	{
		if (!option.IsAttributeArrayCompatible)
		{
			throw new ParserException();
		}
	}

	protected static void EnsureOptionArrayAttributeIsNotBoundToScalar(OptionInfo option)
	{
		if (!option.IsArray && option.IsAttributeArrayCompatible)
		{
			throw new ParserException();
		}
	}

	protected void DefineOptionThatViolatesFormat(OptionInfo option)
	{
		PostParsingState.Add(new ParsingError(option.ShortName, option.LongName, format: true));
	}

	private static string ToOption(string value)
	{
		return "--" + value;
	}

	private static string ToOption(char? value)
	{
		return "-" + value;
	}

	private static bool IsDash(string value)
	{
		return string.CompareOrdinal(value, "-") == 0;
	}

	private static bool IsShortOption(string value)
	{
		return value[0] == '-';
	}

	private static bool IsLongOption(string value)
	{
		if (value[0] == '-')
		{
			return value[1] == '-';
		}
		return false;
	}
}
