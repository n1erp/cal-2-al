using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class ActionDefinition : LanguageElementWithProperties
{
	public ActionKind Kind { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ActionDefinition[] Actions { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	[Obsolete("This property was added to support a feature that was never completed and it should not be used.")]
	public MethodDefinition[] Methods { get; set; }
}
