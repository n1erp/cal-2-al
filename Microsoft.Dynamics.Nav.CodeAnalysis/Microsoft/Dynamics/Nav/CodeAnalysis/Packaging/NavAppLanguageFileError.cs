namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppLanguageFileError
{
	public string RawLine { get; }

	public string FileName { get; }

	public int LineNumber { get; }

	public NavAppLanguageFileError(string fileName, int lineNumber, string rawLine)
	{
		FileName = fileName;
		LineNumber = lineNumber;
		RawLine = rawLine;
	}
}
