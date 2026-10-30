using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class ModuleSymbol : ContainerSymbol, IModuleSymbol, ISymbol, IEquatable<ISymbol>
{
	private ImmutableDictionary<int, ImmutableArray<IApplicationObjectExtensionTypeSymbol>> lazyTableExtensions;

	private ImmutableDictionary<int, ImmutableArray<IApplicationObjectExtensionTypeSymbol>> lazyPageExtensions;

	private ImmutableDictionary<int, ImmutableArray<IApplicationObjectExtensionTypeSymbol>> lazyEnumExtensions;

	private readonly ConcurrentDictionary<PageTypeSymbol, TestPageTypeSymbol> testPageSymbolsCache = new ConcurrentDictionary<PageTypeSymbol, TestPageTypeSymbol>();

	public override SymbolKind Kind => SymbolKind.Module;

	public abstract string PublisherName { get; }

	public abstract Version Version { get; }

	public abstract Guid AppId { get; }

	internal virtual IReferenceResolver ReferenceResolver => null;

	internal abstract IDotNetResolver DotNetResolver { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override Symbol ContainingSymbol => null;

	internal override bool RequiresCompletion => true;

	public ImmutableArray<IModuleSymbol> ReferenceModules { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitModule(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitModule(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitModule(this, a);
	}

	protected ModuleSymbol(ImmutableArray<IModuleSymbol> referenceModules = default(ImmutableArray<IModuleSymbol>))
	{
		ReferenceModules = referenceModules;
	}

	public abstract ImmutableArray<ISymbol> GetDeclaredObjectSymbols();

	public IEnumerable<ISymbol> GetObjectSymbolsByKindAcrossModules(SymbolKind kind)
	{
		return ReferenceResolver.GetObjectSymbolsByKindAcrossModules(this, kind);
	}

	public ImmutableArray<ISymbolWithId> GetObjectSymbolsByIdAcrossModules(SymbolKind kind, int id)
	{
		return ReferenceResolver.GetObjectSymbolsByIdAcrossModules(this, kind, id);
	}

	public ImmutableArray<ISymbol> GetObjectSymbolsByNameAcrossModules(SymbolKind kind, string name)
	{
		return ReferenceResolver.GetObjectSymbolsByNameAcrossModules(this, kind, name);
	}

	public abstract ImmutableArray<ISymbol> GetObjectSymbols(SymbolKind kind);

	public abstract ISymbolWithId GetObjectSymbolById(SymbolKind kind, int id);

	public abstract ISymbol GetObjectSymbolByName(SymbolKind kind, string name);

	public ImmutableArray<IApplicationObjectExtensionTypeSymbol> GetApplicationObjectExtensionTypeSymbolsAcrossModules(IApplicationObjectTypeSymbol symbol)
	{
		if (symbol == null)
		{
			return ImmutableArray<IApplicationObjectExtensionTypeSymbol>.Empty;
		}
		ImmutableDictionary<int, ImmutableArray<IApplicationObjectExtensionTypeSymbol>> immutableDictionary = null;
		switch (symbol.Kind)
		{
		case SymbolKind.Table:
			immutableDictionary = GetExtensionKindGroupedByTargetAcrossModules(SymbolKind.TableExtension, ref lazyTableExtensions);
			break;
		case SymbolKind.Page:
		case SymbolKind.PageReference:
			immutableDictionary = GetExtensionKindGroupedByTargetAcrossModules(SymbolKind.PageExtension, SymbolKind.PageCustomization, ref lazyPageExtensions);
			break;
		case SymbolKind.Enum:
			immutableDictionary = GetExtensionKindGroupedByTargetAcrossModules(SymbolKind.EnumExtension, ref lazyEnumExtensions);
			break;
		}
		if (immutableDictionary != null && immutableDictionary.TryGetValue(symbol.Id, out var value))
		{
			return value;
		}
		return ImmutableArray<IApplicationObjectExtensionTypeSymbol>.Empty;
	}

	internal TestPageTypeSymbol GetTestPage(PageTypeSymbol page)
	{
		return testPageSymbolsCache.GetOrAdd(page, (PageTypeSymbol p) => TestPageTypeSymbol.Create(this, p));
	}

	internal TestRequestPageTypeSymbol GetTestRequestPage(RequestPageTypeSymbol requestPage)
	{
		return requestPage?.GetTestRequestPage();
	}

	internal RecordTypeSymbol GetRecord(TableTypeSymbol table, bool isTemporary)
	{
		return table.GetRecord(isTemporary);
	}

	internal PageReferenceTypeSymbol GetPageReference(PageTypeSymbol page)
	{
		return page.GetPageReference();
	}

	private ImmutableDictionary<int, ImmutableArray<IApplicationObjectExtensionTypeSymbol>> GetExtensionKindGroupedByTargetAcrossModules(SymbolKind kind, ref ImmutableDictionary<int, ImmutableArray<IApplicationObjectExtensionTypeSymbol>> cachedDictionary)
	{
		if (cachedDictionary == null)
		{
			Interlocked.CompareExchange(ref cachedDictionary, (from IApplicationObjectExtensionTypeSymbol ext in GetObjectSymbolsByKindAcrossModules(kind)
				where ext.Target != null
				group ext by ext.Target.Id).ToImmutableDictionary((IGrouping<int, IApplicationObjectExtensionTypeSymbol> g) => g.Key, (IGrouping<int, IApplicationObjectExtensionTypeSymbol> g) => g.ToImmutableArrayOrEmpty()), null);
		}
		return cachedDictionary;
	}

	private ImmutableDictionary<int, ImmutableArray<IApplicationObjectExtensionTypeSymbol>> GetExtensionKindGroupedByTargetAcrossModules(SymbolKind kind, SymbolKind alternativeKind, ref ImmutableDictionary<int, ImmutableArray<IApplicationObjectExtensionTypeSymbol>> cachedDictionary)
	{
		if (cachedDictionary == null)
		{
			ImmutableDictionary<int, ImmutableArray<IApplicationObjectExtensionTypeSymbol>> value = (from IApplicationObjectExtensionTypeSymbol ext in GetObjectSymbolsByKindAcrossModules(kind).Concat(GetObjectSymbolsByKindAcrossModules(alternativeKind))
				where ext.Target != null
				group ext by ext.Target.Id).ToImmutableDictionary((IGrouping<int, IApplicationObjectExtensionTypeSymbol> g) => g.Key, (IGrouping<int, IApplicationObjectExtensionTypeSymbol> g) => g.ToImmutableArrayOrEmpty());
			Interlocked.CompareExchange(ref cachedDictionary, value, null);
		}
		return cachedDictionary;
	}

	protected static ISymbol GetObjectSymbolByName(ImmutableDictionary<SymbolKind, ImmutableDictionary<string, ISymbol>> objectsByName, SymbolKind kind, string name)
	{
		if (name == null)
		{
			return null;
		}
		SymbolKind key = kind;
		if (kind == SymbolKind.DotNet)
		{
			key = SymbolKind.DotNetTypeDeclaration;
		}
		if (!objectsByName.TryGetValue(key, out var value))
		{
			return null;
		}
		if (!value.TryGetValue(name, out var value2))
		{
			return null;
		}
		if (kind != SymbolKind.DotNet)
		{
			return value2;
		}
		return ((DotNetTypeDeclarationSymbol)value2).DotNetType;
	}
}
