using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class PathUtilities
{
	internal const char VolumeSeparatorChar = ':';

	internal static readonly char DirectorySeparatorChar = (IsUnixLikePlatform ? '/' : '\\');

	internal static readonly char AltDirectorySeparatorChar = '/';

	internal static readonly string DirectorySeparatorStr = new string(DirectorySeparatorChar, 1);

	private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

	private static bool IsUnixLikePlatform => Path.DirectorySeparatorChar == '/';

	public static string CombineAbsoluteAndRelativePaths(string root, string relativePath)
	{
		DebugAssertHelper.Assert(IsAbsolute(root));
		return CombinePossiblyRelativeAndRelativePaths(root, relativePath);
	}

	internal static bool IsDirectorySeparator(char c)
	{
		if (c != DirectorySeparatorChar)
		{
			return c == AltDirectorySeparatorChar;
		}
		return true;
	}

	internal static string GetExtension(string path)
	{
		return FileNameUtilities.GetExtension(path);
	}

	internal static string ChangeExtension(string path, string extension)
	{
		return FileNameUtilities.ChangeExtension(path, extension);
	}

	internal static string RemoveExtension(string path)
	{
		return FileNameUtilities.ChangeExtension(path, null);
	}

	internal static string GetFileName(string path)
	{
		return FileNameUtilities.GetFileName(path);
	}

	internal static string GetDirectoryName(this string path)
	{
		int num = FileNameUtilities.IndexOfFileName(path);
		if (num < 0)
		{
			return null;
		}
		return path.Substring(0, num);
	}

	internal static PathKind GetPathKind(this string path)
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

	internal static bool IsAbsolute(string path)
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

	internal static string GetPathRoot(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return null;
		}
		int pathRootLength = GetPathRootLength(path);
		if (pathRootLength == -1)
		{
			return null;
		}
		return path.Substring(0, pathRootLength);
	}

	internal static string CombinePossiblyRelativeAndRelativePaths(string rootOpt, string relativePath)
	{
		if (string.IsNullOrEmpty(rootOpt))
		{
			return null;
		}
		switch (relativePath.GetPathKind())
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

	internal static string ResolveRelativePath(string path, string basePath, string baseDirectory, IEnumerable<string> searchPaths, Func<string, bool> fileExists)
	{
		DebugAssertHelper.Assert(baseDirectory == null || searchPaths != null || IsAbsolute(baseDirectory));
		DebugAssertHelper.Assert(fileExists != null);
		PathKind pathKind = path.GetPathKind();
		string text;
		if (pathKind == PathKind.Relative)
		{
			baseDirectory = GetBaseDirectory(basePath, baseDirectory);
			if (baseDirectory != null)
			{
				text = CombinePathsUnchecked(baseDirectory, path);
				DebugAssertHelper.Assert(IsAbsolute(text));
				if (fileExists(text))
				{
					return text;
				}
			}
			foreach (string searchPath in searchPaths)
			{
				text = CombinePathsUnchecked(searchPath, path);
				DebugAssertHelper.Assert(IsAbsolute(text));
				if (fileExists(text))
				{
					return text;
				}
			}
			return null;
		}
		text = ResolveRelativePath(pathKind, path, basePath, baseDirectory);
		if (text != null)
		{
			DebugAssertHelper.Assert(IsAbsolute(text));
			if (fileExists(text))
			{
				return text;
			}
		}
		return null;
	}

	internal static string ResolveRelativePath(string path, string baseDirectory)
	{
		return ResolveRelativePath(path, null, baseDirectory);
	}

	internal static string ResolveRelativePath(string path, string basePath, string baseDirectory)
	{
		DebugAssertHelper.Assert(baseDirectory == null || IsAbsolute(baseDirectory));
		return ResolveRelativePath(path.GetPathKind(), path, basePath, baseDirectory);
	}

	internal static string NormalizeRelativePath(string path, string basePath, string baseDirectory)
	{
		if (path.IndexOf("://", StringComparison.Ordinal) >= 0 || path.IndexOfAny(InvalidPathChars) >= 0)
		{
			return null;
		}
		if (string.IsNullOrEmpty(baseDirectory))
		{
			return null;
		}
		string text = ResolveRelativePath(path, basePath, baseDirectory);
		if (text == null)
		{
			return null;
		}
		string text2 = TryNormalizeAbsolutePath(text);
		if (text2 == null)
		{
			return null;
		}
		return text2;
	}

	internal static string NormalizeAbsolutePath(string path)
	{
		DebugAssertHelper.Assert(IsAbsolute(path));
		try
		{
			return Path.GetFullPath(path);
		}
		catch (ArgumentException ex)
		{
			throw new IOException(ex.Message, ex);
		}
		catch (SecurityException ex2)
		{
			throw new IOException(ex2.Message, ex2);
		}
		catch (NotSupportedException ex3)
		{
			throw new IOException(ex3.Message, ex3);
		}
	}

	internal static string TryNormalizeAbsolutePath(string path)
	{
		DebugAssertHelper.Assert(IsAbsolute(path));
		try
		{
			return Path.GetFullPath(path);
		}
		catch
		{
			return null;
		}
	}

	internal static string CombinePathsUnchecked(string root, string relativePath)
	{
		DebugAssertHelper.Assert(!string.IsNullOrEmpty(root));
		char c = root[root.Length - 1];
		if (!IsDirectorySeparator(c) && c != ':')
		{
			return root + DirectorySeparatorStr + relativePath;
		}
		return root + relativePath;
	}

	internal static string EnsureEndingInDirectorySeperatorChar(string path)
	{
		if (!path.EndsWith(DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
		{
			return path + DirectorySeparatorChar;
		}
		return path;
	}

	internal static string CreateUriFilePath(this string filePath)
	{
		Uri uri = new Uri(filePath);
		if (uri.IsUnc)
		{
			return uri.LocalPath;
		}
		return filePath.Replace("\\", "/");
	}

	private static bool IsDriveRootedAbsolutePath(string path)
	{
		DebugAssertHelper.Assert(!IsUnixLikePlatform);
		if (path.Length >= 3 && path[1] == ':')
		{
			return IsDirectorySeparator(path[2]);
		}
		return false;
	}

	private static int GetPathRootLength(string path)
	{
		DebugAssertHelper.Assert(!string.IsNullOrEmpty(path));
		if (IsUnixLikePlatform)
		{
			if (IsDirectorySeparator(path[0]))
			{
				return 1;
			}
		}
		else
		{
			if (IsDriveRootedAbsolutePath(path))
			{
				return 3;
			}
			if (IsDirectorySeparator(path[0]))
			{
				return GetUncPathRootLength(path);
			}
		}
		return -1;
	}

	private static int GetUncPathRootLength(string path)
	{
		DebugAssertHelper.Assert(IsDirectorySeparator(path[0]));
		int num = IndexOfNonDirectorySeparator(path, 1);
		if (num < 2)
		{
			return -1;
		}
		int num2 = IndexOfDirectorySeparator(path, num);
		if (num2 == -1)
		{
			return -1;
		}
		int num3 = IndexOfNonDirectorySeparator(path, num2);
		if (num3 == -1)
		{
			return -1;
		}
		int num4 = IndexOfDirectorySeparator(path, num3);
		if (num4 != -1)
		{
			return num4;
		}
		return path.Length;
	}

	private static int IndexOfDirectorySeparator(string path, int start)
	{
		for (int i = start; i < path.Length; i++)
		{
			if (IsDirectorySeparator(path[i]))
			{
				return i;
			}
		}
		return -1;
	}

	private static int IndexOfNonDirectorySeparator(string path, int start)
	{
		for (int i = start; i < path.Length; i++)
		{
			if (!IsDirectorySeparator(path[i]))
			{
				return i;
			}
		}
		return -1;
	}

	internal static string CombinePath(string baseDirPath, string absoluteOrRelativeToBasePath)
	{
		if (IsAbsolute(absoluteOrRelativeToBasePath))
		{
			return absoluteOrRelativeToBasePath;
		}
		string text = ResolveRelativePath(absoluteOrRelativeToBasePath, null, baseDirPath, null, (string p) => true);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return absoluteOrRelativeToBasePath;
	}

	private static string ResolveRelativePath(PathKind kind, string path, string basePath, string baseDirectory)
	{
		switch (kind)
		{
		case PathKind.Empty:
			return null;
		case PathKind.Relative:
			baseDirectory = GetBaseDirectory(basePath, baseDirectory);
			if (baseDirectory == null)
			{
				return null;
			}
			return CombinePathsUnchecked(baseDirectory, path);
		case PathKind.RelativeToCurrentDirectory:
			baseDirectory = GetBaseDirectory(basePath, baseDirectory);
			if (baseDirectory == null)
			{
				return null;
			}
			if (path.Length == 1)
			{
				return baseDirectory;
			}
			return CombinePathsUnchecked(baseDirectory, path);
		case PathKind.RelativeToCurrentParent:
			baseDirectory = GetBaseDirectory(basePath, baseDirectory);
			if (baseDirectory == null)
			{
				return null;
			}
			return CombinePathsUnchecked(baseDirectory, path);
		case PathKind.RelativeToCurrentRoot:
		{
			string pathRoot;
			if (basePath != null)
			{
				pathRoot = GetPathRoot(basePath);
			}
			else
			{
				if (baseDirectory == null)
				{
					return null;
				}
				pathRoot = GetPathRoot(baseDirectory);
			}
			if (pathRoot == null)
			{
				return null;
			}
			DebugAssertHelper.Assert(IsDirectorySeparator(path[0]));
			DebugAssertHelper.Assert(path.Length == 1 || !IsDirectorySeparator(path[1]));
			DebugAssertHelper.Assert(pathRoot.Length >= 3);
			return CombinePathsUnchecked(pathRoot, path.Substring(1));
		}
		case PathKind.RelativeToDriveDirectory:
			return null;
		case PathKind.Absolute:
			return path;
		default:
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unexpected PathKind {0}.", kind));
		}
	}

	private static string GetBaseDirectory(string basePath, string baseDirectory)
	{
		string text = ResolveRelativePath(basePath, baseDirectory);
		if (text == null)
		{
			return baseDirectory;
		}
		DebugAssertHelper.Assert(IsAbsolute(text));
		try
		{
			return Path.GetDirectoryName(text);
		}
		catch (Exception)
		{
			return null;
		}
	}

	internal static string RadPath(this string baseFolder)
	{
		return Path.Combine(Path.Combine(baseFolder, ".vscode"), "rad.json");
	}
}
