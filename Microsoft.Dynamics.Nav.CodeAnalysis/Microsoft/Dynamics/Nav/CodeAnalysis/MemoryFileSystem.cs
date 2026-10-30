using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class MemoryFileSystem : IFileSystem
{
	private class EntryStream : MemoryStream
	{
		public event Action OnClosed;

		public override void Close()
		{
			OnClosed?.Invoke();
			base.Close();
		}
	}

	internal Dictionary<string, byte[]> Data { get; }

	public MemoryFileSystem(IDictionary<string, byte[]> data)
	{
		Data = data.ToDictionary((KeyValuePair<string, byte[]> kv) => kv.Key, (KeyValuePair<string, byte[]> kv) => kv.Value, PackagePathComparer.Instance);
	}

	public byte[] ReadBytes(string path)
	{
		if (!Data.TryGetValue(path, out var value))
		{
			throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.FileNotFoundException, path), path);
		}
		return value;
	}

	public byte[] ReadBytes(string path, int count)
	{
		return ReadBytes(path).Take(count).ToArray();
	}

	public void WriteBytes(string path, byte[] content)
	{
		Data[path] = content;
	}

	public bool Exists(string path)
	{
		return Data.ContainsKey(path);
	}

	public IEnumerable<string> GetFiles(string searchPattern)
	{
		Regex matcher = new Regex("^" + GlobToRegexPattern(searchPattern) + "$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		return Data.Keys.Where((string k) => matcher.IsMatch(k.Replace("\\", "/")));
	}

	private static string GlobToRegexPattern(string globPattern)
	{
		return Regex.Escape(globPattern.Replace("\\", "/")).Replace("\\*", ".*").Replace("\\?", ".");
	}

	public bool DirectoryExistsForFile(string path)
	{
		return Data.Any((KeyValuePair<string, byte[]> kvp) => kvp.Key.StartsWith(path, StringComparison.OrdinalIgnoreCase));
	}

	public void CreateDirectoryForFile(string path)
	{
	}

	public Stream CreateFile(string path)
	{
		EntryStream stream = new EntryStream();
		stream.OnClosed += () =>
		{
			WriteBytes(path, stream.ToArray());
		};
		return stream;
	}

	public void DeleteFile(string path)
	{
		Data.Remove(path);
	}

	public Stream OpenRead(string path)
	{
		return new MemoryStream(Data[path]);
	}

	public Stream OpenWrite(string path)
	{
		return OpenRead(path);
	}

	public Stream OpenFile(string filePath, FileMode mode, FileAccess access, FileShare share = FileShare.None, int bufferSize = 4096, FileOptions options = FileOptions.None)
	{
		return OpenRead(filePath);
	}

	public string GetDirectoryPath()
	{
		return string.Empty;
	}
}
