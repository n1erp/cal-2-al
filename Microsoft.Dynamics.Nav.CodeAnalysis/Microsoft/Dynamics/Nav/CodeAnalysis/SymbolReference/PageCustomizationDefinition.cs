using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class PageCustomizationDefinition : LanguageElement
{
	public string TargetObject { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ExtensionControlDefinition[] ControlChanges { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ExtensionActionDefinition[] ActionChanges { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ExtensionViewDefinition[] ViewChanges { get; set; }
}
