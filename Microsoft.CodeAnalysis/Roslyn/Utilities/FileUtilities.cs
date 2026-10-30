using System;
using System.Collections.Generic;
using System.IO;
using System.Security;

namespace Roslyn.Utilities;

internal static class FileUtilities
{
	private static readonly char[] s_invalidPathChars = Path.GetInvalidPathChars();

	internal static string ResolveRelativePath(string path, string basePath, string baseDirectory, IEnumerable<string> searchPaths, Func<string, bool> fileExists)
	{
		PathKind pathKind = PathUtilities.GetPathKind(path);
		string text;
		if (pathKind == PathKind.Relative)
		{
			baseDirectory = GetBaseDirectory(basePath, baseDirectory);
			if (baseDirectory != null)
			{
				text = PathUtilities.CombinePathsUnchecked(baseDirectory, path);
				if (fileExists(text))
				{
					return text;
				}
			}
			foreach (string searchPath in searchPaths)
			{
				text = PathUtilities.CombinePathsUnchecked(searchPath, path);
				if (fileExists(text))
				{
					return text;
				}
			}
			return null;
		}
		text = ResolveRelativePath(pathKind, path, basePath, baseDirectory);
		if (text != null && fileExists(text))
		{
			return text;
		}
		return null;
	}

	internal static string ResolveRelativePath(string path, string baseDirectory)
	{
		return ResolveRelativePath(path, null, baseDirectory);
	}

	internal static string ResolveRelativePath(string path, string basePath, string baseDirectory)
	{
		return ResolveRelativePath(PathUtilities.GetPathKind(path), path, basePath, baseDirectory);
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
			return PathUtilities.CombinePathsUnchecked(baseDirectory, path);
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
			return PathUtilities.CombinePathsUnchecked(baseDirectory, path);
		case PathKind.RelativeToCurrentParent:
			baseDirectory = GetBaseDirectory(basePath, baseDirectory);
			if (baseDirectory == null)
			{
				return null;
			}
			return PathUtilities.CombinePathsUnchecked(baseDirectory, path);
		case PathKind.RelativeToCurrentRoot:
		{
			string pathRoot;
			if (basePath != null)
			{
				pathRoot = PathUtilities.GetPathRoot(basePath);
			}
			else
			{
				if (baseDirectory == null)
				{
					return null;
				}
				pathRoot = PathUtilities.GetPathRoot(baseDirectory);
			}
			if (string.IsNullOrEmpty(pathRoot))
			{
				return null;
			}
			return PathUtilities.CombinePathsUnchecked(pathRoot, path.Substring(1));
		}
		case PathKind.RelativeToDriveDirectory:
			return null;
		case PathKind.Absolute:
			return path;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	private static string GetBaseDirectory(string basePath, string baseDirectory)
	{
		string text = ResolveRelativePath(basePath, baseDirectory);
		if (text == null)
		{
			return baseDirectory;
		}
		try
		{
			return Path.GetDirectoryName(text);
		}
		catch (Exception)
		{
			return null;
		}
	}

	internal static string NormalizeRelativePath(string path, string basePath, string baseDirectory)
	{
		if (path.IndexOf("://", StringComparison.Ordinal) >= 0 || path.IndexOfAny(s_invalidPathChars) >= 0)
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

	internal static string NormalizeDirectoryPath(string path)
	{
		return NormalizeAbsolutePath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}

	internal static string TryNormalizeAbsolutePath(string path)
	{
		try
		{
			return Path.GetFullPath(path);
		}
		catch
		{
			return null;
		}
	}

	internal static Stream OpenRead(string fullPath)
	{
		try
		{
			return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}
		catch (IOException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			throw new IOException(ex2.Message, ex2);
		}
	}

	internal static Stream OpenAsyncRead(string fullPath)
	{
		return RethrowExceptionsAsIOException(() => new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous));
	}

	internal static T RethrowExceptionsAsIOException<T>(Func<T> operation)
	{
		try
		{
			return operation();
		}
		catch (IOException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			throw new IOException(ex2.Message, ex2);
		}
	}

	internal static Stream CreateFileStreamChecked(Func<string, Stream> factory, string path, string paramName = null)
	{
		try
		{
			return factory(path);
		}
		catch (ArgumentNullException)
		{
			if (paramName == null)
			{
				throw;
			}
			throw new ArgumentNullException(paramName);
		}
		catch (ArgumentException ex2)
		{
			if (paramName == null)
			{
				throw;
			}
			throw new ArgumentException(ex2.Message, paramName);
		}
		catch (IOException)
		{
			throw;
		}
		catch (Exception ex4)
		{
			throw new IOException(ex4.Message, ex4);
		}
	}

	internal static DateTime GetFileTimeStamp(string fullPath)
	{
		try
		{
			return File.GetLastWriteTimeUtc(fullPath);
		}
		catch (IOException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			throw new IOException(ex2.Message, ex2);
		}
	}

	internal static long GetFileLength(string fullPath)
	{
		try
		{
			return new FileInfo(fullPath).Length;
		}
		catch (IOException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			throw new IOException(ex2.Message, ex2);
		}
	}

	internal static Stream OpenFileStream(string path)
	{
		try
		{
			return File.OpenRead(path);
		}
		catch (ArgumentException)
		{
			throw;
		}
		catch (IOException ex2)
		{
			if (((object)ex2).GetType().Name == "DirectoryNotFoundException")
			{
				throw new FileNotFoundException(ex2.Message, path, ex2);
			}
			throw;
		}
		catch (Exception ex3)
		{
			throw new IOException(ex3.Message, ex3);
		}
	}
}
