using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class TestPageMembersBuilder : IDisposable
{
	private readonly ModuleSymbol containingModule;

	private readonly Symbol containingSymbol;

	private PooledNameComparisonHashSet names;

	private ArrayBuilder<Symbol> builder;

	public TestPageMembersBuilder(ModuleSymbol containingModule, Symbol containingSymbol)
	{
		this.containingModule = containingModule;
		this.containingSymbol = containingSymbol;
		names = PooledNameComparisonHashSet.GetInstance();
		builder = ArrayBuilder<Symbol>.GetInstance();
	}

	public void AddPageElements(PageTypeSymbol target, bool addPageParts)
	{
		AddControls(target, addPageParts);
		AddActions(target);
		AddTestFilterSymbol(target);
	}

	public void AddSymbol(Symbol s)
	{
		AddIfMissing(s);
	}

	public ImmutableArray<Symbol> ToImmutable()
	{
		return builder.ToImmutable();
	}

	public void Dispose()
	{
		builder.Free();
		builder = null;
		names.Free();
		names = null;
	}

	private void AddControls(PageTypeSymbol target, bool addPageParts)
	{
		AddControls(target.FlattenedControls, addPageParts);
		foreach (PageExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in containingModule.GetApplicationObjectExtensionTypeSymbolsAcrossModules(target))
		{
			AddControls(applicationObjectExtensionTypeSymbolsAcrossModule.FlattenedControls, addPageParts);
		}
	}

	private void AddActions(PageTypeSymbol target)
	{
		AddActions(target.FlattenedActions);
		foreach (PageExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in containingModule.GetApplicationObjectExtensionTypeSymbolsAcrossModules(target))
		{
			AddActions(applicationObjectExtensionTypeSymbolsAcrossModule.FlattenedActions);
		}
	}

	private void AddTestFilterSymbol(PageTypeSymbol target)
	{
		if (target.RelatedTable != null)
		{
			AddIfMissing(new TestFilterSymbol(containingSymbol, target.Id, "Filter", target, target.RelatedTable));
		}
	}

	private void AddActions(ImmutableArray<ActionSymbol> actions)
	{
		foreach (ActionSymbol item in actions)
		{
			if (item.ActionKind == ActionKind.Action)
			{
				AddIfMissing(new TestActionSymbol(containingSymbol, item));
			}
		}
	}

	private void AddControls(ImmutableArray<ControlSymbol> controls, bool addPageParts)
	{
		foreach (ControlSymbol item in controls)
		{
			switch (item.ControlKind)
			{
			case ControlKind.Field:
			case ControlKind.Label:
				AddIfMissing(new TestFieldSymbol(containingSymbol, item));
				break;
			case ControlKind.Part:
				if (addPageParts)
				{
					PageTypeSymbol relatedPartSymbol = item.RelatedPartSymbol;
					if (relatedPartSymbol != null)
					{
						AddIfMissing(TestPartSymbol.Create(containingSymbol, item, relatedPartSymbol));
					}
				}
				break;
			}
		}
	}

	private void AddIfMissing(Symbol s)
	{
		if (!names.Contains(s.Name))
		{
			names.Add(s.Name);
			builder.Add(s);
		}
	}
}
