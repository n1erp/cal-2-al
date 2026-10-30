using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CommandLine.Extensions;
using CommandLine.Infrastructure;

namespace CommandLine.Text;

public class HelpText
{
	private const int BuilderCapacity = 128;

	private const int DefaultMaximumLength = 80;

	private const string DefaultRequiredWord = "Required.";

	private readonly StringBuilder _preOptionsHelp;

	private readonly StringBuilder _postOptionsHelp;

	private readonly BaseSentenceBuilder _sentenceBuilder;

	private int? _maximumDisplayWidth;

	private string _heading;

	private string _copyright;

	private bool _additionalNewLineAfterOption;

	private StringBuilder _optionsHelp;

	private bool _addDashesToOption;

	public string Heading
	{
		get
		{
			return _heading;
		}
		set
		{
			Assumes.NotNullOrEmpty(value, "value");
			_heading = value;
		}
	}

	public string Copyright
	{
		get
		{
			return _heading;
		}
		set
		{
			Assumes.NotNullOrEmpty(value, "value");
			_copyright = value;
		}
	}

	public int MaximumDisplayWidth
	{
		get
		{
			if (!_maximumDisplayWidth.HasValue)
			{
				return 80;
			}
			return _maximumDisplayWidth.Value;
		}
		set
		{
			_maximumDisplayWidth = value;
		}
	}

	public bool AddDashesToOption
	{
		get
		{
			return _addDashesToOption;
		}
		set
		{
			_addDashesToOption = value;
		}
	}

	public bool AdditionalNewLineAfterOption
	{
		get
		{
			return _additionalNewLineAfterOption;
		}
		set
		{
			_additionalNewLineAfterOption = value;
		}
	}

	public BaseSentenceBuilder SentenceBuilder => _sentenceBuilder;

	public event EventHandler<FormatOptionHelpTextEventArgs> FormatOptionHelpText;

	public HelpText()
	{
		_preOptionsHelp = new StringBuilder(128);
		_postOptionsHelp = new StringBuilder(128);
		_sentenceBuilder = BaseSentenceBuilder.CreateBuiltIn();
	}

	public HelpText(BaseSentenceBuilder sentenceBuilder)
		: this()
	{
		Assumes.NotNull(sentenceBuilder, "sentenceBuilder");
		_sentenceBuilder = sentenceBuilder;
	}

	public HelpText(string heading)
		: this()
	{
		Assumes.NotNullOrEmpty(heading, "heading");
		_heading = heading;
	}

	public HelpText(BaseSentenceBuilder sentenceBuilder, string heading)
		: this(heading)
	{
		Assumes.NotNull(sentenceBuilder, "sentenceBuilder");
		_sentenceBuilder = sentenceBuilder;
	}

	public HelpText(string heading, string copyright)
		: this()
	{
		Assumes.NotNullOrEmpty(heading, "heading");
		Assumes.NotNullOrEmpty(copyright, "copyright");
		_heading = heading;
		_copyright = copyright;
	}

	public HelpText(BaseSentenceBuilder sentenceBuilder, string heading, string copyright)
		: this(heading, copyright)
	{
		Assumes.NotNull(sentenceBuilder, "sentenceBuilder");
		_sentenceBuilder = sentenceBuilder;
	}

	public HelpText(string heading, string copyright, object options)
		: this()
	{
		Assumes.NotNullOrEmpty(heading, "heading");
		Assumes.NotNullOrEmpty(copyright, "copyright");
		Assumes.NotNull(options, "options");
		_heading = heading;
		_copyright = copyright;
		DoAddOptions(options, "Required.", MaximumDisplayWidth, fireEvent: false);
	}

	public HelpText(BaseSentenceBuilder sentenceBuilder, string heading, string copyright, object options)
		: this(heading, copyright, options)
	{
		Assumes.NotNull(sentenceBuilder, "sentenceBuilder");
		_sentenceBuilder = sentenceBuilder;
	}

	public static HelpText AutoBuild(object options)
	{
		return AutoBuild(options, null, false);
	}

	public static HelpText AutoBuild(object options, Action<HelpText> onError, bool verbsIndex = false)
	{
		HelpText helpText = new HelpText();
		helpText.Heading = HeadingInfo.Default;
		helpText.Copyright = CopyrightInfo.Default;
		helpText.AdditionalNewLineAfterOption = true;
		helpText.AddDashesToOption = !verbsIndex;
		HelpText helpText2 = helpText;
		if (onError != null)
		{
			IList<Pair<PropertyInfo, ParserStateAttribute>> list = ReflectionHelper.RetrievePropertyList<ParserStateAttribute>(options);
			if (list != null)
			{
				onError(helpText2);
			}
		}
		ReflectionHelper.GetAttribute<AssemblyLicenseAttribute>()?.AddToHelpText(helpText2, before: true);
		ReflectionHelper.GetAttribute<AssemblyUsageAttribute>()?.AddToHelpText(helpText2, before: true);
		helpText2.AddOptions(options);
		return helpText2;
	}

	public static HelpText AutoBuild(object options, string verb)
	{
		object obj = Parser.InternalGetVerbOptionsInstanceByName(verb, options, out var found);
		bool flag = verb == null || !found;
		object target = (flag ? options : obj);
		return AutoBuild(target, (HelpText current) =>
		{
			DefaultParsingErrorsHandler(target, current);
		}, flag);
	}

	public static void DefaultParsingErrorsHandler(object options, HelpText current)
	{
		IList<Pair<PropertyInfo, ParserStateAttribute>> list = ReflectionHelper.RetrievePropertyList<ParserStateAttribute>(options);
		if (list.Count == 0)
		{
			return;
		}
		IParserState parserState = (IParserState)list[0].Left.GetValue(options, null);
		if (parserState == null || parserState.Errors.Count == 0)
		{
			return;
		}
		string text = current.RenderParsingErrorsText(options, 2);
		if (!string.IsNullOrEmpty(text))
		{
			current.AddPreOptionsLine(Environment.NewLine + current.SentenceBuilder.ErrorsHeadingText);
			string[] array = text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
			string[] array2 = array;
			foreach (string value in array2)
			{
				current.AddPreOptionsLine(value);
			}
		}
	}

	public static implicit operator string(HelpText info)
	{
		return info.ToString();
	}

	public void AddPreOptionsLine(string value)
	{
		AddPreOptionsLine(value, MaximumDisplayWidth);
	}

	public void AddPostOptionsLine(string value)
	{
		AddLine(_postOptionsHelp, value);
	}

	public void AddOptions(object options)
	{
		AddOptions(options, "Required.");
	}

	public void AddOptions(object options, string requiredWord)
	{
		Assumes.NotNull(options, "options");
		Assumes.NotNullOrEmpty(requiredWord, "requiredWord");
		AddOptions(options, requiredWord, MaximumDisplayWidth);
	}

	public void AddOptions(object options, string requiredWord, int maximumLength)
	{
		Assumes.NotNull(options, "options");
		Assumes.NotNullOrEmpty(requiredWord, "requiredWord");
		DoAddOptions(options, requiredWord, maximumLength);
	}

	public string RenderParsingErrorsText(object options, int indent)
	{
		IList<Pair<PropertyInfo, ParserStateAttribute>> list = ReflectionHelper.RetrievePropertyList<ParserStateAttribute>(options);
		if (list.Count == 0)
		{
			return string.Empty;
		}
		IParserState parserState = (IParserState)list[0].Left.GetValue(options, null);
		if (parserState == null || parserState.Errors.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ParsingError error in parserState.Errors)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append(indent.Spaces());
			if (((int?)error.BadOption.ShortName).HasValue)
			{
				stringBuilder2.Append('-');
				stringBuilder2.Append(error.BadOption.ShortName);
				if (!string.IsNullOrEmpty(error.BadOption.LongName))
				{
					stringBuilder2.Append('/');
				}
			}
			if (!string.IsNullOrEmpty(error.BadOption.LongName))
			{
				stringBuilder2.Append("--");
				stringBuilder2.Append(error.BadOption.LongName);
			}
			stringBuilder2.Append(" ");
			stringBuilder2.Append(error.ViolatesRequired ? _sentenceBuilder.RequiredOptionMissingText : _sentenceBuilder.OptionWord);
			if (error.ViolatesFormat)
			{
				stringBuilder2.Append(" ");
				stringBuilder2.Append(_sentenceBuilder.ViolatesFormatText);
			}
			if (error.ViolatesMutualExclusiveness)
			{
				if (error.ViolatesFormat || error.ViolatesRequired)
				{
					stringBuilder2.Append(" ");
					stringBuilder2.Append(_sentenceBuilder.AndWord);
				}
				stringBuilder2.Append(" ");
				stringBuilder2.Append(_sentenceBuilder.ViolatesMutualExclusivenessText);
			}
			stringBuilder2.Append('.');
			stringBuilder.AppendLine(stringBuilder2.ToString());
		}
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(GetLength(_heading) + GetLength(_copyright) + GetLength(_preOptionsHelp) + GetLength(_optionsHelp) + 10);
		stringBuilder.Append(_heading);
		if (!string.IsNullOrEmpty(_copyright))
		{
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(_copyright);
		}
		if (_preOptionsHelp.Length > 0)
		{
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(_preOptionsHelp);
		}
		if (_optionsHelp != null && _optionsHelp.Length > 0)
		{
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(_optionsHelp);
		}
		if (_postOptionsHelp.Length > 0)
		{
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(_postOptionsHelp);
		}
		return stringBuilder.ToString();
	}

	protected virtual void OnFormatOptionHelpText(FormatOptionHelpTextEventArgs e)
	{
		FormatOptionHelpText?.Invoke(this, e);
	}

	private static int GetLength(string value)
	{
		return value?.Length ?? 0;
	}

	private static int GetLength(StringBuilder value)
	{
		return value?.Length ?? 0;
	}

	private static void AddLine(StringBuilder builder, string value, int maximumLength)
	{
		Assumes.NotNull(value, "value");
		if (builder.Length > 0)
		{
			builder.Append(Environment.NewLine);
		}
		do
		{
			int num = 0;
			string[] array = value.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Length < maximumLength - num)
				{
					builder.Append(array[i]);
					num += array[i].Length;
					if (maximumLength - num > 1 && i != array.Length - 1)
					{
						builder.Append(" ");
						num++;
					}
					continue;
				}
				if (array[i].Length >= maximumLength && num == 0)
				{
					builder.Append(array[i].Substring(0, maximumLength));
					num = maximumLength;
				}
				break;
			}
			value = value.Substring(Math.Min(num, value.Length));
			if (value.Length > 0)
			{
				builder.Append(Environment.NewLine);
			}
		}
		while (value.Length > maximumLength);
		builder.Append(value);
	}

	private void DoAddOptions(object options, string requiredWord, int maximumLength, bool fireEvent = true)
	{
		IList<BaseOptionAttribute> list = ReflectionHelper.RetrievePropertyAttributeList<BaseOptionAttribute>(options);
		HelpOptionAttribute helpOptionAttribute = ReflectionHelper.RetrieveMethodAttributeOnly<HelpOptionAttribute>(options);
		if (helpOptionAttribute != null)
		{
			list.Add(helpOptionAttribute);
		}
		if (list.Count == 0)
		{
			return;
		}
		int maxLength = GetMaxLength(list);
		_optionsHelp = new StringBuilder(128);
		int widthOfHelpText = maximumLength - (maxLength + 6);
		foreach (BaseOptionAttribute item in list)
		{
			AddOption(requiredWord, maxLength, item, widthOfHelpText, fireEvent);
		}
	}

	private void AddPreOptionsLine(string value, int maximumLength)
	{
		AddLine(_preOptionsHelp, value, maximumLength);
	}

	private void AddOption(string requiredWord, int maxLength, BaseOptionAttribute option, int widthOfHelpText, bool fireEvent = true)
	{
		_optionsHelp.Append("  ");
		StringBuilder stringBuilder = new StringBuilder(maxLength);
		if (option.HasShortName)
		{
			if (_addDashesToOption)
			{
				stringBuilder.Append('-');
			}
			stringBuilder.AppendFormat("{0}", option.ShortName);
			if (option.HasMetaValue)
			{
				stringBuilder.AppendFormat(" {0}", option.MetaValue);
			}
			if (option.HasLongName)
			{
				stringBuilder.Append(", ");
			}
		}
		if (option.HasLongName)
		{
			if (_addDashesToOption)
			{
				stringBuilder.Append("--");
			}
			stringBuilder.AppendFormat("{0}", option.LongName);
			if (option.HasMetaValue)
			{
				stringBuilder.AppendFormat("={0}", option.MetaValue);
			}
		}
		_optionsHelp.Append((stringBuilder.Length < maxLength) ? stringBuilder.ToString().PadRight(maxLength) : stringBuilder.ToString());
		_optionsHelp.Append("    ");
		if (option.HasDefaultValue)
		{
			option.HelpText = "(Default: {0}) ".FormatLocal(option.DefaultValue) + option.HelpText;
		}
		if (option.Required)
		{
			option.HelpText = "{0} ".FormatInvariant(requiredWord) + option.HelpText;
		}
		if (fireEvent)
		{
			FormatOptionHelpTextEventArgs e = new FormatOptionHelpTextEventArgs(option);
			OnFormatOptionHelpText(e);
			option.HelpText = e.Option.HelpText;
		}
		if (!string.IsNullOrEmpty(option.HelpText))
		{
			do
			{
				int num = 0;
				string[] array = option.HelpText.Split(' ');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Length < widthOfHelpText - num)
					{
						_optionsHelp.Append(array[i]);
						num += array[i].Length;
						if (widthOfHelpText - num > 1 && i != array.Length - 1)
						{
							_optionsHelp.Append(" ");
							num++;
						}
						continue;
					}
					if (array[i].Length >= widthOfHelpText && num == 0)
					{
						_optionsHelp.Append(array[i].Substring(0, widthOfHelpText));
						num = widthOfHelpText;
					}
					break;
				}
				option.HelpText = option.HelpText.Substring(Math.Min(num, option.HelpText.Length)).Trim();
				if (option.HelpText.Length > 0)
				{
					_optionsHelp.Append(Environment.NewLine);
					_optionsHelp.Append(new string(' ', maxLength + 6));
				}
			}
			while (option.HelpText.Length > widthOfHelpText);
		}
		_optionsHelp.Append(option.HelpText);
		_optionsHelp.Append(Environment.NewLine);
		if (_additionalNewLineAfterOption)
		{
			_optionsHelp.Append(Environment.NewLine);
		}
	}

	private void AddLine(StringBuilder builder, string value)
	{
		Assumes.NotNull(value, "value");
		AddLine(builder, value, MaximumDisplayWidth);
	}

	private int GetMaxLength(IEnumerable<BaseOptionAttribute> optionList)
	{
		int num = 0;
		foreach (BaseOptionAttribute option in optionList)
		{
			int num2 = 0;
			bool hasShortName = option.HasShortName;
			bool hasLongName = option.HasLongName;
			int num3 = 0;
			if (option.HasMetaValue)
			{
				num3 = option.MetaValue.Length + 1;
			}
			if (hasShortName)
			{
				num2++;
				if (AddDashesToOption)
				{
					num2++;
				}
				num2 += num3;
			}
			if (hasLongName)
			{
				num2 += option.LongName.Length;
				if (AddDashesToOption)
				{
					num2 += 2;
				}
				num2 += num3;
			}
			if (hasShortName && hasLongName)
			{
				num2 += 2;
			}
			num = Math.Max(num, num2);
		}
		return num;
	}
}
