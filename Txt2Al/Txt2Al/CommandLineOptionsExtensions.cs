using System.IO;
using Microsoft.Dynamics.Nav.Txt2AlConverter;

namespace Txt2Al;

internal static class CommandLineOptionsExtensions
{
	public static void Sanitize(this AlExporterCommandLineOptions options)
	{
		options.SourceDirectory = Path.GetFullPath(NormalizeDirectoryPath(options.SourceDirectory));
		options.TargetDirectory = Path.GetFullPath(NormalizeDirectoryPath(options.TargetDirectory));
	}

	public static void Validate(this AlExporterCommandLineOptions options)
	{
		if (!Directory.Exists(options.SourceDirectory))
		{
			throw new AlExporterException("Source directory does not exist.");
		}
		if (!string.IsNullOrEmpty(options.DotNetAddInsPackage) && !File.Exists(options.DotNetAddInsPackage))
		{
			throw new AlExporterException("The file containing the .NET package does not exist.");
		}
	}

	private static string NormalizeDirectoryPath(string directory)
	{
		string text = directory;
		if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString()))
		{
			text += Path.DirectorySeparatorChar;
		}
		return text;
	}
}
