using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class MemoryCachedSymbolReferenceLoader : ISymbolReferenceLoader
{
	private readonly ISymbolReferenceLoader nextLoader;

	private readonly ConcurrentDictionary<SymbolReferenceSpecification, (ModuleDefinition, IList<Diagnostic>)> moduleCache = new ConcurrentDictionary<SymbolReferenceSpecification, (ModuleDefinition, IList<Diagnostic>)>();

	private readonly ConcurrentDictionary<SymbolReferenceSpecification, (IEnumerable<SymbolReferenceSpecification>, IList<Diagnostic>)> dependenciesCache = new ConcurrentDictionary<SymbolReferenceSpecification, (IEnumerable<SymbolReferenceSpecification>, IList<Diagnostic>)>();

	public MemoryCachedSymbolReferenceLoader(ISymbolReferenceLoader nextLoader)
	{
		if (nextLoader == null)
		{
			throw new ArgumentNullException("nextLoader");
		}
		this.nextLoader = nextLoader;
	}

	public ModuleDefinition LoadModule(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics)
	{
		ModuleDefinition moduleDefinition = FindMatchingModule(reference, diagnostics);
		if (moduleDefinition == null)
		{
			IList<Diagnostic> list = new List<Diagnostic>();
			moduleDefinition = nextLoader.LoadModule(reference, list);
			if (moduleDefinition != null)
			{
				moduleCache.TryAdd(reference, (moduleDefinition, list));
			}
			AddRange(diagnostics, list);
		}
		return moduleDefinition;
	}

	public IEnumerable<SymbolReferenceSpecification> GetDependencies(SymbolReferenceSpecification reference, IList<Diagnostic> diagnostics)
	{
		new List<SymbolReferenceSpecification>();
		IList<Diagnostic> list = new List<Diagnostic>();
		IEnumerable<SymbolReferenceSpecification> enumerable;
		if (dependenciesCache.TryGetValue(reference, out var value))
		{
			(enumerable, list) = value;
		}
		else
		{
			enumerable = nextLoader.GetDependencies(reference, list);
			dependenciesCache[reference] = (enumerable, list);
		}
		AddRange(diagnostics, list);
		return enumerable;
	}

	private ModuleDefinition FindMatchingModule(SymbolReferenceSpecification requestedReference, IList<Diagnostic> diagnostics)
	{
		ModuleDefinition result = null;
		SymbolReferenceSpecification symbolReferenceSpecification = null;
		foreach (SymbolReferenceSpecification key in moduleCache.Keys)
		{
			if (key.IsApplicable(requestedReference) && (symbolReferenceSpecification == null || SymbolReferenceSpecification.VersionComparer.Compare(key, symbolReferenceSpecification) > 0))
			{
				symbolReferenceSpecification = key;
			}
		}
		if (symbolReferenceSpecification != null && moduleCache.TryGetValue(symbolReferenceSpecification, out var value))
		{
			(result, _) = value;
			AddRange(diagnostics, value.Item2);
		}
		return result;
	}

	private static void AddRange(IList<Diagnostic> diagnostics, IList<Diagnostic> newDiagnostics)
	{
		foreach (Diagnostic newDiagnostic in newDiagnostics)
		{
			diagnostics.Add(newDiagnostic);
		}
	}
}
