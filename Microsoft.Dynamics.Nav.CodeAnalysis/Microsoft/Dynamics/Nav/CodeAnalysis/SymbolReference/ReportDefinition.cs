using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class ReportDefinition : LanguageElementWithProperties, IElementWithVariables
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public VariableDefinition[] Variables { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public MethodDefinition[] Methods { get; set; }

	public RequestPageDefinition RequestPage { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ReportDataItemDefinition[] DataItems { get; set; }
}
