using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TempPathHousekeeper
{
	private readonly HashSet<string> allUnknownFiles = new HashSet<string>();

	private static int maxFilesToTrack = 20000;

	public int Count => allUnknownFiles.Count;

	public void Initialize(string path, string searchPattern)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentNullException("path");
		}
		if (string.IsNullOrEmpty(searchPattern))
		{
			throw new ArgumentNullException("searchPattern");
		}
		try
		{
			CheckAndAddFiles(Directory.GetFiles(path, searchPattern));
			string[] directories = Directory.GetDirectories(path);
			foreach (string path2 in directories)
			{
				if (allUnknownFiles.Count < maxFilesToTrack)
				{
					CheckAndAddFiles(Directory.GetFiles(path2, searchPattern));
				}
			}
		}
		catch (Exception exception)
		{
			MapClrExceptionAndRethrow(exception);
			throw;
		}
	}

	private void CheckAndAddFiles(string[] files)
	{
		lock (allUnknownFiles)
		{
			for (int i = 0; i < files.Length; i++)
			{
				if (allUnknownFiles.Count >= maxFilesToTrack)
				{
					break;
				}
				allUnknownFiles.Add(files[i]);
			}
		}
	}

	public void MarkFileAsKnown(string fileName)
	{
		lock (allUnknownFiles)
		{
			if (allUnknownFiles.Contains(fileName))
			{
				allUnknownFiles.Remove(fileName);
			}
		}
	}

	public void Clear()
	{
		lock (allUnknownFiles)
		{
			foreach (string allUnknownFile in allUnknownFiles)
			{
				try
				{
					if (File.Exists(allUnknownFile))
					{
						File.Delete(allUnknownFile);
					}
				}
				catch (IOException exception)
				{
					LogWriterHelper.LogExceptionEvent(exception);
				}
				catch (UnauthorizedAccessException exception2)
				{
					LogWriterHelper.LogExceptionEvent(exception2);
				}
				catch (ArgumentException exception3)
				{
					LogWriterHelper.LogExceptionEvent(exception3);
				}
			}
			allUnknownFiles.Clear();
		}
	}

	internal static void MapClrExceptionAndRethrow(Exception exception)
	{
		LogWriterHelper.LogExceptionEvent(exception);
		if (exception is ArgumentException)
		{
			throw new NavNCLInvalidPathException();
		}
		if (exception is ArgumentNullException)
		{
			throw new NavNCLInvalidPathException();
		}
		if (exception is PathTooLongException)
		{
			throw new NavNCLInvalidPathException();
		}
		if (exception is DirectoryNotFoundException)
		{
			throw new NavNCLInvalidPathException();
		}
		if (exception is NotSupportedException)
		{
			throw new NavNCLInvalidPathException();
		}
		if (exception is FileNotFoundException ex)
		{
			throw new NavNCLFileNotFoundException(ex.FileName);
		}
		if (exception is IOException ex2)
		{
			throw new NavClientIOException(ex2.Message, ex2);
		}
		if (exception is UnauthorizedAccessException)
		{
			throw new NavNCLUnauthorizedAccessException();
		}
		if (exception is SecurityException)
		{
			throw new NavNCLUnauthorizedAccessException();
		}
	}
}
