using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceKeySymbol : KeySymbol
{
	private readonly ImmutableArray<string> fieldNames;

	private readonly Lazy<ImmutableArray<FieldSymbol>> lazyFields;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	public override SymbolKind Kind { get; }

	public override string Name { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<FieldSymbol> Fields => lazyFields.Value;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	internal ReferenceKeySymbol(Symbol table, KeyDefinition key)
	{
		ContainingSymbol = table;
		Kind = SymbolKind.Key;
		Name = key.Name();
		fieldNames = key.FieldNames.AsImmutableOrEmpty();
		lazyFields = ReferenceSymbolHelper.CreateLazy(GetLazyFields);
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, key);
	}

	private ImmutableArray<FieldSymbol> GetLazyFields()
	{
		ApplicationObjectTypeSymbol applicationObjectTypeSymbol = ContainingSymbol as TableTypeSymbol;
		if (applicationObjectTypeSymbol == null)
		{
			applicationObjectTypeSymbol = ContainingSymbol as TableExtensionTypeSymbol;
		}
		IEnumerable<FieldSymbol> membersOfKind = applicationObjectTypeSymbol.GetMembersOfKind<FieldSymbol>(SymbolKind.Field);
		ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance(fieldNames.Length);
		int i;
		for (i = 0; i < fieldNames.Length; i++)
		{
			FieldSymbol fieldSymbol = membersOfKind.FirstOrDefault((FieldSymbol f) => SemanticFacts.IsSameName(f.Name, fieldNames[i]));
			if (fieldSymbol != null)
			{
				instance.Add(fieldSymbol);
			}
		}
		return instance.ToImmutableAndFree();
	}
}
