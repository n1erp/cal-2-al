using System;
using System.Globalization;
using System.IO;
using System.Threading;
using CommandLine.Infrastructure;

namespace CommandLine;

public sealed class ParserSettings
{
	private const bool CaseSensitiveDefault = true;

	private bool _disposed;

	private bool _caseSensitive;

	private bool _mutuallyExclusive;

	private bool _ignoreUnknownArguments;

	private TextWriter _helpWriter;

	private CultureInfo _parsingCulture;

	public bool CaseSensitive
	{
		get
		{
			return _caseSensitive;
		}
		set
		{
			PopsicleSetter.Set(Consumed, ref _caseSensitive, value);
		}
	}

	public bool MutuallyExclusive
	{
		get
		{
			return _mutuallyExclusive;
		}
		set
		{
			PopsicleSetter.Set(Consumed, ref _mutuallyExclusive, value);
		}
	}

	public TextWriter HelpWriter
	{
		get
		{
			return _helpWriter;
		}
		set
		{
			PopsicleSetter.Set(Consumed, ref _helpWriter, value);
		}
	}

	public bool IgnoreUnknownArguments
	{
		get
		{
			return _ignoreUnknownArguments;
		}
		set
		{
			PopsicleSetter.Set(Consumed, ref _ignoreUnknownArguments, value);
		}
	}

	public CultureInfo ParsingCulture
	{
		get
		{
			return _parsingCulture;
		}
		set
		{
			PopsicleSetter.Set(Consumed, ref _parsingCulture, value);
		}
	}

	internal bool Consumed { get; set; }

	public ParserSettings()
		: this(caseSensitive: true, mutuallyExclusive: false, ignoreUnknownArguments: false, null)
	{
	}

	public ParserSettings(bool caseSensitive)
		: this(caseSensitive, mutuallyExclusive: false, ignoreUnknownArguments: false, null)
	{
	}

	public ParserSettings(TextWriter helpWriter)
		: this(caseSensitive: true, mutuallyExclusive: false, ignoreUnknownArguments: false, helpWriter)
	{
	}

	public ParserSettings(bool caseSensitive, TextWriter helpWriter)
		: this(caseSensitive, mutuallyExclusive: false, ignoreUnknownArguments: false, helpWriter)
	{
	}

	public ParserSettings(bool caseSensitive, bool mutuallyExclusive)
		: this(caseSensitive, mutuallyExclusive, ignoreUnknownArguments: false, null)
	{
	}

	public ParserSettings(bool caseSensitive, bool mutuallyExclusive, TextWriter helpWriter)
		: this(caseSensitive, mutuallyExclusive, ignoreUnknownArguments: false, helpWriter)
	{
	}

	public ParserSettings(bool caseSensitive, bool mutuallyExclusive, bool ignoreUnknownArguments, TextWriter helpWriter)
	{
		CaseSensitive = caseSensitive;
		MutuallyExclusive = mutuallyExclusive;
		HelpWriter = helpWriter;
		IgnoreUnknownArguments = ignoreUnknownArguments;
		ParsingCulture = Thread.CurrentThread.CurrentCulture;
	}

	~ParserSettings()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			if (_helpWriter != null)
			{
				_helpWriter.Dispose();
				_helpWriter = null;
			}
			_disposed = true;
		}
	}
}
