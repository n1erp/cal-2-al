using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class AnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
{
	private readonly object _guard = new object();

	private readonly Dictionary<string, Assembly> _loadedAssembliesByPath = new Dictionary<string, Assembly>();

	private readonly Dictionary<string, AssemblyIdentity> _loadedAssemblyIdentitiesByPath = new Dictionary<string, AssemblyIdentity>();

	private readonly Dictionary<AssemblyIdentity, Assembly> _loadedAssembliesByIdentity = new Dictionary<AssemblyIdentity, Assembly>();

	private readonly Dictionary<string, List<string>> _knownAssemblyPathsBySimpleName = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

	protected abstract Assembly LoadFromPathImpl(string fullPath);

	public void AddDependencyLocation(string fullPath)
	{
		CompilerPathUtilities.RequireAbsolutePath(fullPath, "fullPath");
		string fileName = PathUtilities.GetFileName(fullPath, includeExtension: false);
		lock (_guard)
		{
			if (!_knownAssemblyPathsBySimpleName.TryGetValue(fileName, out var value))
			{
				_knownAssemblyPathsBySimpleName.Add(fileName, new List<string> { fullPath });
			}
			else if (!value.Contains(fullPath))
			{
				value.Add(fullPath);
			}
		}
	}

	public Assembly LoadFromPath(string fullPath)
	{
		CompilerPathUtilities.RequireAbsolutePath(fullPath, "fullPath");
		return LoadFromPathUnchecked(fullPath);
	}

	private Assembly LoadFromPathUnchecked(string fullPath)
	{
		return LoadFromPathUncheckedCore(fullPath);
	}

	private Assembly LoadFromPathUncheckedCore(string fullPath, AssemblyIdentity identity = null)
	{
		Assembly assembly = null;
		lock (_guard)
		{
			if (_loadedAssembliesByPath.TryGetValue(fullPath, out var value))
			{
				assembly = value;
			}
			else
			{
				identity = identity ?? GetOrAddAssemblyIdentity(fullPath);
				if (identity != null && _loadedAssembliesByIdentity.TryGetValue(identity, out value))
				{
					assembly = value;
				}
			}
		}
		if ((object)assembly == null)
		{
			assembly = LoadFromPathImpl(fullPath);
		}
		return AddToCache(assembly, fullPath, identity);
	}

	private Assembly AddToCache(Assembly assembly, string fullPath, AssemblyIdentity identity)
	{
		identity = AddToCache(fullPath, identity ?? AssemblyIdentity.FromAssemblyDefinition(assembly));
		lock (_guard)
		{
			if (_loadedAssembliesByIdentity.TryGetValue(identity, out var value))
			{
				assembly = value;
			}
			else
			{
				_loadedAssembliesByIdentity.Add(identity, assembly);
			}
			_loadedAssembliesByPath[fullPath] = assembly;
			return assembly;
		}
	}

	private AssemblyIdentity GetOrAddAssemblyIdentity(string fullPath)
	{
		lock (_guard)
		{
			if (_loadedAssemblyIdentitiesByPath.TryGetValue(fullPath, out var value))
			{
				return value;
			}
		}
		AssemblyIdentity identity = AssemblyIdentityUtils.TryGetAssemblyIdentity(fullPath);
		return AddToCache(fullPath, identity);
	}

	private AssemblyIdentity AddToCache(string fullPath, AssemblyIdentity identity)
	{
		lock (_guard)
		{
			if (_loadedAssemblyIdentitiesByPath.TryGetValue(fullPath, out var value) && value != null)
			{
				identity = value;
			}
			else
			{
				_loadedAssemblyIdentitiesByPath[fullPath] = identity;
			}
		}
		return identity;
	}

	public Assembly Load(string displayName)
	{
		if (!AssemblyIdentity.TryParseDisplayName(displayName, out var identity))
		{
			return null;
		}
		ImmutableArray<string> immutableArray;
		lock (_guard)
		{
			if (_loadedAssembliesByIdentity.TryGetValue(identity, out var value))
			{
				return value;
			}
			if (!_knownAssemblyPathsBySimpleName.TryGetValue(identity.Name, out var value2))
			{
				return null;
			}
			immutableArray = value2.ToImmutableArray();
		}
		foreach (string item in immutableArray)
		{
			AssemblyIdentity orAddAssemblyIdentity = GetOrAddAssemblyIdentity(item);
			if (identity.Equals(orAddAssemblyIdentity))
			{
				return LoadFromPathUncheckedCore(item, orAddAssemblyIdentity);
			}
		}
		return null;
	}
}
