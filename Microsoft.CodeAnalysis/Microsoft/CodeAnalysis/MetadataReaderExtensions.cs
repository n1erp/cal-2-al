using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal static class MetadataReaderExtensions
{
	internal static bool GetWinMdVersion(this MetadataReader reader, out int majorVersion, out int minorVersion)
	{
		if (reader.MetadataKind == MetadataKind.WindowsMetadata)
		{
			string metadataVersion = reader.MetadataVersion;
			if (metadataVersion.StartsWith("WindowsRuntime ", StringComparison.Ordinal))
			{
				string[] array = metadataVersion.Substring("WindowsRuntime ".Length).Split(new char[1] { '.' });
				if (array.Length == 2 && int.TryParse(array[0], NumberStyles.None, CultureInfo.InvariantCulture, out majorVersion) && int.TryParse(array[1], NumberStyles.None, CultureInfo.InvariantCulture, out minorVersion))
				{
					return true;
				}
			}
		}
		majorVersion = 0;
		minorVersion = 0;
		return false;
	}

	internal static AssemblyIdentity ReadAssemblyIdentityOrThrow(this MetadataReader reader)
	{
		if (!reader.IsAssembly)
		{
			return null;
		}
		AssemblyDefinition assemblyDefinition = reader.GetAssemblyDefinition();
		return reader.CreateAssemblyIdentityOrThrow(assemblyDefinition.Version, assemblyDefinition.Flags, assemblyDefinition.PublicKey, assemblyDefinition.Name, assemblyDefinition.Culture, isReference: false);
	}

	internal static ImmutableArray<AssemblyIdentity> GetReferencedAssembliesOrThrow(this MetadataReader reader)
	{
		ArrayBuilder<AssemblyIdentity> instance = ArrayBuilder<AssemblyIdentity>.GetInstance(reader.AssemblyReferences.Count);
		try
		{
			foreach (AssemblyReferenceHandle assemblyReference2 in reader.AssemblyReferences)
			{
				AssemblyReference assemblyReference = reader.GetAssemblyReference(assemblyReference2);
				instance.Add(reader.CreateAssemblyIdentityOrThrow(assemblyReference.Version, assemblyReference.Flags, assemblyReference.PublicKeyOrToken, assemblyReference.Name, assemblyReference.Culture, isReference: true));
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static Guid GetModuleVersionIdOrThrow(this MetadataReader reader)
	{
		return reader.GetGuid(reader.GetModuleDefinition().Mvid);
	}

	private static AssemblyIdentity CreateAssemblyIdentityOrThrow(this MetadataReader reader, Version version, AssemblyFlags flags, BlobHandle publicKey, StringHandle name, StringHandle culture, bool isReference)
	{
		string text = reader.GetString(name);
		if (!MetadataHelpers.IsValidMetadataIdentifier(text))
		{
			throw new BadImageFormatException(string.Format(CodeAnalysisResources.InvalidAssemblyName, text));
		}
		string text2 = (culture.IsNil ? null : reader.GetString(culture));
		if (text2 != null && !MetadataHelpers.IsValidMetadataIdentifier(text2))
		{
			throw new BadImageFormatException(string.Format(CodeAnalysisResources.InvalidCultureName, text2));
		}
		ImmutableArray<byte> immutableArray = reader.GetBlobContent(publicKey);
		bool flag;
		if (isReference)
		{
			flag = (flags & AssemblyFlags.PublicKey) != 0;
			if (flag)
			{
				if (!MetadataHelpers.IsValidPublicKey(immutableArray))
				{
					throw new BadImageFormatException(CodeAnalysisResources.InvalidPublicKey);
				}
			}
			else if (!immutableArray.IsEmpty && immutableArray.Length != 8)
			{
				throw new BadImageFormatException(CodeAnalysisResources.InvalidPublicKeyToken);
			}
		}
		else
		{
			flag = !immutableArray.IsEmpty;
			if (flag && !MetadataHelpers.IsValidPublicKey(immutableArray))
			{
				throw new BadImageFormatException(CodeAnalysisResources.InvalidPublicKey);
			}
		}
		if (immutableArray.IsEmpty)
		{
			immutableArray = default;
		}
		return new AssemblyIdentity(noThrow: true, text, version, text2, immutableArray, flag, (flags & AssemblyFlags.Retargetable) != 0, (AssemblyContentType)((int)(flags & AssemblyFlags.ContentTypeMask) >> 9));
	}

	internal static bool DeclaresTheObjectClass(this MetadataReader reader)
	{
		return reader.DeclaresType(IsTheObjectClass);
	}

	private static bool IsTheObjectClass(this MetadataReader reader, TypeDefinition typeDef)
	{
		if (typeDef.BaseType.IsNil)
		{
			return reader.IsPublicNonInterfaceType(typeDef, "System", "Object");
		}
		return false;
	}

	internal static bool DeclaresType(this MetadataReader reader, Func<MetadataReader, TypeDefinition, bool> predicate)
	{
		foreach (TypeDefinitionHandle typeDefinition2 in reader.TypeDefinitions)
		{
			try
			{
				TypeDefinition typeDefinition = reader.GetTypeDefinition(typeDefinition2);
				if (predicate(reader, typeDefinition))
				{
					return true;
				}
			}
			catch (BadImageFormatException)
			{
			}
		}
		return false;
	}

	internal static bool IsPublicNonInterfaceType(this MetadataReader reader, TypeDefinition typeDef, string namespaceName, string typeName)
	{
		if ((typeDef.Attributes & (TypeAttributes.Public | TypeAttributes.ClassSemanticsMask)) == TypeAttributes.Public && reader.StringComparer.Equals(typeDef.Name, typeName))
		{
			return reader.StringComparer.Equals(typeDef.Namespace, namespaceName);
		}
		return false;
	}
}
