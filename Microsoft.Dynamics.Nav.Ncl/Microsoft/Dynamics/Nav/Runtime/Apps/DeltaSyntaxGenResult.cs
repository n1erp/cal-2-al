using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class DeltaSyntaxGenResult
{
	public Dictionary<SyntaxNodeIdentifier, SyntaxNode> SyntaxNodes { get; }

	public IEnumerable<NavXmlDeltaConversionDiagnostics> ConversionErrors { get; }

	public IEnumerable<NavXmlDeltaConversionDiagnostics> ConversionWarnings { get; }

	public DeltaSyntaxGenResult(Dictionary<SyntaxNodeIdentifier, SyntaxNode> syntaxNodes, IEnumerable<NavXmlDeltaConversionDiagnostics> conversionWarnings, IEnumerable<NavXmlDeltaConversionDiagnostics> conversionErrors)
	{
		SyntaxNodes = syntaxNodes ?? new Dictionary<SyntaxNodeIdentifier, SyntaxNode>();
		ConversionWarnings = conversionWarnings?.ToList() ?? new List<NavXmlDeltaConversionDiagnostics>();
		ConversionErrors = conversionErrors?.ToList() ?? new List<NavXmlDeltaConversionDiagnostics>();
	}
}
