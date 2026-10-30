namespace CommandLine;

public sealed class BadOptionInfo
{
	public char? ShortName { get; internal set; }

	public string LongName { get; internal set; }

	internal BadOptionInfo()
	{
	}

	internal BadOptionInfo(char? shortName, string longName)
	{
		ShortName = shortName;
		LongName = longName;
	}
}
