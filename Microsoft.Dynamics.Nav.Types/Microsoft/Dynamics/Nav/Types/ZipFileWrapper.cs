using System.IO;
using System.IO.Compression;

namespace Microsoft.Dynamics.Nav.Types;

public class ZipFileWrapper
{
	public virtual void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel compressionLevel, bool includeBaseDirectory)
	{
		ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, compressionLevel, includeBaseDirectory);
	}

	public virtual void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectory)
	{
		using ZipArchive zipArchive = ZipFile.OpenRead(sourceArchiveFileName);
		foreach (ZipArchiveEntry entry in zipArchive.Entries)
		{
			string text = Path.Combine(destinationDirectory, entry.FullName);
			if (!File.Exists(text))
			{
				string directoryName = Path.GetDirectoryName(text);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				if (!string.IsNullOrEmpty(Path.GetFileName(text)))
				{
					entry.ExtractToFile(text);
				}
			}
		}
	}
}
