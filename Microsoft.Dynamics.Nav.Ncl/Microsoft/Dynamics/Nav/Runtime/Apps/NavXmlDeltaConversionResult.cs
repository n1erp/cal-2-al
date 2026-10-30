using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavXmlDeltaConversionResult
{
	public ConversionType ConversionType { get; }

	public IEnumerable<NavXmlDeltaPageConversionResult> ConvertedPages { get; }

	public IEnumerable<NavXmlDeltaConversionDiagnostics> Diagnostics { get; }

	public bool HadErrors { get; }

	public bool HadWarnings { get; }

	private NavXmlDeltaConversionResult(ConversionType type, IEnumerable<NavXmlDeltaPageConversionResult> pages, IEnumerable<NavXmlDeltaConversionDiagnostics> diagnostics)
	{
		ConversionType = type;
		ConvertedPages = pages;
		Diagnostics = diagnostics.Concat(ConvertedPages.SelectMany((NavXmlDeltaPageConversionResult x) => x.Diagnostics));
		HadErrors = Diagnostics.Any((NavXmlDeltaConversionDiagnostics e) => e.Severity.In(Verbosity.Error, Verbosity.Critical));
		HadWarnings = Diagnostics.Any((NavXmlDeltaConversionDiagnostics e) => e.Severity == Verbosity.Warning);
	}

	public static NavXmlDeltaConversionResult Create(ConversionType type, IEnumerable<NavXmlDeltaPageConversionResult> convertedPages, IEnumerable<NavXmlDeltaConversionDiagnostics> diagnostics)
	{
		return new NavXmlDeltaConversionResult(type, convertedPages, diagnostics ?? new List<NavXmlDeltaConversionDiagnostics>());
	}
}
