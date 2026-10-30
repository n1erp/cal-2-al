using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class PEAssembly
{
	internal readonly ImmutableArray<AssemblyIdentity> AssemblyReferences;

	internal readonly ImmutableArray<int> ModuleReferenceCounts;

	private readonly ImmutableArray<PEModule> _modules;

	private readonly AssemblyIdentity _identity;

	private ThreeState _lazyContainsNoPiaLocalTypes;

	private ThreeState _lazyDeclaresTheObjectClass;

	private readonly AssemblyMetadata _owner;

	private Dictionary<string, List<ImmutableArray<byte>>> _lazyInternalsVisibleToMap;

	internal EntityHandle Handle => EntityHandle.AssemblyDefinition;

	internal PEModule ManifestModule => Modules[0];

	internal ImmutableArray<PEModule> Modules => _modules;

	internal AssemblyIdentity Identity => _identity;

	internal bool DeclaresTheObjectClass
	{
		get
		{
			if (_lazyDeclaresTheObjectClass == ThreeState.Unknown)
			{
				bool value = _modules[0].MetadataReader.DeclaresTheObjectClass();
				_lazyDeclaresTheObjectClass = value.ToThreeState();
			}
			return _lazyDeclaresTheObjectClass == ThreeState.True;
		}
	}

	internal PEAssembly(AssemblyMetadata owner, ImmutableArray<PEModule> modules)
	{
		_identity = modules[0].ReadAssemblyIdentityOrThrow();
		ArrayBuilder<AssemblyIdentity> instance = ArrayBuilder<AssemblyIdentity>.GetInstance();
		int[] array = new int[modules.Length];
		for (int i = 0; i < modules.Length; i++)
		{
			ImmutableArray<AssemblyIdentity> referencedAssemblies = modules[i].ReferencedAssemblies;
			array[i] = referencedAssemblies.Length;
			instance.AddRange(referencedAssemblies);
		}
		_modules = modules;
		AssemblyReferences = instance.ToImmutableAndFree();
		ModuleReferenceCounts = array.AsImmutableOrNull();
		_owner = owner;
	}

	internal bool ContainsNoPiaLocalTypes()
	{
		if (_lazyContainsNoPiaLocalTypes == ThreeState.Unknown)
		{
			foreach (PEModule module in Modules)
			{
				if (module.ContainsNoPiaLocalTypes())
				{
					_lazyContainsNoPiaLocalTypes = ThreeState.True;
					return true;
				}
			}
			_lazyContainsNoPiaLocalTypes = ThreeState.False;
		}
		return _lazyContainsNoPiaLocalTypes == ThreeState.True;
	}

	private Dictionary<string, List<ImmutableArray<byte>>> BuildInternalsVisibleToMap()
	{
		Dictionary<string, List<ImmutableArray<byte>>> dictionary = new Dictionary<string, List<ImmutableArray<byte>>>(StringComparer.OrdinalIgnoreCase);
		foreach (string internalsVisibleToAttributeValue in Modules[0].GetInternalsVisibleToAttributeValues(Handle))
		{
			if (AssemblyIdentity.TryParseDisplayName(internalsVisibleToAttributeValue, out var identity))
			{
				if (dictionary.TryGetValue(identity.Name, out var value))
				{
					value.Add(identity.PublicKey);
					continue;
				}
				value = new List<ImmutableArray<byte>>();
				value.Add(identity.PublicKey);
				dictionary[identity.Name] = value;
			}
		}
		return dictionary;
	}

	internal IEnumerable<ImmutableArray<byte>> GetInternalsVisibleToPublicKeys(string simpleName)
	{
		if (_lazyInternalsVisibleToMap == null)
		{
			Interlocked.CompareExchange(ref _lazyInternalsVisibleToMap, BuildInternalsVisibleToMap(), null);
		}
		_lazyInternalsVisibleToMap.TryGetValue(simpleName, out var value);
		IEnumerable<ImmutableArray<byte>> enumerable = value;
		return enumerable ?? SpecializedCollections.EmptyEnumerable<ImmutableArray<byte>>();
	}

	public AssemblyMetadata GetNonDisposableMetadata()
	{
		return _owner.Copy();
	}
}
