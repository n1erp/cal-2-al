using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class ConverterResult
{
	public IEnumerable<SyntaxNode> SyntaxNodes { get; }

	public IEnumerable<NavXmlDeltaConversionDiagnostics> ConversionErrors { get; }

	public IEnumerable<NavXmlDeltaConversionDiagnostics> ConversionWarnings { get; }

	public ConverterResult(IEnumerable<SyntaxNode> syntaxNodes, IEnumerable<NavXmlDeltaConversionDiagnostics> conversionWarnings, IEnumerable<NavXmlDeltaConversionDiagnostics> conversionErrors)
	{
		SyntaxNodes = syntaxNodes?.ToList() ?? new List<SyntaxNode>();
		ConversionWarnings = conversionWarnings?.ToList() ?? new List<NavXmlDeltaConversionDiagnostics>();
		ConversionErrors = conversionErrors?.ToList() ?? new List<NavXmlDeltaConversionDiagnostics>();
	}
}
