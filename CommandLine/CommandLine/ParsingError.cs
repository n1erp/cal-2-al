namespace CommandLine;

public sealed class ParsingError
{
	public BadOptionInfo BadOption { get; private set; }

	public bool ViolatesRequired { get; set; }

	public bool ViolatesFormat { get; set; }

	public bool ViolatesMutualExclusiveness { get; set; }

	internal ParsingError()
	{
		BadOption = new BadOptionInfo();
	}

	internal ParsingError(char? shortName, string longName, bool format)
	{
		BadOption = new BadOptionInfo(shortName, longName);
		ViolatesFormat = format;
	}
}
