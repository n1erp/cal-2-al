using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using CommandLine.Infrastructure;

namespace CommandLine.Text;

public class CopyrightInfo
{
	private const string DefaultCopyrightWord = "Copyright";

	private const string SymbolLower = "(c)";

	private const string SymbolUpper = "(C)";

	private readonly AssemblyCopyrightAttribute _attribute;

	private readonly bool _isSymbolUpper;

	private readonly int[] _copyrightYears;

	private readonly string _author;

	private readonly int _builderSize;

	public static CopyrightInfo Default
	{
		get
		{
			AssemblyCopyrightAttribute attribute = ReflectionHelper.GetAttribute<AssemblyCopyrightAttribute>();
			if (attribute != null)
			{
				return new CopyrightInfo(attribute);
			}
			AssemblyCompanyAttribute attribute2 = ReflectionHelper.GetAttribute<AssemblyCompanyAttribute>();
			if (attribute2 != null)
			{
				return new CopyrightInfo(attribute2.Company, DateTime.Now.Year);
			}
			throw new InvalidOperationException("CopyrightInfo::Default requires that you define AssemblyCopyrightAttribute or AssemblyCompanyAttribute.");
		}
	}

	protected virtual string CopyrightWord => "Copyright";

	public CopyrightInfo(string author, int year)
		: this(true, author, year)
	{
	}

	public CopyrightInfo(string author, params int[] years)
		: this(isSymbolUpper: true, author, years)
	{
	}

	public CopyrightInfo(bool isSymbolUpper, string author, params int[] copyrightYears)
	{
		Assumes.NotNullOrEmpty(author, "author");
		Assumes.NotZeroLength(copyrightYears, "copyrightYears");
		_isSymbolUpper = isSymbolUpper;
		_author = author;
		_copyrightYears = copyrightYears;
		_builderSize = 12 + author.Length + 4 * copyrightYears.Length + 10;
	}

	protected CopyrightInfo()
	{
	}

	private CopyrightInfo(AssemblyCopyrightAttribute attribute)
	{
		_attribute = attribute;
	}

	public static implicit operator string(CopyrightInfo info)
	{
		return info.ToString();
	}

	public override string ToString()
	{
		if (_attribute != null)
		{
			return _attribute.Copyright;
		}
		StringBuilder stringBuilder = new StringBuilder(_builderSize);
		stringBuilder.Append(CopyrightWord);
		stringBuilder.Append(' ');
		stringBuilder.Append(_isSymbolUpper ? "(C)" : "(c)");
		stringBuilder.Append(' ');
		stringBuilder.Append(FormatYears(_copyrightYears));
		stringBuilder.Append(' ');
		stringBuilder.Append(_author);
		return stringBuilder.ToString();
	}

	protected virtual string FormatYears(int[] years)
	{
		if (years.Length == 1)
		{
			return years[0].ToString(CultureInfo.InvariantCulture);
		}
		StringBuilder stringBuilder = new StringBuilder(years.Length * 6);
		for (int i = 0; i < years.Length; i++)
		{
			stringBuilder.Append(years[i].ToString(CultureInfo.InvariantCulture));
			int num = i + 1;
			if (num < years.Length)
			{
				stringBuilder.Append((years[num] - years[i] > 1) ? " - " : ", ");
			}
		}
		return stringBuilder.ToString();
	}
}
