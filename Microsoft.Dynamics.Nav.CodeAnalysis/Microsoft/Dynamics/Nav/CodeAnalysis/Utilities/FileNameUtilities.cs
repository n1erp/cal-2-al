using System.IO;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class FileNameUtilities
{
	internal const char DirectorySeparatorChar = '\\';

	internal const char AltDirectorySeparatorChar = '/';

	internal const char VolumeSeparatorChar = ':';

	internal static bool IsFileName(string path)
	{
		return IndexOfFileName(path) == 0;
	}

	private static int IndexOfExtension(string path)
	{
		if (path == null)
		{
			return -1;
		}
		int length = path.Length;
		int num = length;
		while (--num >= 0)
		{
			char c = path[num];
			if (c == '.')
			{
				if (num != length - 1)
				{
					return num;
				}
				return -1;
			}
			if (c == '\\' || c == '/' || c == ':')
			{
				break;
			}
		}
		return -1;
	}

	internal static string GetExtension(string path)
	{
		if (path == null)
		{
			return null;
		}
		int num = IndexOfExtension(path);
		if (num < 0)
		{
			return string.Empty;
		}
		return path.Substring(num);
	}

	private static string RemoveExtension(string path)
	{
		if (path == null)
		{
			return null;
		}
		int num = IndexOfExtension(path);
		if (num >= 0)
		{
			return path.Substring(0, num);
		}
		if (path.Length > 0 && path[path.Length - 1] == '.')
		{
			return path.Substring(0, path.Length - 1);
		}
		return path;
	}

	internal static string ChangeExtension(string path, string extension)
	{
		if (path == null)
		{
			return null;
		}
		string text = RemoveExtension(path);
		if (extension == null || path.Length == 0)
		{
			return text;
		}
		if (extension.Length == 0 || extension[0] != '.')
		{
			return text + "." + extension;
		}
		return text + extension;
	}

	internal static int IndexOfFileName(string path)
	{
		if (path == null)
		{
			return -1;
		}
		for (int num = path.Length - 1; num >= 0; num--)
		{
			char c = path[num];
			if (c == '\\' || c == '/' || c == ':')
			{
				return num + 1;
			}
		}
		return 0;
	}

	internal static string GetFileName(string path)
	{
		int num = IndexOfFileName(path);
		if (num > 0)
		{
			return path.Substring(num);
		}
		return path;
	}

	internal static string SanitizeFilename(string filename)
	{
		string text = filename;
		foreach (char item in Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()))
		{
			text = text.Replace(item.ToString(), string.Empty);
		}
		return text;
	}
}
