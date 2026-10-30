using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferencePropertySymbol : PropertySymbol
{
	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override PropertyKind PropertyKind { get; }

	internal override BoundProperty Property => null;

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	internal override object Value { get; }

	public override string ValueText
	{
		get
		{
			if (Value != null)
			{
				return Value.ToString();
			}
			return string.Empty;
		}
	}

	internal ReferencePropertySymbol(Symbol containingSymbol, PropertyDefinition property)
	{
		ContainingSymbol = containingSymbol;
		PropertyKind = (Enum.TryParse<PropertyKind>(property.Name, ignoreCase: true, out var result) ? result : PropertyKind.None);
		Name = property.Name();
		Value = property.Value;
	}
}
