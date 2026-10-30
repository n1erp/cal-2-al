using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public class FileNameExporter : BaseFileNameExporter
{
	private readonly string sourceDirectory;

	private readonly string targetDirectory;

	public FileNameExporter(AlExportOptions exportOptions, string extensionObjectFileNamePattern, string objectFileNamePattern, string sourceDirectory, string targetDirectory)
		: base(exportOptions, extensionObjectFileNamePattern, objectFileNamePattern)
	{
		this.sourceDirectory = sourceDirectory;
		this.targetDirectory = targetDirectory;
	}

	public string GetPathToResourceFile(string sourceTxtFilePath, string newFileName)
	{
		Uri uri = new Uri(sourceTxtFilePath, UriKind.Absolute);
		string directoryName = Path.GetDirectoryName(new Uri(sourceDirectory, UriKind.Absolute).MakeRelativeUri(uri).ToString());
		string text = Path.Combine(targetDirectory, directoryName);
		Directory.CreateDirectory(text);
		newFileName = FileNameHelper.SanitizeFileName(newFileName);
		return Path.Combine(text, newFileName);
	}
}
