using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class LanguageElementWithProperties : LanguageElement
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public PropertyDefinition[] Properties { get; set; }
}
