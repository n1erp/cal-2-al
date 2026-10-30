using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

public static class ProcessStarter
{
	public static Process Start(string applicationOrFileName, string arguments)
	{
		if (applicationOrFileName == null)
		{
			throw new ArgumentNullException("applicationOrFileName");
		}
		if (applicationOrFileName.Length == 0)
		{
			throw new ArgumentException(Lang.ProcessNameOrCommandLineArgumentCannotBeEmptyString, "applicationOrFileName");
		}
		return Start(string.IsNullOrEmpty(arguments) ? new ProcessStartInfo(applicationOrFileName) : new ProcessStartInfo(applicationOrFileName, arguments));
	}

	public static void Start(string applicationOrFileName)
	{
		if (applicationOrFileName == null)
		{
			throw new ArgumentNullException("applicationOrFileName");
		}
		if (applicationOrFileName.Length == 0)
		{
			throw new ArgumentException(Lang.ProcessNameOrCommandLineArgumentCannotBeEmptyString, "applicationOrFileName");
		}
		ProcessStartInfo startupParameters = new ProcessStartInfo(applicationOrFileName);
		Process process = null;
		try
		{
			process = Start(startupParameters);
		}
		finally
		{
			process?.Dispose();
		}
	}

	public static Process Start(ProcessStartInfo startupParameters)
	{
		if (startupParameters == null)
		{
			throw new ArgumentNullException("startupParameters");
		}
		try
		{
			Process process = new Process();
			process.StartInfo = startupParameters;
			process.Start();
			return process;
		}
		catch (ObjectDisposedException innerException)
		{
			throw new NavProcessException(CreateExceptionMessage(startupParameters), innerException);
		}
		catch (Win32Exception innerException2)
		{
			throw new NavProcessException(CreateExceptionMessage(startupParameters), innerException2);
		}
		catch (InvalidOperationException innerException3)
		{
			throw new NavProcessException(CreateExceptionMessage(startupParameters), innerException3);
		}
	}

	private static string CreateExceptionMessage(ProcessStartInfo info)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.ProcessCannotBeStarted, info.Arguments);
	}
}
