using System.Collections.Immutable;
using System.IO;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal class StrongNameFileSystem
{
	internal static readonly StrongNameFileSystem Instance = new StrongNameFileSystem();

	protected StrongNameFileSystem()
	{
	}

	internal virtual byte[] ReadAllBytes(string fullPath)
	{
		return File.ReadAllBytes(fullPath);
	}

	internal string ResolveStrongNameKeyFile(string path, ImmutableArray<string> keyFileSearchPaths)
	{
		if (PathUtilities.IsAbsolute(path))
		{
			if (FileExists(path))
			{
				return FileUtilities.TryNormalizeAbsolutePath(path);
			}
			return path;
		}
		foreach (string item in keyFileSearchPaths)
		{
			string text = PathUtilities.CombineAbsoluteAndRelativePaths(item, path);
			if (FileExists(text))
			{
				return FileUtilities.TryNormalizeAbsolutePath(text);
			}
		}
		return null;
	}

	internal virtual bool FileExists(string fullPath)
	{
		return File.Exists(fullPath);
	}
}
