using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.HybridDeploy;

internal static class HybridDeployDiagnosticSession
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
			string text = (string.IsNullOrWhiteSpace(additionalInfo) ? string.Empty : (", Additional info: " + additionalInfo));
			mostSpecificInstance.SendExceptionTag("000023M", Category.HybridDeploy, ex, Verbosity.Error, DiagnosticParameter.Oii(string.Format(CultureInfo.InvariantCulture, "The execution of the code block at {0} had one or more unhandled exceptions." + text, callerName)));
			if (exceptionMap != null)
			{
				throw exceptionMap(ex);
			}
			throw;
		}
	}

	public static T SendTraceTagOnFailure<T>(Func<T> operation, [CallerMemberName] string callerName = "")
	{
		if (operation == null)
		{
			throw new ArgumentNullException("operation");
		}
		T result = default;
		SendTraceTagOnFailure(() =>
		{
			result = operation();
		}, FormatAdditionalInfo(), WrapInALException, callerName);
		return result;
	}

	private static string FormatAdditionalInfo()
	{
		return "Tenant:" + NavCurrentThread.Session?.Tenant?.Id;
	}

	private static Exception WrapInALException(Exception e)
	{
		return (e as NavALException) ?? new NavALException(e.Message);
	}
}
