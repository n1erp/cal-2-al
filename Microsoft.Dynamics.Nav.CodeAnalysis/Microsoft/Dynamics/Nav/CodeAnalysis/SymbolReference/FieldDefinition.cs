using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class FieldDefinition : LanguageElementWithProperties
{
	[SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
	public string Type { get; set; }

	public string OptionMembers { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
	public TypeDefinition TypeDefinition { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	[Obsolete("This property was added to support a feature that was never completed and it should not be used.")]
	public MethodDefinition[] Methods { get; set; }
}
