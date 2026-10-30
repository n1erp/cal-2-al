using System.Collections.Generic;

namespace CommandLine.Parsing;

internal sealed class LongOptionParser : ArgumentParser
{
	private readonly bool _ignoreUnkwnownArguments;

	public LongOptionParser(bool ignoreUnkwnownArguments)
	{
		_ignoreUnkwnownArguments = ignoreUnkwnownArguments;
	}

	public override PresentParserState Parse(IArgumentEnumerator argumentEnumerator, OptionMap map, object options)
	{
		string[] array = argumentEnumerator.Current.Substring(2).Split(new char[1] { '=' }, 2);
		OptionInfo optionInfo = map[array[0]];
		if (optionInfo == null)
		{
			if (!_ignoreUnkwnownArguments)
			{
				return PresentParserState.Failure;
			}
			return PresentParserState.MoveOnNextElement;
		}
		optionInfo.IsDefined = true;
		ArgumentParser.EnsureOptionArrayAttributeIsNotBoundToScalar(optionInfo);
		bool flag;
		if (!optionInfo.IsBoolean)
		{
			if (array.Length == 1 && (argumentEnumerator.IsLast || !ArgumentParser.IsInputValue(argumentEnumerator.Next)))
			{
				return PresentParserState.Failure;
			}
			if (array.Length == 2)
			{
				if (!optionInfo.IsArray)
				{
					flag = optionInfo.SetValue(array[1], options);
					if (!flag)
					{
						DefineOptionThatViolatesFormat(optionInfo);
					}
					return ArgumentParser.BooleanToParserState(flag);
				}
				ArgumentParser.EnsureOptionAttributeIsArrayCompatible(optionInfo);
				IList<string> nextInputValues = ArgumentParser.GetNextInputValues(argumentEnumerator);
				nextInputValues.Insert(0, array[1]);
				flag = optionInfo.SetValue(nextInputValues, options);
				if (!flag)
				{
					DefineOptionThatViolatesFormat(optionInfo);
				}
				return ArgumentParser.BooleanToParserState(flag);
			}
			if (!optionInfo.IsArray)
			{
				flag = optionInfo.SetValue(argumentEnumerator.Next, options);
				if (!flag)
				{
					DefineOptionThatViolatesFormat(optionInfo);
				}
				return ArgumentParser.BooleanToParserState(flag, addMoveNextIfTrue: true);
			}
			ArgumentParser.EnsureOptionAttributeIsArrayCompatible(optionInfo);
			IList<string> nextInputValues2 = ArgumentParser.GetNextInputValues(argumentEnumerator);
			flag = optionInfo.SetValue(nextInputValues2, options);
			if (!flag)
			{
				DefineOptionThatViolatesFormat(optionInfo);
			}
			return ArgumentParser.BooleanToParserState(flag);
		}
		if (array.Length == 2)
		{
			return PresentParserState.Failure;
		}
		flag = optionInfo.SetValue(value: true, options);
		if (!flag)
		{
			DefineOptionThatViolatesFormat(optionInfo);
		}
		return ArgumentParser.BooleanToParserState(flag);
	}
}
