using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Mono.Cecil;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

public abstract class AssemblyLocatorBase : IAssemblyLocator
{
	[DebuggerDisplay("{FullName}")]
	protected class AssemblyNameAdapter
	{
		public string Name { get; set; }

		public string FullName { get; }

		public string CultureName { get; set; }

		public Version Version { get; }

		public ProcessorArchitecture? ProcessorArchitecture { get; }

		public byte[] PublicKeyToken { get; }

		public AssemblyNameAdapter(string name, string cultureName, byte[] publicKeyToken, Version version, ProcessorArchitecture? processorArchitecture)
		{
			string normalizedCultureName = DotNetUtilities.GetNormalizedCultureName(cultureName);
			if (IsNullVersion(version))
			{
				version = null;
			}
			FullName = DotNetUtilities.GetAssemblyName(name, version?.ToString(), normalizedCultureName, DotNetUtilities.GetPublicKeyToken(publicKeyToken));
			Name = name;
			ProcessorArchitecture = processorArchitecture;
			Version = version;
			CultureName = normalizedCultureName;
			PublicKeyToken = publicKeyToken;
		}

		private static bool IsNullVersion(Version version)
		{
			if (version != null && version.Major == 0 && version.Minor == 0 && version.Build == 0)
			{
				return version.MinorRevision == 0;
			}
			return false;
		}
	}

	private static readonly ConcurrentDictionary<string, AssemblyNameAdapter> pathNameToAssemblyNameMap = new ConcurrentDictionary<string, AssemblyNameAdapter>();

	private ImmutableArray<string> pathsToAssembliesInProbingPaths;

	private readonly object lockOnGetPathToAssembly = new object();

	private readonly IReadOnlyDictionary<string, string> wellKnownAssemblies;

	private static readonly byte[][] MicrosoftPublicKeyTokens = new byte[3][]
	{
		new byte[8] { 176, 63, 95, 127, 17, 213, 10, 58 },
		new byte[8] { 49, 191, 56, 86, 173, 54, 78, 53 },
		new byte[8] { 183, 122, 92, 86, 25, 52, 224, 137 }
	};

	public IEnumerable<string> ProbingPaths { get; }

	protected AssemblyLocatorBase(IEnumerable<string> probingPaths, IReadOnlyDictionary<string, string> wellKnownAssemblies = null)
	{
		ProbingPaths = probingPaths.ToArray();
		this.wellKnownAssemblies = wellKnownAssemblies ?? new Dictionary<string, string>();
	}

	public string GetPathToAssembly(string fullyQualifiedAssemblyName)
	{
		AssemblyNameAdapter assemblyName = GetAssemblyName(fullyQualifiedAssemblyName);
		if (assemblyName == null)
		{
			return null;
		}
		lock (lockOnGetPathToAssembly)
		{
			return GetPathToCompatibleAssembly(assemblyName);
		}
	}

	private string GetPathToCompatibleAssembly(AssemblyNameAdapter searchName)
	{
		IEnumerable<string> pathsToAssembliesInProbingPath = GetPathsToAssembliesInProbingPath(searchName, invalidateCache: false);
		string pathToCompatibleAssemblyFromCandidates = GetPathToCompatibleAssemblyFromCandidates(searchName, pathsToAssembliesInProbingPath);
		if (pathToCompatibleAssemblyFromCandidates != null)
		{
			return pathToCompatibleAssemblyFromCandidates;
		}
		if (!TryRetrieveAssembly(searchName))
		{
			return null;
		}
		pathsToAssembliesInProbingPath = GetPathsToAssembliesInProbingPath(searchName, invalidateCache: true);
		return GetPathToCompatibleAssemblyFromCandidates(searchName, pathsToAssembliesInProbingPath);
	}

	protected abstract bool TryRetrieveAssembly(AssemblyNameAdapter searchName);

	private static string GetPathToCompatibleAssemblyFromCandidates(AssemblyNameAdapter searchName, IEnumerable<string> pathsToAssemblies)
	{
		GetBuildAndRevisionNumbersForAssemblyVersion(searchName, out var buildNumber, out var revisionNumber);
		string result = null;
		foreach (string pathsToAssembly in pathsToAssemblies)
		{
			AssemblyNameAdapter orAdd = pathNameToAssemblyNameMap.GetOrAdd(pathsToAssembly, GetAssemblyNameFromPath);
			if (orAdd != null && IsAssemblyCompatible(searchName, orAdd))
			{
				if (IsExactMatch(searchName, orAdd))
				{
					return pathsToAssembly;
				}
				if (IsAssemblyBuildAndRevisionCompatible(orAdd, ref buildNumber, ref revisionNumber))
				{
					result = pathsToAssembly;
				}
			}
		}
		return result;
	}

	private static void GetBuildAndRevisionNumbersForAssemblyVersion(AssemblyNameAdapter searchName, out int buildNumber, out int revisionNumber)
	{
		buildNumber = 0;
		revisionNumber = 0;
		if (searchName.Version != null)
		{
			buildNumber = searchName.Version.Build;
			revisionNumber = searchName.Version.Revision;
		}
	}

	private AssemblyNameAdapter GetAssemblyName(string searchAssemblyName)
	{
		if (string.IsNullOrEmpty(searchAssemblyName))
		{
			return null;
		}
		try
		{
			return CreateAdapter(AssemblyNameReference.Parse(searchAssemblyName), ProcessorArchitecture.None);
		}
		catch (ArgumentException)
		{
		}
		catch (FormatException)
		{
		}
		return null;
	}

	private static bool IsAssemblyCompatible(AssemblyNameAdapter searchName, AssemblyNameAdapter assemblyBeingEvaluated)
	{
		if (assemblyBeingEvaluated.FullName.Equals(searchName.FullName))
		{
			return true;
		}
		if (!searchName.Name.Equals(assemblyBeingEvaluated.Name))
		{
			return false;
		}
		byte[] publicKeyToken = searchName.PublicKeyToken;
		if (publicKeyToken != null && publicKeyToken.Length != 0 && !IsPublicKeyTokenCompatible(publicKeyToken, assemblyBeingEvaluated.PublicKeyToken))
		{
			return false;
		}
		if (!string.Equals(searchName.CultureName, assemblyBeingEvaluated.CultureName, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (IsUnifiedAssembly(searchName))
		{
			return true;
		}
		if (!IsVersionCompatible(searchName.Version, assemblyBeingEvaluated.Version, specificVersion: false))
		{
			return false;
		}
		return true;
	}

	private static bool IsUnifiedAssembly(AssemblyNameAdapter searchName)
	{
		if (searchName.Name == "mscorlib" || searchName.Name.StartsWith("System", StringComparison.Ordinal))
		{
			for (int i = 0; i < MicrosoftPublicKeyTokens.Length; i++)
			{
				if (IsPublicKeyTokenCompatible(searchName.PublicKeyToken, MicrosoftPublicKeyTokens[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsProcessorArchitectureCompatible(ProcessorArchitecture? assemblyArchitecture)
	{
		if (assemblyArchitecture == ProcessorArchitecture.None || assemblyArchitecture == ProcessorArchitecture.MSIL)
		{
			return true;
		}
		if (Environment.Is64BitProcess)
		{
			if (assemblyArchitecture == ProcessorArchitecture.Amd64 || assemblyArchitecture == ProcessorArchitecture.IA64)
			{
				return true;
			}
		}
		else if (assemblyArchitecture == ProcessorArchitecture.X86)
		{
			return true;
		}
		return false;
	}

	private static bool IsPublicKeyTokenCompatible(byte[] searchPublicKeyToken, byte[] assemblyPublicKeyToken)
	{
		if (searchPublicKeyToken == null || assemblyPublicKeyToken == null)
		{
			return searchPublicKeyToken == assemblyPublicKeyToken;
		}
		if (searchPublicKeyToken.Length != assemblyPublicKeyToken.Length)
		{
			return false;
		}
		for (int i = 0; i < searchPublicKeyToken.Length; i++)
		{
			if (searchPublicKeyToken[i] != assemblyPublicKeyToken[i])
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsVersionCompatible(Version searchVersion, Version assemblyVersion, bool specificVersion)
	{
		if (searchVersion != null)
		{
			if (assemblyVersion == null)
			{
				return false;
			}
			if (!searchVersion.Major.Equals(assemblyVersion.Major) || !searchVersion.Minor.Equals(assemblyVersion.Minor))
			{
				return false;
			}
			if (specificVersion && (!searchVersion.Build.Equals(assemblyVersion.Build) || !searchVersion.Revision.Equals(assemblyVersion.Revision)))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsAssemblyBuildAndRevisionCompatible(AssemblyNameAdapter assemblyNameBeingEvaluated, ref int compatibleBuildNumber, ref int compatibleRevisionNumber)
	{
		if (!IsProcessorArchitectureCompatible(assemblyNameBeingEvaluated.ProcessorArchitecture))
		{
			return false;
		}
		GetBuildAndRevisionNumbersForAssemblyVersion(assemblyNameBeingEvaluated, out var buildNumber, out var revisionNumber);
		if (buildNumber == compatibleBuildNumber)
		{
			if (revisionNumber >= compatibleRevisionNumber)
			{
				compatibleBuildNumber = buildNumber;
				compatibleRevisionNumber = revisionNumber;
				return true;
			}
		}
		else if (buildNumber > compatibleBuildNumber)
		{
			compatibleBuildNumber = buildNumber;
			compatibleRevisionNumber = revisionNumber;
			return true;
		}
		return false;
	}

	private static bool IsExactMatch(AssemblyNameAdapter searchName, AssemblyNameAdapter assemblyBeingEvaluated)
	{
		if (IsVersionCompatible(searchName.Version, assemblyBeingEvaluated.Version, specificVersion: true) && IsProcessorArchitectureCompatible(assemblyBeingEvaluated.ProcessorArchitecture))
		{
			return true;
		}
		return false;
	}

	private IEnumerable<string> GetPathsToAssembliesInProbingPath(AssemblyNameAdapter searchName, bool invalidateCache)
	{
		try
		{
			if (wellKnownAssemblies.TryGetValue(searchName.Name, out var value))
			{
				return SpecializedCollections.SingletonEnumerable(value);
			}
			return GetAssembliesFromProbingPathLazy(invalidateCache);
		}
		catch (DirectoryNotFoundException)
		{
		}
		return SpecializedCollections.EmptyEnumerable<string>();
	}

	private IEnumerable<string> GetAssembliesFromProbingPathLazy(bool invalidateCache)
	{
		if (!invalidateCache && pathsToAssembliesInProbingPaths != null)
		{
			return pathsToAssembliesInProbingPaths;
		}
		pathsToAssembliesInProbingPaths = GetAllAssembliesFromProbingPaths().ToImmutableArrayOrEmpty();
		return pathsToAssembliesInProbingPaths;
	}

	private IEnumerable<string> GetAllAssembliesFromProbingPaths()
	{
		return ProbingPaths.Where(Directory.Exists).SelectMany((string searchFolder) => Directory.GetFiles(searchFolder, "*.dll", SearchOption.AllDirectories));
	}

	private static AssemblyNameAdapter GetAssemblyNameFromPath(string filePath)
	{
		try
		{
			using ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(filePath);
			if (moduleDefinition.Assembly == null)
			{
				return null;
			}
			return CreateAdapter(moduleDefinition.Assembly.Name, moduleDefinition.GetProcessorArchitecture());
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
		catch (BadImageFormatException)
		{
		}
		catch (SecurityException)
		{
		}
		return null;
	}

	private static AssemblyNameAdapter CreateAdapter(AssemblyNameReference assemblyName, ProcessorArchitecture? processorArchitecture = ProcessorArchitecture.None)
	{
		if (assemblyName == null)
		{
			return null;
		}
		return new AssemblyNameAdapter(assemblyName.Name, assemblyName.Culture, assemblyName.PublicKeyToken, assemblyName.Version, processorArchitecture);
	}
}
