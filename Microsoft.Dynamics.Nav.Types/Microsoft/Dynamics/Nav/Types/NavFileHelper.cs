using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

public static class NavFileHelper
{
	private static readonly HashSet<char> InvalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());

	private static readonly HashSet<char> InvalidPathChars = new HashSet<char>(Path.GetInvalidPathChars());

	public static void MapAndRethrowClrException(Exception ex, bool writeExceptionToEventLog)
	{
		if (writeExceptionToEventLog)
		{
			EventLogWriter.Write(ex);
		}
		if (ex is FileNotFoundException ex2)
		{
			throw new NavNCLFileNotFoundException(ex2.FileName);
		}
		if (ex is UnauthorizedAccessException)
		{
			throw new NavNCLUnauthorizedAccessException();
		}
		if (ex is NotSupportedException)
		{
			throw new NavNCLInvalidPathException();
		}
		if (ex is DirectoryNotFoundException || ex is PathTooLongException)
		{
			throw new NavNCLInvalidPathException(ex.Message);
		}
		if (ex is IOException)
		{
			throw new NavNCLStreamIOException();
		}
		if (ex is ArgumentOutOfRangeException)
		{
			throw NavNCLArgumentOutOfRangeException.CreateForFile(CultureInfo.CurrentCulture, ex.Message);
		}
		if (ex is ArgumentException)
		{
			throw new NavNCLInvalidPathException(ex.Message);
		}
	}

	public static string SanitizeFileName(string fileName, char replacementCharacter = '_')
	{
		return SanitizeStringByRemovingInvalidCharacters(InvalidFileNameChars, fileName, replacementCharacter);
	}

	public static string SanitizePath(string path, char replacementCharacter = '_')
	{
		return SanitizeStringByRemovingInvalidCharacters(InvalidPathChars, path, replacementCharacter);
	}

	private static string SanitizeStringByRemovingInvalidCharacters(HashSet<char> invalidCharacters, string value, char replacementCharacter = '_')
	{
		if (invalidCharacters.Contains(replacementCharacter))
		{
			replacementCharacter = '_';
		}
		char[] array = new char[value.Length];
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			array[i] = (invalidCharacters.Contains(c) ? replacementCharacter : c);
		}
		return new string(array);
	}
}
