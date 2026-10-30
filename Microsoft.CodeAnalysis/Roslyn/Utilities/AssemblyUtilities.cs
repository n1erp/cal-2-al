using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis;

namespace Roslyn.Utilities;

internal static class AssemblyUtilities
{
	public static ImmutableArray<string> FindAssemblySet(string filePath)
	{
		Queue<string> queue = new Queue<string>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		queue.Enqueue(filePath);
		while (queue.Count > 0)
		{
			string text = queue.Dequeue();
			if (!hashSet.Add(text))
			{
				continue;
			}
			string directoryName = Path.GetDirectoryName(text);
			using PEReader peReader = new PEReader(FileUtilities.OpenRead(text));
			MetadataReader metadataReader = peReader.GetMetadataReader();
			foreach (AssemblyReferenceHandle assemblyReference in metadataReader.AssemblyReferences)
			{
				string text2 = metadataReader.GetString(metadataReader.GetAssemblyReference(assemblyReference).Name);
				string text3 = Path.Combine(directoryName, text2 + ".dll");
				if (!hashSet.Contains(text3) && File.Exists(text3))
				{
					queue.Enqueue(text3);
				}
			}
		}
		return ImmutableArray.CreateRange(hashSet);
	}

	public static Guid ReadMvid(string filePath)
	{
		using PEReader peReader = new PEReader(FileUtilities.OpenRead(filePath));
		MetadataReader metadataReader = peReader.GetMetadataReader();
		GuidHandle mvid = metadataReader.GetModuleDefinition().Mvid;
		return metadataReader.GetGuid(mvid);
	}

	public static ImmutableArray<string> FindSatelliteAssemblies(string filePath)
	{
		ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>();
		string? directoryName = Path.GetDirectoryName(filePath);
		string text = Path.GetFileNameWithoutExtension(filePath) + ".resources";
		string text2 = text + ".dll";
		foreach (string item in Directory.EnumerateDirectories(directoryName, "*", SearchOption.TopDirectoryOnly))
		{
			string text3 = Path.Combine(item, text2);
			if (File.Exists(text3))
			{
				builder.Add(text3);
			}
			text3 = Path.Combine(item, text, text2);
			if (File.Exists(text3))
			{
				builder.Add(text3);
			}
		}
		return builder.ToImmutable();
	}

	public static ImmutableArray<AssemblyIdentity> IdentifyMissingDependencies(string assemblyPath, IEnumerable<string> dependencyFilePaths)
	{
		HashSet<AssemblyIdentity> hashSet = new HashSet<AssemblyIdentity>();
		foreach (string dependencyFilePath in dependencyFilePaths)
		{
			using PEReader peReader = new PEReader(FileUtilities.OpenRead(dependencyFilePath));
			AssemblyIdentity item = peReader.GetMetadataReader().ReadAssemblyIdentityOrThrow();
			hashSet.Add(item);
		}
		HashSet<AssemblyIdentity> hashSet2 = new HashSet<AssemblyIdentity>();
		using (PEReader peReader2 = new PEReader(FileUtilities.OpenRead(assemblyPath)))
		{
			ImmutableArray<AssemblyIdentity> referencedAssembliesOrThrow = peReader2.GetMetadataReader().GetReferencedAssembliesOrThrow();
			hashSet2.AddAll(referencedAssembliesOrThrow);
		}
		hashSet2.ExceptWith(hashSet);
		return ImmutableArray.CreateRange(hashSet2);
	}

	public static AssemblyIdentity GetAssemblyIdentity(string assemblyPath)
	{
		using PEReader peReader = new PEReader(FileUtilities.OpenRead(assemblyPath));
		return peReader.GetMetadataReader().ReadAssemblyIdentityOrThrow();
	}
}
