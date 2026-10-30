using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class DeclarationTable
{
	private class Cache
	{
		internal readonly Lazy<MergedModuleDeclaration> MergedRoot;

		public Cache(DeclarationTable table)
		{
			MergedRoot = new Lazy<MergedModuleDeclaration>(() => MergedModuleDeclaration.Create(((IEnumerable<SingleObjectCollectionDeclaration>)table.allOlderRootDeclarations.InInsertionOrder).ToImmutableArray()));
		}
	}

	public static readonly DeclarationTable Empty = new DeclarationTable(ImmutableSetWithInsertionOrder<RootObjectCollectionDeclaration>.Empty, null, null);

	private readonly ImmutableSetWithInsertionOrder<RootObjectCollectionDeclaration> allOlderRootDeclarations;

	private readonly Lazy<RootObjectCollectionDeclaration> latestLazyRootDeclaration;

	private readonly Cache cache;

	private readonly Lazy<MergedModuleDeclaration> mergedRoot;

	public MergedModuleDeclaration MergedRoot => mergedRoot.Value;

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cache")]
	private DeclarationTable(ImmutableSetWithInsertionOrder<RootObjectCollectionDeclaration> allOlderRootDeclarationsParam, Lazy<RootObjectCollectionDeclaration> latestLazyRootDeclarationParam, Cache cache)
	{
		allOlderRootDeclarations = allOlderRootDeclarationsParam;
		latestLazyRootDeclaration = latestLazyRootDeclarationParam;
		this.cache = this.cache ?? new Cache(this);
		mergedRoot = new Lazy<MergedModuleDeclaration>(GetMergedRoot);
	}

	public DeclarationTable AddRootDeclaration(Lazy<RootObjectCollectionDeclaration> lazyRootDeclaration)
	{
		if (latestLazyRootDeclaration == null)
		{
			return new DeclarationTable(allOlderRootDeclarations, lazyRootDeclaration, cache);
		}
		return new DeclarationTable(allOlderRootDeclarations.Add(latestLazyRootDeclaration.Value), lazyRootDeclaration, null);
	}

	public DeclarationTable RemoveRootDeclaration(Lazy<RootObjectCollectionDeclaration> lazyRootDeclaration)
	{
		if (latestLazyRootDeclaration == lazyRootDeclaration)
		{
			return new DeclarationTable(allOlderRootDeclarations, null, cache);
		}
		return new DeclarationTable(allOlderRootDeclarations.Remove(lazyRootDeclaration.Value), latestLazyRootDeclaration, null);
	}

	public IEnumerable<RootObjectCollectionDeclaration> AllRootObjectCollectionsUnordered()
	{
		if (latestLazyRootDeclaration == null)
		{
			return allOlderRootDeclarations;
		}
		return allOlderRootDeclarations.Add(latestLazyRootDeclaration.Value);
	}

	private MergedModuleDeclaration GetMergedRoot()
	{
		MergedModuleDeclaration value = cache.MergedRoot.Value;
		if (latestLazyRootDeclaration == null)
		{
			return value;
		}
		if (value == null)
		{
			return MergedModuleDeclaration.Create(latestLazyRootDeclaration.Value);
		}
		return MergedModuleDeclaration.Create(value, latestLazyRootDeclaration.Value);
	}
}
