using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class VariableDefinition : LanguageElement, IElementWithAttributes
{
	[SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
	public string Type { get; set; }

	public string OptionMembers { get; set; }

	public Subtype Subtype { get; set; }

	public string ArrayDimensions { get; set; }

	public TypeDefinition TypeDefinition { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public AttributeDefinition[] Attributes { get; set; }
}
