namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class TypeDefinition
{
	public string Name { get; set; }

	public Subtype Subtype { get; set; }

	public TypeDefinition[] TypeArguments { get; set; }

	public int[] ArrayDimensions { get; set; }

	public string[] OptionMembers { get; set; }
}
