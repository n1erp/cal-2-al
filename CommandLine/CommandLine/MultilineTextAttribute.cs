using System;
using System.Text;
using CommandLine.Infrastructure;
using CommandLine.Text;

namespace CommandLine;

public abstract class MultilineTextAttribute : Attribute
{
	private readonly string _line1;

	private readonly string _line2;

	private readonly string _line3;

	private readonly string _line4;

	private readonly string _line5;

	public virtual string Value
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(string.Empty);
			string[] array = new string[5] { _line1, _line2, _line3, _line4, _line5 };
			for (int i = 0; i < GetLastLineWithText(array); i++)
			{
				stringBuilder.AppendLine(array[i]);
			}
			return stringBuilder.ToString();
		}
	}

	public string Line1 => _line1;

	public string Line2 => _line2;

	public string Line3 => _line3;

	public string Line4 => _line4;

	public string Line5 => _line5;

	protected MultilineTextAttribute(string line1)
	{
		Assumes.NotNullOrEmpty(line1, "line1");
		_line1 = line1;
	}

	protected MultilineTextAttribute(string line1, string line2)
		: this(line1)
	{
		Assumes.NotNullOrEmpty(line2, "line2");
		_line2 = line2;
	}

	protected MultilineTextAttribute(string line1, string line2, string line3)
		: this(line1, line2)
	{
		Assumes.NotNullOrEmpty(line3, "line3");
		_line3 = line3;
	}

	protected MultilineTextAttribute(string line1, string line2, string line3, string line4)
		: this(line1, line2, line3)
	{
		Assumes.NotNullOrEmpty(line4, "line4");
		_line4 = line4;
	}

	protected MultilineTextAttribute(string line1, string line2, string line3, string line4, string line5)
		: this(line1, line2, line3, line4)
	{
		Assumes.NotNullOrEmpty(line5, "line5");
		_line5 = line5;
	}

	internal void AddToHelpText(Action<string> action)
	{
		string[] array = new string[5] { _line1, _line2, _line3, _line4, _line5 };
		Array.ForEach(array, (string line) =>
		{
			if (!string.IsNullOrEmpty(line))
			{
				action(line);
			}
		});
	}

	internal void AddToHelpText(HelpText helpText, bool before)
	{
		if (before)
		{
			AddToHelpText(helpText.AddPreOptionsLine);
		}
		else
		{
			AddToHelpText(helpText.AddPostOptionsLine);
		}
	}

	protected virtual int GetLastLineWithText(string[] value)
	{
		int num = Array.FindLastIndex(value, (string str) => !string.IsNullOrEmpty(str));
		return num + 1;
	}
}
