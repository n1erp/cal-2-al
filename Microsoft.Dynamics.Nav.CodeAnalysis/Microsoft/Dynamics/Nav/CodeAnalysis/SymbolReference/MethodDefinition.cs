using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class MethodDefinition : LanguageElement, IElementWithAttributes, IElementWithParameters
{
	public string ReturnType { get; set; }

	public TypeDefinition ReturnTypeDefinition { get; set; }

	public bool? IsLocal { get; set; }

	public MethodKind? MethodKind { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	[Obsolete("This property was added to support a feature that was never completed and it should not be used.")]
	public VariableDefinition[] Variables { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ParameterDefinition[] Parameters { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public AttributeDefinition[] Attributes { get; set; }
}
