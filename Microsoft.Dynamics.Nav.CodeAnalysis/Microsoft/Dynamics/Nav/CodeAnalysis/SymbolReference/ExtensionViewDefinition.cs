using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class ExtensionViewDefinition
{
	public string Anchor { get; set; }

	public ChangeKind ChangeKind { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ViewDefinition[] Views { get; set; }
}
