using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class ParameterDefinition
{
	public bool? IsVar { get; set; }

	public string Name { get; set; }

	public TypeDefinition TypeDefinition { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
	public string Type { get; set; }

	public string ArrayDimensions { get; set; }

	public string OptionMembers { get; set; }

	public Subtype Subtype { get; set; }
}
