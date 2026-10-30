using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine.Infrastructure;
using CommandLine.Parsing;
using CommandLine.Text;

namespace CommandLine;

public sealed class Parser : IDisposable
{
	public const int DefaultExitCodeFail = 1;

	private static readonly Parser DefaultParser = new Parser(singleton: true);

	private readonly ParserSettings _settings;

	private bool _disposed;

	public static Parser Default => DefaultParser;

	public ParserSettings Settings => _settings;

	public Parser()
	{
		_settings = new ParserSettings
		{
			Consumed = true
		};
	}

	[Obsolete("Use constructor that accepts Action<ParserSettings>.")]
	public Parser(ParserSettings settings)
	{
		Assumes.NotNull(settings, "settings", "The command line parser settings instance cannot be null.");
		if (settings.Consumed)
		{
			throw new InvalidOperationException("The command line parserSettings instance cannnot be used more than once.");
		}
		_settings = settings;
		_settings.Consumed = true;
	}

	public Parser(Action<ParserSettings> configuration)
	{
		Assumes.NotNull(configuration, "configuration", "The command line parser settings delegate cannot be null.");
		_settings = new ParserSettings();
		configuration(Settings);
		_settings.Consumed = true;
	}

	private Parser(bool singleton)
		: this((ParserSettings with) =>
		{
			with.CaseSensitive = false;
			with.MutuallyExclusive = false;
			with.HelpWriter = Console.Error;
			with.ParsingCulture = CultureInfo.InvariantCulture;
		})
	{
	}

	~Parser()
	{
		Dispose(disposing: false);
	}

	public bool ParseArguments(string[] args, object options)
	{
		Assumes.NotNull(args, "args", "The arguments string array cannot be null.");
		Assumes.NotNull(options, "options", "The target options instance cannot be null.");
		return DoParseArguments(args, options);
	}

	public bool ParseArguments(string[] args, object options, Action<string, object> onVerbCommand)
	{
		Assumes.NotNull(args, "args", "The arguments string array cannot be null.");
		Assumes.NotNull(options, "options", "The target options instance cannot be null.");
		Assumes.NotNull(options, "onVerbCommand", "Delegate executed to capture verb command instance reference cannot be null.");
		object verbInstance = null;
		bool flag = DoParseArgumentsVerbs(args, options, ref verbInstance);
		onVerbCommand(args.FirstOrDefault() ?? string.Empty, flag ? verbInstance : null);
		return flag;
	}

	public bool ParseArgumentsStrict(string[] args, object options, Action onFail = null)
	{
		Assumes.NotNull(args, "args", "The arguments string array cannot be null.");
		Assumes.NotNull(options, "options", "The target options instance cannot be null.");
		if (!DoParseArguments(args, options))
		{
			InvokeAutoBuildIfNeeded(options);
			if (onFail == null)
			{
				Environment.Exit(1);
			}
			else
			{
				onFail();
			}
			return false;
		}
		return true;
	}

	public bool ParseArgumentsStrict(string[] args, object options, Action<string, object> onVerbCommand, Action onFail = null)
	{
		Assumes.NotNull(args, "args", "The arguments string array cannot be null.");
		Assumes.NotNull(options, "options", "The target options instance cannot be null.");
		Assumes.NotNull(options, "onVerbCommand", "Delegate executed to capture verb command instance reference cannot be null.");
		object verbInstance = null;
		if (!DoParseArgumentsVerbs(args, options, ref verbInstance))
		{
			onVerbCommand(args.FirstOrDefault() ?? string.Empty, null);
			InvokeAutoBuildIfNeeded(options);
			if (onFail == null)
			{
				Environment.Exit(1);
			}
			else
			{
				onFail();
			}
			return false;
		}
		onVerbCommand(args.FirstOrDefault() ?? string.Empty, verbInstance);
		return true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal static object InternalGetVerbOptionsInstanceByName(string verb, object target, out bool found)
	{
		found = false;
		if (string.IsNullOrEmpty(verb))
		{
			return target;
		}
		Pair<PropertyInfo, VerbOptionAttribute> pair = ReflectionHelper.RetrieveOptionProperty<VerbOptionAttribute>(target, verb);
		found = pair != null;
		if (!found)
		{
			return target;
		}
		return pair.Left.GetValue(target, null);
	}

	private static void SetParserStateIfNeeded(object options, IEnumerable<ParsingError> errors)
	{
		if (!options.CanReceiveParserState())
		{
			return;
		}
		PropertyInfo left = ReflectionHelper.RetrievePropertyList<ParserStateAttribute>(options)[0].Left;
		object value = left.GetValue(options, null);
		if (value != null)
		{
			if (!(value is IParserState))
			{
				throw new InvalidOperationException("Cannot apply ParserStateAttribute to a property that does not implement IParserState or is not accessible.");
			}
			if (!(value is ParserState))
			{
				throw new InvalidOperationException("ParserState instance cannot be supplied.");
			}
		}
		else
		{
			try
			{
				left.SetValue(options, new ParserState(), null);
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException("Cannot apply ParserStateAttribute to a property that does not implement IParserState or is not accessible.", innerException);
			}
		}
		IParserState parserState = (IParserState)left.GetValue(options, null);
		foreach (ParsingError error in errors)
		{
			parserState.Errors.Add(error);
		}
	}

	private static StringComparison GetStringComparison(ParserSettings settings)
	{
		if (!settings.CaseSensitive)
		{
			return StringComparison.OrdinalIgnoreCase;
		}
		return StringComparison.Ordinal;
	}

	private bool DoParseArguments(string[] args, object options)
	{
		Pair<MethodInfo, HelpOptionAttribute> pair = ReflectionHelper.RetrieveMethod<HelpOptionAttribute>(options);
		TextWriter helpWriter = _settings.HelpWriter;
		if (pair != null && helpWriter != null)
		{
			if (ParseHelp(args, pair.Right) || !DoParseArgumentsCore(args, options))
			{
				HelpOptionAttribute.InvokeMethod(options, pair, out var text);
				helpWriter.Write(text);
				return false;
			}
			return true;
		}
		return DoParseArgumentsCore(args, options);
	}

	private bool DoParseArgumentsCore(string[] args, object options)
	{
		bool flag = false;
		OptionMap optionMap = OptionMap.Create(options, _settings);
		optionMap.SetDefaults();
		ValueMapper valueMapper = new ValueMapper(options, _settings.ParsingCulture);
		StringArrayEnumerator stringArrayEnumerator = new StringArrayEnumerator(args);
		while (stringArrayEnumerator.MoveNext())
		{
			string current = stringArrayEnumerator.Current;
			if (string.IsNullOrEmpty(current))
			{
				continue;
			}
			ArgumentParser argumentParser = ArgumentParser.Create(current, _settings.IgnoreUnknownArguments);
			if (argumentParser != null)
			{
				PresentParserState presentParserState = argumentParser.Parse(stringArrayEnumerator, optionMap, options);
				if ((presentParserState & PresentParserState.Failure) == PresentParserState.Failure)
				{
					SetParserStateIfNeeded(options, argumentParser.PostParsingState);
					flag = true;
				}
				else if ((presentParserState & PresentParserState.MoveOnNextElement) == PresentParserState.MoveOnNextElement)
				{
					stringArrayEnumerator.MoveNext();
				}
			}
			else if (valueMapper.CanReceiveValues && !valueMapper.MapValueItem(current))
			{
				flag = true;
			}
		}
		flag |= !optionMap.EnforceRules();
		return !flag;
	}

	private bool DoParseArgumentsVerbs(string[] args, object options, ref object verbInstance)
	{
		IList<Pair<PropertyInfo, VerbOptionAttribute>> verbs = ReflectionHelper.RetrievePropertyList<VerbOptionAttribute>(options);
		Pair<MethodInfo, HelpVerbOptionAttribute> pair = ReflectionHelper.RetrieveMethod<HelpVerbOptionAttribute>(options);
		if (args.Length == 0)
		{
			if (pair != null || _settings.HelpWriter != null)
			{
				DisplayHelpVerbText(options, pair, null);
			}
			return false;
		}
		OptionMap optionMap = OptionMap.Create(options, verbs, _settings);
		if (TryParseHelpVerb(args, options, pair, optionMap))
		{
			return false;
		}
		OptionInfo optionInfo = optionMap[args.First()];
		if (optionInfo == null)
		{
			if (pair != null)
			{
				DisplayHelpVerbText(options, pair, null);
			}
			return false;
		}
		verbInstance = optionInfo.GetValue(options);
		if (verbInstance == null)
		{
			verbInstance = optionInfo.CreateInstance(options);
		}
		bool flag = DoParseArgumentsCore(args.Skip(1).ToArray(), verbInstance);
		if (!flag && pair != null)
		{
			DisplayHelpVerbText(options, pair, args.First());
		}
		return flag;
	}

	private bool ParseHelp(string[] args, HelpOptionAttribute helpOption)
	{
		bool caseSensitive = _settings.CaseSensitive;
		foreach (string argument in args)
		{
			if (((int?)helpOption.ShortName).HasValue && ArgumentParser.CompareShort(argument, helpOption.ShortName, caseSensitive))
			{
				return true;
			}
			if (!string.IsNullOrEmpty(helpOption.LongName) && ArgumentParser.CompareLong(argument, helpOption.LongName, caseSensitive))
			{
				return true;
			}
		}
		return false;
	}

	private bool TryParseHelpVerb(string[] args, object options, Pair<MethodInfo, HelpVerbOptionAttribute> helpInfo, OptionMap optionMap)
	{
		TextWriter helpWriter = _settings.HelpWriter;
		if (helpInfo != null && helpWriter != null && string.Compare(args[0], helpInfo.Right.LongName, GetStringComparison(_settings)) == 0)
		{
			string text = args.FirstOrDefault();
			if (text != null)
			{
				OptionInfo optionInfo = optionMap[text];
				if (optionInfo != null && optionInfo.GetValue(options) == null)
				{
					optionInfo.CreateInstance(options);
				}
			}
			DisplayHelpVerbText(options, helpInfo, text);
			return true;
		}
		return false;
	}

	private void DisplayHelpVerbText(object options, Pair<MethodInfo, HelpVerbOptionAttribute> helpInfo, string verb)
	{
		string text;
		if (verb == null)
		{
			HelpVerbOptionAttribute.InvokeMethod(options, helpInfo, null, out text);
		}
		else
		{
			HelpVerbOptionAttribute.InvokeMethod(options, helpInfo, verb, out text);
		}
		if (_settings.HelpWriter != null)
		{
			_settings.HelpWriter.Write(text);
		}
	}

	private void InvokeAutoBuildIfNeeded(object options)
	{
		if (_settings.HelpWriter != null && !options.HasHelp() && !options.HasVerbHelp())
		{
			_settings.HelpWriter.Write(HelpText.AutoBuild(options, (HelpText current) =>
			{
				HelpText.DefaultParsingErrorsHandler(options, current);
			}, options.HasVerbs()));
		}
	}

	private void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			if (_settings != null)
			{
				_settings.Dispose();
			}
			_disposed = true;
		}
	}
}
