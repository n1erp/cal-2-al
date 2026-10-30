using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types;

public abstract class ExceptionService
{
	private static ExceptionService instance;

	public static ExceptionService Instance
	{
		get
		{
			return instance;
		}
		set
		{
			instance = value;
		}
	}

	protected abstract bool IgnoreSecurityExceptionInternal();

	protected abstract bool IgnoreWin32ExceptionInternal(Win32Exception exception);

	public static bool IgnoreSecurityException()
	{
		return instance.IgnoreSecurityExceptionInternal();
	}

	public static bool IgnoreWin32Exception(Win32Exception exception)
	{
		return instance.IgnoreWin32ExceptionInternal(exception);
	}

	internal static void FailFast(Exception exception, string messageToWrite)
	{
		string message = string.Format(CultureInfo.InvariantCulture, "The service could not write to the EventLog because of: '{0}'.\r\nThe original message is: '{1}'", exception.Message, messageToWrite);
		NavDiagnostics.GetMostSpecificInstance.SendExceptionTag("00001MQ", Category.Runtime, exception, Verbosity.Critical);
		EventLogWriter.StopAndWait();
		Environment.FailFast(message);
	}
}
