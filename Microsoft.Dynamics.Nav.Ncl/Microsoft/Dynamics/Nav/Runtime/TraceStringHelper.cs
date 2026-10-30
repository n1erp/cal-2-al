using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class TraceStringHelper
{
	public static string GetStackTrace()
	{
		return "StackTrace:" + Environment.NewLine + new StackTrace(1, fNeedFileInfo: true);
	}

	[Conditional("DEBUG")]
	public static void GetStackTraceDebugOnly(ref string stackTrace)
	{
		stackTrace = "StackTrace:" + Environment.NewLine + new StackTrace(1, fNeedFileInfo: true);
	}

	public static string AppendStackTrace(string original)
	{
		return original + Environment.NewLine + "StackTrace:" + Environment.NewLine + new StackTrace(1, fNeedFileInfo: true);
	}

	public static string AppendStackTraceDebugOnly(string original)
	{
		return original;
	}

	internal static string CreateALFunctionStatementTrace(NavSession session, string alFunction)
	{
		NavMethodScope navMethodScope = session?.CurrentMethodScope;
		if (navMethodScope == null || navMethodScope.ApplicationObject == null)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} called without identifying information.", alFunction);
		}
		try
		{
			ALCodeEnvironment codeEnvironment = NavGlobal.NCLMetadata.CodeEnvironment;
			Tuple<int, string> tuple = ((session.Permissions != null) ? codeEnvironment.GetLineNumberAndStatement(navMethodScope) : null);
			ApplicationObjectId applicationObjectId = codeEnvironment.GetApplicationObjectId(navMethodScope);
			return string.Format(CultureInfo.InvariantCulture, "{0} called with this identifying information: Tenant ID: {1}, Session ID: {2}, Session Type: {3}, Object type: {4}, Object ID: {5}, Function name: {6}, Line number: {7}, Statement: {8} ", alFunction, session.Tenant?.Id, session.Id, session.ClientConnectionType, applicationObjectId.ObjectType, applicationObjectId.ObjectNumber, navMethodScope.ScopeName, tuple?.Item1, tuple?.Item2);
		}
		catch (NavBaseException ex)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} called with error: {1}", alFunction, ex.Message);
		}
	}
}
