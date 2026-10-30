using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavXmlDeltaPageConversionResult
{
	public int PageId { get; set; }

	public ObjectSyntax ConvertedObject { get; set; }

	public IEnumerable<NavXmlDeltaConversionDiagnostics> Diagnostics { get; set; }

	public bool HadErrors => Diagnostics.Any((NavXmlDeltaConversionDiagnostics e) => e.Severity.In(Verbosity.Error, Verbosity.Critical));

	public bool HadWarnings => Diagnostics.Any((NavXmlDeltaConversionDiagnostics e) => e.Severity == Verbosity.Warning);

	internal NavXmlDeltaPageConversionResult(int pageId, ObjectSyntax objectSyntax = null, IEnumerable<NavXmlDeltaConversionDiagnostics> diagnostics = null)
	{
		PageId = pageId;
		ConvertedObject = objectSyntax;
		Diagnostics = diagnostics ?? new List<NavXmlDeltaConversionDiagnostics>();
	}
}
