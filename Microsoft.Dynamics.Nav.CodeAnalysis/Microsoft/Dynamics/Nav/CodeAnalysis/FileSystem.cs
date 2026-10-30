using System.Collections.Generic;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class FileSystem : IFileSystem
{
	public static FileSystem Instance { get; } = new FileSystem();

	public void CreateDirectoryForFile(string path)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
	}

	public Stream CreateFile(string path)
	{
		return File.Create(path);
	}

	public void DeleteFile(string path)
	{
		File.Delete(path);
	}

	public bool Exists(string path)
	{
		return File.Exists(path);
	}

	public bool DirectoryExistsForFile(string path)
	{
		return Directory.Exists(Path.GetDirectoryName(path));
	}

	public IEnumerable<string> GetFiles(string searchPattern)
	{
		return Directory.GetFiles(Directory.GetCurrentDirectory(), searchPattern);
	}

	public string GetDirectoryPath()
	{
		return Directory.GetCurrentDirectory();
	}

	public Stream OpenRead(string path)
	{
		return File.OpenRead(path);
	}

	public Stream OpenWrite(string path)
	{
		return File.OpenWrite(path);
	}

	public Stream OpenFile(string filePath, FileMode mode, FileAccess access, FileShare share = FileShare.None, int bufferSize = 4096, FileOptions options = FileOptions.None)
	{
		return new FileStream(filePath, mode, access, share, bufferSize, options);
	}

	public byte[] ReadBytes(string path)
	{
		return File.ReadAllBytes(path);
	}

	public byte[] ReadBytes(string path, int count)
	{
		return FileUtilities.ReadBytes(path, count);
	}

	public void WriteBytes(string path, byte[] content)
	{
		File.WriteAllBytes(path, content);
	}
}
