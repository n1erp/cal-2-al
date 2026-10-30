using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceTableTypeSymbol : TableTypeSymbol
{
	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<ImmutableArray<VariableSymbol>> lazyGlobalVariables;

	private readonly Lazy<ImmutableArray<KeySymbol>> lazyKeys;

	private readonly Lazy<ImmutableArray<FieldSymbol>> lazyFields;

	private readonly Lazy<ImmutableArray<FieldGroupSymbol>> lazyFieldGroups;

	public override int Id { get; }

	public override string Name { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override TableTypeKind TableType => this.GetEnumPropertyValueOrDefault<TableTypeKind>(PropertyKind.TableType);

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => lazyGlobalVariables.Value;

	internal ReferenceTableTypeSymbol(ModuleSymbol module, TableDefinition table)
		: base(module)
	{
		ReferenceTableTypeSymbol referenceTableTypeSymbol = this;
		DebugAssertHelper.Assert(table != null);
		Id = table.Id ?? (-1);
		Name = table.Name().UnquoteIdentifier();
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, table);
		lazyGlobalVariables = ReferenceSymbolHelper.BuildGlobalVariablesLazy(this, table);
		lazyKeys = ReferenceSymbolHelper.CreateLazy(() => table.Keys.AsImmutableOrEmpty((Func<KeyDefinition, KeySymbol>)((KeyDefinition p) => new ReferenceKeySymbol(referenceTableTypeSymbol, p))));
		lazyFields = ReferenceSymbolHelper.CreateLazy(() => table.Fields.AsImmutableOrEmpty((Func<FieldDefinition, FieldSymbol>)((FieldDefinition p) => new ReferenceFieldSymbol(referenceTableTypeSymbol, p))));
		lazyFieldGroups = ReferenceSymbolHelper.CreateLazy(() => table.FieldGroups.AsImmutableOrEmpty((Func<FieldGroupDefinition, FieldGroupSymbol>)((FieldGroupDefinition m) => new ReferenceFieldGroupSymbol(referenceTableTypeSymbol, m))));
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referenceTableTypeSymbol.GetMembersLazy(table));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	internal override ImmutableArray<FieldSymbol> GetFields()
	{
		return lazyFields.Value;
	}

	internal override ImmutableArray<KeySymbol> GetKeys()
	{
		return lazyKeys.Value;
	}

	internal override ImmutableArray<FieldGroupSymbol> GetFieldGroups()
	{
		return lazyFieldGroups.Value;
	}

	private ImmutableArray<Symbol> GetMembersLazy(TableDefinition table)
	{
		ImmutableArray<VariableSymbol> globalVariables = GlobalVariables;
		ImmutableArray<FieldSymbol> fields = GetFields();
		int lengthSafe = table.Methods.GetLengthSafe();
		IEnumerable<ReferenceMethodSymbol> items = table.Methods?.Select((MethodDefinition m) => new ReferenceMethodSymbol(this, m)) ?? SpecializedCollections.EmptyEnumerable<ReferenceMethodSymbol>();
		ImmutableArray<KeySymbol> keys = GetKeys();
		ImmutableArray<FieldGroupSymbol> fieldGroups = GetFieldGroups();
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(globalVariables.Length + lengthSafe + fields.Length + keys.Length + fieldGroups.Length);
		instance.AddRange(globalVariables);
		instance.AddRange(items);
		instance.AddRange(fields);
		instance.AddRange(keys);
		instance.AddRange(fieldGroups);
		return instance.ToImmutableAndFree();
	}
}
