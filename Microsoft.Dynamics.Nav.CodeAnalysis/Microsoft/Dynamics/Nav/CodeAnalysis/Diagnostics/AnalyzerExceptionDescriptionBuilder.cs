using System;
using System.IO;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal static class AnalyzerExceptionDescriptionBuilder
{
	private static readonly string separator = Environment.NewLine + "-----" + Environment.NewLine;

	public static string CreateDiagnosticDescription(this Exception exception)
	{
		if (exception is AggregateException ex)
		{
			AggregateException ex2 = ex.Flatten();
			return string.Join(separator, ex2.InnerExceptions.Select(GetExceptionMessage));
		}
		if (exception != null)
		{
			return string.Join(separator, GetExceptionMessage(exception), exception.InnerException.CreateDiagnosticDescription());
		}
		return string.Empty;
	}

	private static string GetExceptionMessage(Exception exception)
	{
		if (!(exception is FileNotFoundException ex))
		{
			return exception.ToString();
		}
		return string.Join(separator, ex.Message);
	}
}
