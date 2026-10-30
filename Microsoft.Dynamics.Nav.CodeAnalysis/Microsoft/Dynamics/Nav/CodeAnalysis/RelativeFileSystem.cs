using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class RelativeFileSystem : IFileSystem
{
	public string Root { get; }

	public RelativeFileSystem(string root)
	{
		Root = root;
	}

	public byte[] ReadBytes(string path)
	{
		return File.ReadAllBytes(GetAbsolutePath(path));
	}

	public byte[] ReadBytes(string path, int count)
	{
		return FileUtilities.ReadBytes(GetAbsolutePath(path), count);
	}

	public void WriteBytes(string path, byte[] content)
	{
		File.WriteAllBytes(GetAbsolutePath(path), content);
	}

	public bool Exists(string path)
	{
		return File.Exists(GetAbsolutePath(path));
	}

	public bool DirectoryExistsForFile(string path)
	{
		return Directory.Exists(Path.GetDirectoryName(GetAbsolutePath(path)));
	}

	private string GetAbsolutePath(string relativePath)
	{
		try
		{
			return Path.Combine(Root, relativePath);
		}
		catch (ArgumentException)
		{
			return null;
		}
	}

	public IEnumerable<string> GetFiles(string searchPattern)
	{
		if (Root != null)
		{
			if (searchPattern.Length > 0 && (searchPattern[0] == '/' || searchPattern[0] == '\\'))
			{
				searchPattern = searchPattern.Substring(1);
			}
			string path = Path.Combine(Root, Path.GetDirectoryName(searchPattern) ?? string.Empty);
			int startIndex = searchPattern.LastIndexOfAny(new char[2] { '/', '\\' }) + 1;
			string searchPattern2 = searchPattern.Substring(startIndex);
			if (Directory.Exists(path))
			{
				return Directory.GetFiles(path, searchPattern2, SearchOption.AllDirectories);
			}
		}
		return new List<string>();
	}

	public void CreateDirectoryForFile(string path)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(GetAbsolutePath(path)));
	}

	public Stream CreateFile(string path)
	{
		return File.Create(GetAbsolutePath(path));
	}

	public void DeleteFile(string path)
	{
		File.Delete(GetAbsolutePath(path));
	}

	public Stream OpenRead(string path)
	{
		return File.OpenRead(GetAbsolutePath(path));
	}

	public Stream OpenWrite(string path)
	{
		return File.OpenWrite(GetAbsolutePath(path));
	}

	public Stream OpenFile(string filePath, FileMode mode, FileAccess access, FileShare share = FileShare.None, int bufferSize = 4096, FileOptions options = FileOptions.None)
	{
		return new FileStream(filePath, mode, access, share, bufferSize, options);
	}

	public string GetDirectoryPath()
	{
		return Root;
	}
}
