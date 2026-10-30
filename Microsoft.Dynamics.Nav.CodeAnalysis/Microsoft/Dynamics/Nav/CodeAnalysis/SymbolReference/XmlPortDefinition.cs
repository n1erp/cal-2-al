using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class XmlPortDefinition : LanguageElementWithProperties, IElementWithVariables
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public VariableDefinition[] Variables { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public MethodDefinition[] Methods { get; set; }
}
