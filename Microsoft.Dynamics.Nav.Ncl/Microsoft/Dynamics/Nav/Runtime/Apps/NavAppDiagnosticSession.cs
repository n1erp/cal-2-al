using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppDiagnosticSession
{
	public static void SendTraceTagOnFailure(Action operation, string additionalInfo = "", Func<Exception, Exception> exceptionMap = null, [CallerMemberName] string callerName = "")
	{
		if (operation == null)
		{
			throw new ArgumentNullException("operation");
		}
		try
		{
			operation();
		}
		catch (Exception ex)
		{
			NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance(NavCurrentThread.Session);
			string text = (string.IsNullOrWhiteSpace(additionalInfo) ? "" : string.Format(CultureInfo.InvariantCulture, ", Additional info: {0}", additionalInfo));
			mostSpecificInstance.SendTraceTag("00000GQ", Category.Extensions, Verbosity.Error, PrivacyClassification.OrganizationIdentifiableInformation, "The execution of the code block at {0} had one or more unhandled exceptions." + text, callerName);
			mostSpecificInstance.SendExceptionTag("00000GR", Category.Extensions, ex);
			if (exceptionMap != null)
			{
				throw exceptionMap(ex);
			}
			throw;
		}
	}

	public static T SendTraceTagOnFailure<T>(Func<T> operation, string additionalInfo = "", Func<Exception, Exception> exceptionMap = null, [CallerMemberName] string callerName = "")
	{
		if (operation == null)
		{
			throw new ArgumentNullException("operation");
		}
		T result = default;
		SendTraceTagOnFailure(() =>
		{
			result = operation();
		}, additionalInfo, exceptionMap, callerName);
		return result;
	}
}
