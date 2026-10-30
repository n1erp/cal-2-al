using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class NonMissingAssemblySymbol : AssemblySymbol
{
	private readonly ConcurrentDictionary<MetadataTypeName.Key, NamedTypeSymbol> _emittedNameToTypeMap = new ConcurrentDictionary<MetadataTypeName.Key, NamedTypeSymbol>();

	private NamespaceSymbol _globalNamespace;

	internal sealed override bool IsMissing => false;

	public sealed override NamespaceSymbol GlobalNamespace
	{
		get
		{
			if ((object)_globalNamespace == null)
			{
				IEnumerable<NamespaceSymbol> items = Modules.Select((ModuleSymbol m) => m.GlobalNamespace);
				NamespaceSymbol value = MergedNamespaceSymbol.Create(new NamespaceExtent(this), null, items.AsImmutable());
				Interlocked.CompareExchange(ref _globalNamespace, value, null);
			}
			return _globalNamespace;
		}
	}

	internal int EmittedNameToTypeMapCount => _emittedNameToTypeMap.Count;

	internal sealed override NamedTypeSymbol LookupTopLevelMetadataTypeWithCycleDetection(ref MetadataTypeName emittedName, ConsList<AssemblySymbol> visitedAssemblies, bool digThroughForwardedTypes)
	{
		NamedTypeSymbol namedTypeSymbol = LookupTopLevelMetadataTypeInCache(ref emittedName);
		if ((object)namedTypeSymbol != null)
		{
			if (digThroughForwardedTypes || (!namedTypeSymbol.IsErrorType() && (object)namedTypeSymbol.ContainingAssembly == this))
			{
				return namedTypeSymbol;
			}
			return new MissingMetadataTypeSymbol.TopLevel(Modules[0], ref emittedName);
		}
		ImmutableArray<ModuleSymbol> modules = Modules;
		int length = modules.Length;
		int i = 0;
		namedTypeSymbol = modules[i].LookupTopLevelMetadataType(ref emittedName);
		if (namedTypeSymbol is MissingMetadataTypeSymbol)
		{
			for (i = 1; i < length; i++)
			{
				NamedTypeSymbol namedTypeSymbol2 = modules[i].LookupTopLevelMetadataType(ref emittedName);
				if (!(namedTypeSymbol2 is MissingMetadataTypeSymbol))
				{
					namedTypeSymbol = namedTypeSymbol2;
					break;
				}
			}
		}
		bool flag = i < length;
		if (!flag & digThroughForwardedTypes)
		{
			NamedTypeSymbol namedTypeSymbol3 = TryLookupForwardedMetadataTypeWithCycleDetection(ref emittedName, visitedAssemblies);
			if ((object)namedTypeSymbol3 != null)
			{
				namedTypeSymbol = namedTypeSymbol3;
			}
		}
		if (digThroughForwardedTypes | flag)
		{
			CacheTopLevelMetadataType(ref emittedName, namedTypeSymbol);
		}
		return namedTypeSymbol;
	}

	internal abstract override NamedTypeSymbol TryLookupForwardedMetadataTypeWithCycleDetection(ref MetadataTypeName emittedName, ConsList<AssemblySymbol> visitedAssemblies);

	private NamedTypeSymbol LookupTopLevelMetadataTypeInCache(ref MetadataTypeName emittedName)
	{
		NamedTypeSymbol value = null;
		if (_emittedNameToTypeMap.TryGetValue(emittedName.ToKey(), out value))
		{
			return value;
		}
		return null;
	}

	internal NamedTypeSymbol CachedTypeByEmittedName(string emittedname)
	{
		MetadataTypeName metadataTypeName = MetadataTypeName.FromFullName(emittedname);
		return _emittedNameToTypeMap[metadataTypeName.ToKey()];
	}

	private void CacheTopLevelMetadataType(ref MetadataTypeName emittedName, NamedTypeSymbol result)
	{
		_emittedNameToTypeMap.GetOrAdd(emittedName.ToKey(), result);
	}
}
