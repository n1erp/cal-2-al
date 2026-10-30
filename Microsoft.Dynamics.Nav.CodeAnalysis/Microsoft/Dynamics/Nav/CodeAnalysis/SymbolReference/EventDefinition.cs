using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class EventDefinition : LanguageElement, IElementWithAttributes, IElementWithParameters
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ParameterDefinition[] Parameters { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public AttributeDefinition[] Attributes { get; set; }
}
