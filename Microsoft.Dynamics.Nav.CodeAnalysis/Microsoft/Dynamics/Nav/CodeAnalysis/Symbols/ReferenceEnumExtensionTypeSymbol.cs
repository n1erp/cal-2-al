using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class ReferenceEnumExtensionTypeSymbol : EnumExtensionTypeSymbol
{
	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<ImmutableArray<EnumValueSymbol>> lazyEnumValues;

	private readonly Lazy<EnumTypeSymbol> lazyTarget;

	public override int Id { get; }

	public override string Name { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal override ApplicationObjectTypeSymbol Target => lazyTarget.Value;

	public override ImmutableArray<EnumValueSymbol> Values => lazyEnumValues.Value;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public static ReferenceEnumExtensionTypeSymbol Create(ModuleSymbol containingModule, EnumExtensionTypeDefinition definition)
	{
		return new ReferenceEnumExtensionTypeSymbol(containingModule, definition);
	}

	private ReferenceEnumExtensionTypeSymbol(ModuleSymbol containingModule, EnumExtensionTypeDefinition definition)
		: base(containingModule)
	{
		ReferenceEnumExtensionTypeSymbol referenceEnumExtensionTypeSymbol = this;
		Id = definition.Id.Value;
		Name = definition.Name.UnquoteIdentifier();
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, definition);
		lazyEnumValues = ReferenceSymbolHelper.CreateLazy(() => referenceEnumExtensionTypeSymbol.GetEnumValuesLazy(definition));
		lazyTarget = ReferenceSymbolHelper.CreateLazy(() => referenceEnumExtensionTypeSymbol.GetTarget(definition));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return StaticCast<Symbol>.From(Values);
	}

	private ImmutableArray<EnumValueSymbol> GetEnumValuesLazy(EnumExtensionTypeDefinition definition)
	{
		return definition.Values.AsImmutableOrEmpty((Func<EnumValueDefinition, EnumValueSymbol>)((EnumValueDefinition v) => ReferenceEnumValueSymbol.Create(this, v.Name, v.Ordinal)));
	}

	private EnumTypeSymbol GetTarget(EnumExtensionTypeDefinition definition)
	{
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = ContainingModule.GetObjectSymbolsByNameAcrossModules(SymbolKind.Enum, definition.TargetObject);
		if (objectSymbolsByNameAcrossModules.Length == 1)
		{
			return objectSymbolsByNameAcrossModules[0] as EnumTypeSymbol;
		}
		return null;
	}
}
