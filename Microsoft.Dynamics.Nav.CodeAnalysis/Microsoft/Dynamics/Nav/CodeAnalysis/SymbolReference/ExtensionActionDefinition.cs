using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class ExtensionActionDefinition
{
	public string Anchor { get; set; }

	public ChangeKind ChangeKind { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ActionDefinition[] Actions { get; set; }
}
