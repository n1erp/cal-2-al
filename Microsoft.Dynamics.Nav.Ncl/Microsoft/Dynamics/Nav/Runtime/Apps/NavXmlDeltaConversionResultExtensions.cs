using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class NavXmlDeltaConversionResultExtensions
{
	public static IEnumerable<NavXmlDeltaConversionDiagnostics> Errors(this NavXmlDeltaConversionResult result)
	{
		return result?.Diagnostics?.Where((NavXmlDeltaConversionDiagnostics d) => d.IsError()) ?? Enumerable.Empty<NavXmlDeltaConversionDiagnostics>();
	}

	public static IEnumerable<NavXmlDeltaConversionDiagnostics> Warnings(this NavXmlDeltaConversionResult result)
	{
		return result?.Diagnostics?.Where((NavXmlDeltaConversionDiagnostics d) => d.IsWarning()) ?? Enumerable.Empty<NavXmlDeltaConversionDiagnostics>();
	}

	public static string FormatMessages(this IEnumerable<NavXmlDeltaConversionDiagnostics> diagnostics)
	{
		if (diagnostics != null)
		{
			return string.Join("," + Environment.NewLine, from d in diagnostics
				where !string.IsNullOrWhiteSpace(d?.Message)
				select d.Message);
		}
		return string.Empty;
	}
}
