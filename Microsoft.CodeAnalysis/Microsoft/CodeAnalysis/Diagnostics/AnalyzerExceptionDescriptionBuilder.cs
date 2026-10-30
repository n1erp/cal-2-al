using System;
using System.IO;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal static class AnalyzerExceptionDescriptionBuilder
{
	private static readonly string s_separator = Environment.NewLine + "-----" + Environment.NewLine;

	public static string CreateDiagnosticDescription(this Exception exception)
	{
		if (exception is AggregateException ex)
		{
			AggregateException ex2 = ex.Flatten();
			return string.Join(s_separator, ex2.InnerExceptions.Select((Exception e) => GetExceptionMessage(e)));
		}
		if (exception != null)
		{
			return string.Join(s_separator, GetExceptionMessage(exception), exception.InnerException.CreateDiagnosticDescription());
		}
		return string.Empty;
	}

	private static string GetExceptionMessage(Exception exception)
	{
		if (!(exception is FileNotFoundException ex))
		{
			return exception.ToString();
		}
		string text = DesktopShim.FileNotFoundException.TryGetFusionLog(ex);
		if (text == null)
		{
			return exception.ToString();
		}
		return string.Join(s_separator, ex.Message, text);
	}
}
