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
internal class ReferencePageTypeSymbol : PageTypeSymbol
{
	private readonly Lazy<TableTypeSymbol> lazyRelatedTable;

	private readonly Lazy<ImmutableArray<VariableSymbol>> lazyGlobalVariables;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private ImmutableArray<Symbol> codeMembers;

	private Dictionary<string, ImmutableArray<Symbol>> codeMembersByName;

	public override int Id { get; }

	public override string Name { get; }

	internal override TypeSymbol BaseType => PageClassTypeSymbol.Instance;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => lazyGlobalVariables.Value;

	public override PageTypeKind PageType => this.GetEnumPropertyValueOrDefault<PageTypeKind>(PropertyKind.PageType);

	public override TableTypeSymbol RelatedTable => lazyRelatedTable.Value;

	internal ReferencePageTypeSymbol(ModuleSymbol module, PageDefinition page)
		: base(module)
	{
		ReferencePageTypeSymbol referencePageTypeSymbol = this;
		DebugAssertHelper.Assert(page != null);
		Id = page.Id ?? (-1);
		Name = page.Name().UnquoteIdentifier();
		lazyRelatedTable = ReferenceSymbolHelper.CreateLazy(GetRelatedTable);
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, page);
		lazyGlobalVariables = ReferenceSymbolHelper.BuildGlobalVariablesLazy(this, page);
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referencePageTypeSymbol.GetMembersLazy(page));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		if (codeMembers == null)
		{
			PooledHashSet<Symbol> instance = PooledHashSet<Symbol>.GetInstance();
			try
			{
				instance.AddAll(GetMembers());
				instance.AddAll(FlattenedControls.Where((ControlSymbol m) => !m.IsControlArea()));
				instance.AddAll(FlattenedActions.Where((ActionSymbol m) => !m.IsActionArea()));
				ImmutableArray<Symbol> value = instance.ToImmutableArray();
				ImmutableInterlocked.InterlockedCompareExchange(ref codeMembers, value, default);
			}
			finally
			{
				instance.Free();
			}
		}
		return codeMembers;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return GetCodeMembers(ref codeMembersByName, name);
	}

	private ImmutableArray<Symbol> GetMembersLazy(PageDefinition page)
	{
		IEnumerable<ReferenceMethodSymbol> items = page.Methods?.Select((MethodDefinition m) => new ReferenceMethodSymbol(this, m)) ?? SpecializedCollections.EmptyEnumerable<ReferenceMethodSymbol>();
		int lengthSafe = page.Methods.GetLengthSafe();
		IEnumerable<ReferenceActionSymbol> items2 = page.Actions?.Select((ActionDefinition a) => new ReferenceActionSymbol(this, a)) ?? SpecializedCollections.EmptyEnumerable<ReferenceActionSymbol>();
		int lengthSafe2 = page.Actions.GetLengthSafe();
		IEnumerable<ReferenceControlSymbol> items3 = page.Controls?.Select((ControlDefinition c) => new ReferenceControlSymbol(this, c)) ?? SpecializedCollections.EmptyEnumerable<ReferenceControlSymbol>();
		int lengthSafe3 = page.Controls.GetLengthSafe();
		IEnumerable<ReferenceViewSymbol> items4 = page.Views?.Select((ViewDefinition c) => new ReferenceViewSymbol(this, c)) ?? SpecializedCollections.EmptyEnumerable<ReferenceViewSymbol>();
		int lengthSafe4 = page.Views.GetLengthSafe();
		PageTypeKind enumPropertyValueOrDefault = this.GetEnumPropertyValueOrDefault<PageTypeKind>(PropertyKind.PageType);
		ImmutableArray<VariableSymbol> globalVariables = GlobalVariables;
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(globalVariables.Length + lengthSafe + lengthSafe2 + lengthSafe3 + lengthSafe4 + SyntaxFacts.ActionAreaValues.Length + SyntaxFacts.AreaValues.Length);
		try
		{
			instance.AddRange(globalVariables);
			instance.AddRange(items);
			instance.AddRange(items2);
			PageObjectMembers.AddStubActionAreas(this, enumPropertyValueOrDefault, instance);
			instance.AddRange(items3);
			PageObjectMembers.AddStubAreas(this, enumPropertyValueOrDefault, instance);
			instance.AddRange(items4);
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	private TableTypeSymbol GetRelatedTable()
	{
		PropertySymbol property = GetProperty(PropertyKind.SourceTable);
		if (property == null)
		{
			return null;
		}
		return ReferenceSymbolHelper.GetRelatedTableSymbol(this, property.ValueText);
	}
}
