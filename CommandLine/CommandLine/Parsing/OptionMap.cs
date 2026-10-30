using System;
using System.Collections.Generic;
using System.Reflection;
using CommandLine.Extensions;
using CommandLine.Infrastructure;

namespace CommandLine.Parsing;

internal sealed class OptionMap
{
	private sealed class MutuallyExclusiveInfo
	{
		private int _count;

		public OptionInfo BadOption { get; private set; }

		public int Occurrence => _count;

		public MutuallyExclusiveInfo(OptionInfo option)
		{
			BadOption = option;
		}

		public void IncrementOccurrence()
		{
			_count++;
		}
	}

	private readonly ParserSettings _settings;

	private readonly Dictionary<string, string> _names;

	private readonly Dictionary<string, OptionInfo> _map;

	private readonly Dictionary<string, MutuallyExclusiveInfo> _mutuallyExclusiveSetMap;

	internal object RawOptions { private get; set; }

	public OptionInfo this[string key]
	{
		get
		{
			OptionInfo result = null;
			if (_map.ContainsKey(key))
			{
				result = _map[key];
			}
			else if (_names.ContainsKey(key))
			{
				string key2 = _names[key];
				result = _map[key2];
			}
			return result;
		}
		set
		{
			_map[key] = value;
			if (value.HasBothNames)
			{
				_names[value.LongName] = new string(value.ShortName.Value, 1);
			}
		}
	}

	internal OptionMap(int capacity, ParserSettings settings)
	{
		_settings = settings;
		IEqualityComparer<string> comparer = (_settings.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
		_names = new Dictionary<string, string>(capacity, comparer);
		_map = new Dictionary<string, OptionInfo>(capacity * 2, comparer);
		if (_settings.MutuallyExclusive)
		{
			_mutuallyExclusiveSetMap = new Dictionary<string, MutuallyExclusiveInfo>(capacity, StringComparer.OrdinalIgnoreCase);
		}
	}

	public static OptionMap Create(object target, ParserSettings settings)
	{
		IList<Pair<PropertyInfo, BaseOptionAttribute>> list = ReflectionHelper.RetrievePropertyList<BaseOptionAttribute>(target);
		if (list == null)
		{
			return null;
		}
		OptionMap optionMap = new OptionMap(list.Count, settings);
		foreach (Pair<PropertyInfo, BaseOptionAttribute> item in list)
		{
			if (item.Left != null && item.Right != null)
			{
				string text;
				if (item.Right.AutoLongName)
				{
					text = item.Left.Name.ToLowerInvariant();
					item.Right.LongName = text;
				}
				else
				{
					text = item.Right.UniqueName;
				}
				optionMap[text] = new OptionInfo(item.Right, item.Left, settings.ParsingCulture);
			}
		}
		optionMap.RawOptions = target;
		return optionMap;
	}

	public static OptionMap Create(object target, IList<Pair<PropertyInfo, VerbOptionAttribute>> verbs, ParserSettings settings)
	{
		OptionMap optionMap = new OptionMap(verbs.Count, settings);
		foreach (Pair<PropertyInfo, VerbOptionAttribute> verb in verbs)
		{
			OptionInfo optionInfo = new OptionInfo(verb.Right, verb.Left, settings.ParsingCulture);
			optionInfo.HasParameterLessCtor = verb.Left.PropertyType.GetConstructor(Type.EmptyTypes) != null;
			OptionInfo optionInfo2 = optionInfo;
			if (!optionInfo2.HasParameterLessCtor && verb.Left.GetValue(target, null) == null)
			{
				throw new ParserException("Type {0} must have a parameterless constructor or" + " be already initialized to be used as a verb command.".FormatInvariant(verb.Left.PropertyType));
			}
			optionMap[verb.Right.UniqueName] = optionInfo2;
		}
		optionMap.RawOptions = target;
		return optionMap;
	}

	public bool EnforceRules()
	{
		if (EnforceMutuallyExclusiveMap())
		{
			return EnforceRequiredRule();
		}
		return false;
	}

	public void SetDefaults()
	{
		foreach (OptionInfo value in _map.Values)
		{
			value.SetDefault(RawOptions);
		}
	}

	private static void SetParserStateIfNeeded(object options, OptionInfo option, bool? required, bool? mutualExclusiveness)
	{
		IList<Pair<PropertyInfo, ParserStateAttribute>> list = ReflectionHelper.RetrievePropertyList<ParserStateAttribute>(options);
		if (list.Count == 0)
		{
			return;
		}
		PropertyInfo left = list[0].Left;
		if (left.GetValue(options, null) == null)
		{
			left.SetValue(options, new ParserState(), null);
		}
		IParserState parserState = (IParserState)left.GetValue(options, null);
		if (parserState != null)
		{
			ParsingError parsingError = new ParsingError();
			parsingError.BadOption.ShortName = option.ShortName;
			parsingError.BadOption.LongName = option.LongName;
			ParsingError parsingError2 = parsingError;
			if (required.HasValue)
			{
				parsingError2.ViolatesRequired = required.Value;
			}
			if (mutualExclusiveness.HasValue)
			{
				parsingError2.ViolatesMutualExclusiveness = mutualExclusiveness.Value;
			}
			parserState.Errors.Add(parsingError2);
		}
	}

	private bool EnforceRequiredRule()
	{
		bool result = true;
		foreach (OptionInfo value in _map.Values)
		{
			if (value.Required && (!value.IsDefined || !value.ReceivedValue))
			{
				SetParserStateIfNeeded(RawOptions, value, true, null);
				result = false;
			}
		}
		return result;
	}

	private bool EnforceMutuallyExclusiveMap()
	{
		if (!_settings.MutuallyExclusive)
		{
			return true;
		}
		foreach (OptionInfo value in _map.Values)
		{
			if (value.IsDefined && value.MutuallyExclusiveSet != null)
			{
				BuildMutuallyExclusiveMap(value);
			}
		}
		foreach (MutuallyExclusiveInfo value2 in _mutuallyExclusiveSetMap.Values)
		{
			if (value2.Occurrence > 1)
			{
				SetParserStateIfNeeded(RawOptions, value2.BadOption, null, true);
				return false;
			}
		}
		return true;
	}

	private void BuildMutuallyExclusiveMap(OptionInfo option)
	{
		string mutuallyExclusiveSet = option.MutuallyExclusiveSet;
		if (!_mutuallyExclusiveSetMap.ContainsKey(mutuallyExclusiveSet))
		{
			_mutuallyExclusiveSetMap.Add(mutuallyExclusiveSet, new MutuallyExclusiveInfo(option));
		}
		_mutuallyExclusiveSetMap[mutuallyExclusiveSet].IncrementOccurrence();
	}
}
