using System.IO;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public static class FileNameHelper
{
	public static string SanitizeFileName(string fileName)
	{
		return string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));
	}
}
