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
internal sealed class ReferencePageCustomizationTypeSymbol : PageCustomizationTypeSymbol
{
	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<ApplicationObjectTypeSymbol> lazyTarget;

	private Dictionary<string, ImmutableArray<Symbol>> codeMembersByName;

	private ImmutableArray<Symbol> codeMembers;

	public override int Id { get; }

	public override string Name { get; }

	internal override bool RequiresCompletion => true;

	internal override ApplicationObjectTypeSymbol Target => lazyTarget.Value;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => ImmutableArray<PropertySymbol>.Empty;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => ImmutableArray<VariableSymbol>.Empty;

	internal ReferencePageCustomizationTypeSymbol(ModuleSymbol containingSymbol, PageCustomizationDefinition pageCustomization)
		: base(containingSymbol)
	{
		ReferencePageCustomizationTypeSymbol referencePageCustomizationTypeSymbol = this;
		DebugAssertHelper.Assert(pageCustomization != null);
		Id = pageCustomization.Id ?? (-1);
		Name = pageCustomization.Name?.UnquoteIdentifier();
		lazyTarget = ReferenceSymbolHelper.CreateLazy(() => referencePageCustomizationTypeSymbol.GetTargetLazy(pageCustomization));
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referencePageCustomizationTypeSymbol.GetMembersLazy(pageCustomization));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		if (codeMembers == null)
		{
			ImmutableInterlocked.InterlockedCompareExchange(ref codeMembers, GetMembers().Concat(FlattenedViews).ToImmutableArray(), default);
		}
		return codeMembers;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return GetCodeMembers(ref codeMembersByName, name);
	}

	private ImmutableArray<Symbol> GetMembersLazy(PageCustomizationDefinition pageCustomization)
	{
		int lengthSafe = pageCustomization.ViewChanges.GetLengthSafe();
		IEnumerable<Symbol> enumerable = pageCustomization.ViewChanges?.Select((ExtensionViewDefinition viewChange) => new ReferenceChangeAddViewSymbol(this, viewChange));
		IEnumerable<Symbol> items = enumerable ?? SpecializedCollections.EmptyEnumerable<Symbol>();
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(lengthSafe);
		instance.AddRange(items);
		return instance.ToImmutableAndFree();
	}

	private ApplicationObjectTypeSymbol GetTargetLazy(PageCustomizationDefinition pageCustomization)
	{
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = ContainingModule.ReferenceResolver.GetObjectSymbolsByNameAcrossModules(ContainingModule, SymbolKind.Page, pageCustomization.TargetObject);
		if (objectSymbolsByNameAcrossModules.Length == 1)
		{
			return objectSymbolsByNameAcrossModules[0] as PageTypeSymbol;
		}
		return null;
	}
}
