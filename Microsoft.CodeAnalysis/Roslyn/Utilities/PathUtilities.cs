using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Roslyn.Utilities;

internal static class PathUtilities
{
	private class PathComparer : IEqualityComparer<string>
	{
		public bool Equals(string x, string y)
		{
			if (x == null && y == null)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			return PathsEqual(x, y);
		}

		public int GetHashCode(string s)
		{
			return PathHashCode(s);
		}
	}

	internal static readonly char DirectorySeparatorChar = (PlatformInformation.IsUnix ? '/' : '\\');

	internal const char AltDirectorySeparatorChar = '/';

	internal const string ParentRelativeDirectory = "..";

	internal const string ThisDirectory = ".";

	internal static readonly string DirectorySeparatorStr = new string(DirectorySeparatorChar, 1);

	internal const char VolumeSeparatorChar = ':';

	private static readonly char[] s_pathChars = new char[3] { ':', DirectorySeparatorChar, '/' };

	public static readonly IEqualityComparer<string> Comparer = new PathComparer();

	internal static bool IsUnixLikePlatform => PlatformInformation.IsUnix;

	public static bool IsDirectorySeparator(char c)
	{
		if (c != DirectorySeparatorChar)
		{
			return c == '/';
		}
		return true;
	}

	public static bool IsAnyDirectorySeparator(char c)
	{
		if (c != '\\')
		{
			return c == '/';
		}
		return true;
	}

	public static string TrimTrailingSeparators(string s)
	{
		int num = s.Length;
		while (num > 0 && IsDirectorySeparator(s[num - 1]))
		{
			num--;
		}
		if (num != s.Length)
		{
			s = s.Substring(0, num);
		}
		return s;
	}

	public static string EnsureTrailingSeparator(string s)
	{
		if (s.Length == 0 || IsAnyDirectorySeparator(s[s.Length - 1]))
		{
			return s;
		}
		bool flag = s.IndexOf('/') >= 0;
		bool flag2 = s.IndexOf('\\') >= 0;
		if (flag && !flag2)
		{
			return s + "/";
		}
		if (!flag & flag2)
		{
			return s + "\\";
		}
		char directorySeparatorChar = DirectorySeparatorChar;
		return s + directorySeparatorChar;
	}

	public static string GetExtension(string path)
	{
		return FileNameUtilities.GetExtension(path);
	}

	public static string ChangeExtension(string path, string extension)
	{
		return FileNameUtilities.ChangeExtension(path, extension);
	}

	public static string RemoveExtension(string path)
	{
		return FileNameUtilities.ChangeExtension(path, null);
	}

	public static string GetFileName(string path, bool includeExtension = true)
	{
		return FileNameUtilities.GetFileName(path, includeExtension);
	}

	public static string GetDirectoryName(string path)
	{
		return GetDirectoryName(path, IsUnixLikePlatform);
	}

	internal static string GetDirectoryName(string path, bool isUnixLike)
	{
		if (path != null)
		{
			int length = GetPathRoot(path, isUnixLike).Length;
			if (path.Length > length)
			{
				int num = path.Length;
				while (num > length)
				{
					num--;
					if (IsDirectorySeparator(path[num]) && (num <= 0 || !IsDirectorySeparator(path[num - 1])))
					{
						break;
					}
				}
				return path.Substring(0, num);
			}
		}
		return null;
	}

	internal static bool IsSameDirectoryOrChildOf(string child, string parent)
	{
		parent = RemoveTrailingDirectorySeparator(parent);
		while (child != null)
		{
			child = RemoveTrailingDirectorySeparator(child);
			if (child.Equals(parent, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			child = GetDirectoryName(child);
		}
		return false;
	}

	public static string GetPathRoot(string path)
	{
		return GetPathRoot(path, IsUnixLikePlatform);
	}

	private static string GetPathRoot(string path, bool isUnixLike)
	{
		if (path == null)
		{
			return null;
		}
		if (isUnixLike)
		{
			return GetUnixRoot(path);
		}
		return GetWindowsRoot(path);
	}

	private static string GetWindowsRoot(string path)
	{
		int length = path.Length;
		if (length >= 1 && IsDirectorySeparator(path[0]))
		{
			if (length < 2 || !IsDirectorySeparator(path[1]))
			{
				return path.Substring(0, 1);
			}
			int i = 2;
			i = ConsumeDirectorySeparators(path, length, i);
			bool flag = false;
			while (true)
			{
				if (i == length)
				{
					return path;
				}
				if (!IsDirectorySeparator(path[i]))
				{
					i++;
					continue;
				}
				if (flag)
				{
					break;
				}
				flag = true;
				i = ConsumeDirectorySeparators(path, length, i);
			}
			return path.Substring(0, i);
		}
		if (length >= 2 && path[1] == ':')
		{
			if (length < 3 || !IsDirectorySeparator(path[2]))
			{
				return path.Substring(0, 2);
			}
			return path.Substring(0, 3);
		}
		return "";
	}

	private static int ConsumeDirectorySeparators(string path, int length, int i)
	{
		while (i < length && IsDirectorySeparator(path[i]))
		{
			i++;
		}
		return i;
	}

	private static string GetUnixRoot(string path)
	{
		if (path.Length <= 0 || !IsDirectorySeparator(path[0]))
		{
			return "";
		}
		return path.Substring(0, 1);
	}

	public static PathKind GetPathKind(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return PathKind.Empty;
		}
		if (IsAbsolute(path))
		{
			return PathKind.Absolute;
		}
		if (path.Length > 0 && path[0] == '.')
		{
			if (path.Length == 1 || IsDirectorySeparator(path[1]))
			{
				return PathKind.RelativeToCurrentDirectory;
			}
			if (path[1] == '.' && (path.Length == 2 || IsDirectorySeparator(path[2])))
			{
				return PathKind.RelativeToCurrentParent;
			}
		}
		if (!IsUnixLikePlatform)
		{
			if (path.Length >= 1 && IsDirectorySeparator(path[0]))
			{
				return PathKind.RelativeToCurrentRoot;
			}
			if (path.Length >= 2 && path[1] == ':' && (path.Length <= 2 || !IsDirectorySeparator(path[2])))
			{
				return PathKind.RelativeToDriveDirectory;
			}
		}
		return PathKind.Relative;
	}

	public static bool IsAbsolute(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		if (IsUnixLikePlatform)
		{
			return path[0] == DirectorySeparatorChar;
		}
		if (IsDriveRootedAbsolutePath(path))
		{
			return true;
		}
		if (path.Length >= 2 && IsDirectorySeparator(path[0]))
		{
			return IsDirectorySeparator(path[1]);
		}
		return false;
	}

	private static bool IsDriveRootedAbsolutePath(string path)
	{
		if (path.Length >= 3 && path[1] == ':')
		{
			return IsDirectorySeparator(path[2]);
		}
		return false;
	}

	public static string CombineAbsoluteAndRelativePaths(string root, string relativePath)
	{
		return CombinePossiblyRelativeAndRelativePaths(root, relativePath);
	}

	public static string CombinePossiblyRelativeAndRelativePaths(string rootOpt, string relativePath)
	{
		if (string.IsNullOrEmpty(rootOpt))
		{
			return null;
		}
		switch (GetPathKind(relativePath))
		{
		case PathKind.Empty:
			return rootOpt;
		case PathKind.RelativeToCurrentRoot:
		case PathKind.RelativeToDriveDirectory:
		case PathKind.Absolute:
			return null;
		default:
			return CombinePathsUnchecked(rootOpt, relativePath);
		}
	}

	public static string CombinePathsUnchecked(string root, string relativePath)
	{
		char c = root[root.Length - 1];
		if (!IsDirectorySeparator(c) && c != ':')
		{
			return root + DirectorySeparatorStr + relativePath;
		}
		return root + relativePath;
	}

	private static string RemoveTrailingDirectorySeparator(string path)
	{
		if (path.Length > 0 && IsDirectorySeparator(path[path.Length - 1]))
		{
			return path.Substring(0, path.Length - 1);
		}
		return path;
	}

	public static bool IsFilePath(string assemblyDisplayNameOrPath)
	{
		string extension = FileNameUtilities.GetExtension(assemblyDisplayNameOrPath);
		if (!string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase) && !string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase) && assemblyDisplayNameOrPath.IndexOf(DirectorySeparatorChar) == -1)
		{
			return assemblyDisplayNameOrPath.IndexOf('/') != -1;
		}
		return true;
	}

	public static bool ContainsPathComponent(string path, string component, bool ignoreCase)
	{
		StringComparison comparisonType = (ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		if (path != null && path.IndexOf(component, comparisonType) >= 0)
		{
			StringComparer stringComparer = (ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
			int num = 0;
			string text = path;
			while (text != null)
			{
				string fileName = GetFileName(text);
				if (stringComparer.Equals(fileName, component))
				{
					return true;
				}
				text = GetDirectoryName(text);
				num++;
			}
		}
		return false;
	}

	public static string GetRelativePath(string directory, string fullPath)
	{
		string text = string.Empty;
		if (IsChildPath(directory, fullPath))
		{
			return GetRelativeChildPath(directory, fullPath);
		}
		string[] pathParts = GetPathParts(directory);
		string[] pathParts2 = GetPathParts(fullPath);
		if (pathParts.Length == 0 || pathParts2.Length == 0)
		{
			return fullPath;
		}
		int i;
		for (i = 0; i < pathParts.Length && PathsEqual(pathParts[i], pathParts2[i]); i++)
		{
		}
		if (i == 0)
		{
			return fullPath;
		}
		int num = pathParts.Length - i;
		if (num > 0)
		{
			for (int j = 0; j < num; j++)
			{
				text = text + ".." + DirectorySeparatorStr;
			}
		}
		for (int k = i; k < pathParts2.Length; k++)
		{
			text = CombinePathsUnchecked(text, pathParts2[k]);
		}
		return text;
	}

	public static bool IsChildPath(string parentPath, string childPath)
	{
		if (parentPath.Length > 0 && childPath.Length > parentPath.Length && PathsEqual(childPath, parentPath, parentPath.Length))
		{
			if (!IsDirectorySeparator(parentPath[parentPath.Length - 1]))
			{
				return IsDirectorySeparator(childPath[parentPath.Length]);
			}
			return true;
		}
		return false;
	}

	private static string GetRelativeChildPath(string parentPath, string childPath)
	{
		string text = childPath.Substring(parentPath.Length);
		int num = ConsumeDirectorySeparators(text, text.Length, 0);
		if (num > 0)
		{
			text = text.Substring(num);
		}
		return text;
	}

	private static string[] GetPathParts(string path)
	{
		string[] array = path.Split(s_pathChars);
		if (array.Contains("."))
		{
			array = array.Where((string s) => s != ".").ToArray();
		}
		return array;
	}

	public static bool PathsEqual(string path1, string path2)
	{
		return PathsEqual(path1, path2, Math.Max(path1.Length, path2.Length));
	}

	private static bool PathsEqual(string path1, string path2, int length)
	{
		if (path1.Length < length || path2.Length < length)
		{
			return false;
		}
		for (int i = 0; i < length; i++)
		{
			if (!PathCharEqual(path1[i], path2[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static bool PathCharEqual(char x, char y)
	{
		if (IsDirectorySeparator(x) && IsDirectorySeparator(y))
		{
			return true;
		}
		if (!IsUnixLikePlatform)
		{
			return char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
		}
		return x == y;
	}

	private static int PathHashCode(string path)
	{
		int num = 0;
		if (path != null)
		{
			foreach (char c in path)
			{
				if (!IsDirectorySeparator(c))
				{
					num = Hash.Combine((int)char.ToUpperInvariant(c), num);
				}
			}
		}
		return num;
	}

	public static string NormalizePathPrefix(string filePath, ImmutableArray<KeyValuePair<string, string>> pathMap)
	{
		if (pathMap.IsDefaultOrEmpty)
		{
			return filePath;
		}
		foreach (KeyValuePair<string, string> item in pathMap)
		{
			string key = item.Key;
			if (key == null || key.Length <= 0 || !filePath.StartsWith(key, StringComparison.Ordinal))
			{
				continue;
			}
			string value = item.Value;
			string text = value + filePath.Substring(key.Length);
			bool flag = value.IndexOf('/') >= 0;
			bool flag2 = value.IndexOf('\\') >= 0;
			if (!flag || flag2)
			{
				if (!flag2 || flag)
				{
					return text;
				}
				return text.Replace('/', '\\');
			}
			return text.Replace('\\', '/');
		}
		return filePath;
	}

	public static bool IsValidFilePath(string fullPath)
	{
		try
		{
			if (string.IsNullOrEmpty(fullPath))
			{
				return false;
			}
			return !string.IsNullOrEmpty(new FileInfo(fullPath).Name);
		}
		catch (Exception ex) when (ex is ArgumentException || ex is PathTooLongException || ex is NotSupportedException)
		{
			return false;
		}
	}
}
