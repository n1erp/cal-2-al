using System.Collections.Generic;

namespace CommandLine.Parsing;

internal sealed class OptionGroupParser : ArgumentParser
{
	private readonly bool _ignoreUnkwnownArguments;

	public OptionGroupParser(bool ignoreUnkwnownArguments)
	{
		_ignoreUnkwnownArguments = ignoreUnkwnownArguments;
	}

	public override PresentParserState Parse(IArgumentEnumerator argumentEnumerator, OptionMap map, object options)
	{
		OneCharStringEnumerator oneCharStringEnumerator = new OneCharStringEnumerator(argumentEnumerator.Current.Substring(1));
		while (oneCharStringEnumerator.MoveNext())
		{
			OptionInfo optionInfo = map[oneCharStringEnumerator.Current];
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
			if (!optionInfo.IsBoolean)
			{
				if (argumentEnumerator.IsLast && oneCharStringEnumerator.IsLast)
				{
					return PresentParserState.Failure;
				}
				bool flag;
				if (!oneCharStringEnumerator.IsLast)
				{
					if (!optionInfo.IsArray)
					{
						flag = optionInfo.SetValue(oneCharStringEnumerator.GetRemainingFromNext(), options);
						if (!flag)
						{
							DefineOptionThatViolatesFormat(optionInfo);
						}
						return ArgumentParser.BooleanToParserState(flag);
					}
					ArgumentParser.EnsureOptionAttributeIsArrayCompatible(optionInfo);
					IList<string> nextInputValues = ArgumentParser.GetNextInputValues(argumentEnumerator);
					nextInputValues.Insert(0, oneCharStringEnumerator.GetRemainingFromNext());
					flag = optionInfo.SetValue(nextInputValues, options);
					if (!flag)
					{
						DefineOptionThatViolatesFormat(optionInfo);
					}
					return ArgumentParser.BooleanToParserState(flag, addMoveNextIfTrue: true);
				}
				if (!argumentEnumerator.IsLast && !ArgumentParser.IsInputValue(argumentEnumerator.Next))
				{
					return PresentParserState.Failure;
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
			if (!oneCharStringEnumerator.IsLast && map[oneCharStringEnumerator.Next] == null)
			{
				return PresentParserState.Failure;
			}
			if (!optionInfo.SetValue(value: true, options))
			{
				return PresentParserState.Failure;
			}
		}
		return PresentParserState.Success;
	}
}
