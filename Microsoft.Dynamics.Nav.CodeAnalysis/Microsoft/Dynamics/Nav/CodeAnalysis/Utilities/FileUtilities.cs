using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class FileUtilities
{
	public static bool ValidateFileExists(string path)
	{
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			return false;
		}
		return true;
	}

	internal static Stream OpenRead(string fullPath)
	{
		DebugAssertHelper.Assert(PathUtilities.IsAbsolute(fullPath));
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
		DebugAssertHelper.Assert(PathUtilities.IsAbsolute(fullPath));
		try
		{
			return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
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
		DebugAssertHelper.Assert(PathUtilities.IsAbsolute(fullPath));
		try
		{
			return File.GetLastWriteTimeUtc(fullPath);
		}
		catch (Exception ex)
		{
			throw new IOException(ex.Message);
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
			if (ex2.GetType().Name == "DirectoryNotFoundException")
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

	internal static byte[] ReadBytes(string path, int requestedBytes)
	{
		using FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		using BinaryReader binaryReader = new BinaryReader(input);
		return binaryReader.ReadBytes(requestedBytes);
	}
}
