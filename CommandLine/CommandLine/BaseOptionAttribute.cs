using System;
using CommandLine.Extensions;

namespace CommandLine;

public abstract class BaseOptionAttribute : Attribute
{
	internal const string DefaultMutuallyExclusiveSet = "Default";

	private char? _shortName;

	private object _defaultValue;

	private string _metaValue;

	private bool _hasMetaValue;

	private string _mutuallyExclusiveSet;

	public virtual char? ShortName
	{
		get
		{
			return _shortName;
		}
		internal set
		{
			_shortName = value;
		}
	}

	public string LongName { get; internal set; }

	public string MutuallyExclusiveSet
	{
		get
		{
			return _mutuallyExclusiveSet;
		}
		set
		{
			_mutuallyExclusiveSet = (string.IsNullOrEmpty(value) ? "Default" : value);
		}
	}

	public virtual bool Required { get; set; }

	public virtual object DefaultValue
	{
		get
		{
			return _defaultValue;
		}
		set
		{
			_defaultValue = value;
			HasDefaultValue = true;
		}
	}

	public virtual string MetaValue
	{
		get
		{
			return _metaValue;
		}
		set
		{
			_metaValue = value;
			_hasMetaValue = !string.IsNullOrEmpty(_metaValue);
		}
	}

	public string HelpText { get; set; }

	internal string UniqueName { get; private set; }

	internal bool HasShortName => ((int?)_shortName).HasValue;

	internal bool HasLongName => !string.IsNullOrEmpty(LongName);

	internal bool HasDefaultValue { get; private set; }

	internal bool HasMetaValue => _hasMetaValue;

	internal bool AutoLongName { get; set; }

	protected BaseOptionAttribute()
	{
	}

	protected BaseOptionAttribute(char shortName, string longName)
	{
		_shortName = shortName;
		if (_shortName.Value.IsWhiteSpace() || _shortName.Value.IsLineTerminator())
		{
			throw new ArgumentException("shortName with whitespace or line terminator character is not allowed.", "shortName");
		}
		UniqueName = new string(shortName, 1);
		LongName = longName;
	}

	protected BaseOptionAttribute(char? shortName, string longName)
	{
		_shortName = shortName;
		if (((int?)_shortName).HasValue)
		{
			if (_shortName.Value.IsWhiteSpace() || _shortName.Value.IsLineTerminator())
			{
				throw new ArgumentException("shortName with whitespace or line terminator character is not allowed.", "shortName");
			}
			UniqueName = new string(_shortName.Value, 1);
		}
		LongName = longName;
		if (UniqueName == null)
		{
			if (LongName == null)
			{
				throw new ArgumentNullException("longName", "The option must have short, long name or both.");
			}
			UniqueName = LongName;
		}
	}
}
