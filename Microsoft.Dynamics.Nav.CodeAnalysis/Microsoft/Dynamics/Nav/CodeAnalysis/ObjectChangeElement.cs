namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class ObjectChangeElement
{
	public int? Id { get; set; }

	public SymbolKind Kind { get; set; }

	public string Name { get; set; }

	public string PackagePath { get; set; }
}
