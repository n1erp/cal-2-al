using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

internal class ModuleDependencyGraph
{
	private readonly IEnumerable<SymbolReferenceSpecification> referenceSpecifications;

	private readonly ImmutableDictionary<SymbolReferenceSpecification, ImmutableHashSet<SymbolReferenceSpecification>> referencesMap;

	private readonly NonReentrantLock dataLock = new NonReentrantLock();

	private IEnumerable<SymbolReferenceSpecification> lazyTopologicallySortedModules;

	public ModuleDependencyGraph(IEnumerable<SymbolReferenceSpecification> referenceSpecifications, ImmutableDictionary<SymbolReferenceSpecification, ImmutableHashSet<SymbolReferenceSpecification>> referencesMap)
	{
		this.referenceSpecifications = referenceSpecifications;
		this.referencesMap = referencesMap;
	}

	public IEnumerable<SymbolReferenceSpecification> GetTopologicallySortedModules(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (lazyTopologicallySortedModules == null)
		{
			using (dataLock.DisposableWait(cancellationToken))
			{
				ComputeTopologicallySortedModulesWithNoLock(cancellationToken);
			}
		}
		return lazyTopologicallySortedModules;
	}

	private void ComputeTopologicallySortedModulesWithNoLock(CancellationToken cancellationToken)
	{
		if (lazyTopologicallySortedModules == null)
		{
			HashSet<SymbolReferenceSpecification> seenModules = new HashSet<SymbolReferenceSpecification>(SymbolReferenceSpecification.VersionLessEqualityComparer);
			ArrayBuilder<SymbolReferenceSpecification> instance = ArrayBuilder<SymbolReferenceSpecification>.GetInstance();
			try
			{
				TopologicalSort(referenceSpecifications, seenModules, instance, cancellationToken);
				lazyTopologicallySortedModules = instance.ToImmutable();
			}
			finally
			{
				instance.Free();
			}
		}
	}

	private void TopologicalSort(IEnumerable<SymbolReferenceSpecification> moduleIdsToSort, HashSet<SymbolReferenceSpecification> seenModules, ArrayBuilder<SymbolReferenceSpecification> resultList, CancellationToken cancellationToken)
	{
		foreach (SymbolReferenceSpecification item in moduleIdsToSort)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (seenModules.Add(item))
			{
				if (referencesMap.TryGetValue(item, out var value))
				{
					TopologicalSort(value, seenModules, resultList, cancellationToken);
				}
				resultList.Add(item);
			}
		}
	}
}
