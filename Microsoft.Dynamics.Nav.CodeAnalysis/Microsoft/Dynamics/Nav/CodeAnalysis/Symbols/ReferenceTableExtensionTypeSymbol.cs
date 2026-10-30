using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class ReferenceTableExtensionTypeSymbol : TableExtensionTypeSymbol
{
	private readonly Lazy<ApplicationObjectTypeSymbol> lazyTarget;

	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<ImmutableArray<FieldSymbol>> lazyAddedFields;

	private readonly Lazy<ImmutableArray<KeySymbol>> lazyAddedKeys;

	private readonly Lazy<ImmutableArray<VariableSymbol>> lazyGlobalVariables;

	public override int Id { get; }

	public override string Name { get; }

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => ImmutableArray<PropertySymbol>.Empty;

	internal override bool RequiresCompletion => true;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => lazyGlobalVariables.Value;

	internal override ApplicationObjectTypeSymbol Target => lazyTarget.Value;

	internal ReferenceTableExtensionTypeSymbol(ModuleSymbol containingSymbol, TableExtensionDefinition tableExtensionDefinition)
		: base(containingSymbol)
	{
		ReferenceTableExtensionTypeSymbol referenceTableExtensionTypeSymbol = this;
		DebugAssertHelper.Assert(tableExtensionDefinition != null);
		Id = tableExtensionDefinition.Id ?? (-1);
		Name = tableExtensionDefinition.Name?.UnquoteIdentifier();
		lazyGlobalVariables = ReferenceSymbolHelper.BuildGlobalVariablesLazy(this, tableExtensionDefinition);
		lazyAddedKeys = ReferenceSymbolHelper.CreateLazy(() => tableExtensionDefinition.Keys.AsImmutableOrEmpty((Func<KeyDefinition, KeySymbol>)((KeyDefinition f) => new ReferenceKeySymbol(referenceTableExtensionTypeSymbol, f))));
		lazyAddedFields = ReferenceSymbolHelper.CreateLazy(() => tableExtensionDefinition.Fields.AsImmutableOrEmpty((Func<FieldDefinition, FieldSymbol>)((FieldDefinition f) => new ReferenceFieldSymbol(referenceTableExtensionTypeSymbol, f))));
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referenceTableExtensionTypeSymbol.GetMembersLazy(tableExtensionDefinition));
		lazyTarget = ReferenceSymbolHelper.CreateLazy(() => referenceTableExtensionTypeSymbol.LazyGetTarget(tableExtensionDefinition));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	internal override ImmutableArray<FieldSymbol> GetAddedFields()
	{
		return lazyAddedFields.Value;
	}

	internal override ImmutableArray<KeySymbol> GetAddedKeys()
	{
		return lazyAddedKeys.Value;
	}

	private ImmutableArray<Symbol> GetMembersLazy(TableExtensionDefinition tableExtension)
	{
		ImmutableArray<VariableSymbol> globalVariables = GlobalVariables;
		ImmutableArray<FieldSymbol> addedFields = GetAddedFields();
		ImmutableArray<KeySymbol> addedKeys = GetAddedKeys();
		IEnumerable<ReferenceMethodSymbol> items = tableExtension.Methods?.Select((MethodDefinition m) => new ReferenceMethodSymbol(this, m)) ?? SpecializedCollections.EmptyEnumerable<ReferenceMethodSymbol>();
		int lengthSafe = tableExtension.Methods.GetLengthSafe();
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(globalVariables.Length + lengthSafe + addedFields.Length + addedKeys.Length);
		instance.AddRange(globalVariables);
		instance.AddRange(items);
		instance.AddRange(addedFields);
		instance.AddRange(addedKeys);
		return instance.ToImmutableAndFree();
	}

	private ApplicationObjectTypeSymbol LazyGetTarget(TableExtensionDefinition tableExtension)
	{
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = ContainingModule.GetObjectSymbolsByNameAcrossModules(SymbolKind.Table, tableExtension.TargetObject);
		if (objectSymbolsByNameAcrossModules.Length == 1)
		{
			return (TableTypeSymbol)objectSymbolsByNameAcrossModules[0];
		}
		return null;
	}
}
