using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public abstract class MetadataReference
{
	public MetadataReferenceProperties Properties { get; }

	public virtual string Display => null;

	internal virtual bool IsUnresolved => false;

	protected MetadataReference(MetadataReferenceProperties properties)
	{
		Properties = properties;
	}

	public MetadataReference WithAliases(IEnumerable<string> aliases)
	{
		return WithAliases(ImmutableArray.CreateRange(aliases));
	}

	public MetadataReference WithEmbedInteropTypes(bool value)
	{
		return WithProperties(Properties.WithEmbedInteropTypes(value));
	}

	public MetadataReference WithAliases(ImmutableArray<string> aliases)
	{
		return WithProperties(Properties.WithAliases(aliases));
	}

	public MetadataReference WithProperties(MetadataReferenceProperties properties)
	{
		if (properties == Properties)
		{
			return this;
		}
		return WithPropertiesImplReturningMetadataReference(properties);
	}

	internal abstract MetadataReference WithPropertiesImplReturningMetadataReference(MetadataReferenceProperties properties);

	public static PortableExecutableReference CreateFromImage(ImmutableArray<byte> peImage, MetadataReferenceProperties properties = default(MetadataReferenceProperties), DocumentationProvider documentation = null, string filePath = null)
	{
		return new MetadataImageReference(AssemblyMetadata.CreateFromImage(peImage), properties, documentation, filePath, null);
	}

	public static PortableExecutableReference CreateFromImage(IEnumerable<byte> peImage, MetadataReferenceProperties properties = default(MetadataReferenceProperties), DocumentationProvider documentation = null, string filePath = null)
	{
		return new MetadataImageReference(AssemblyMetadata.CreateFromImage(peImage), properties, documentation, filePath, null);
	}

	public static PortableExecutableReference CreateFromStream(Stream peStream, MetadataReferenceProperties properties = default(MetadataReferenceProperties), DocumentationProvider documentation = null, string filePath = null)
	{
		return new MetadataImageReference(AssemblyMetadata.CreateFromStream(peStream, PEStreamOptions.PrefetchEntireImage), properties, documentation, filePath, null);
	}

	public static PortableExecutableReference CreateFromFile(string path, MetadataReferenceProperties properties = default(MetadataReferenceProperties), DocumentationProvider documentation = null)
	{
		ModuleMetadata moduleMetadata = ModuleMetadata.CreateFromStream(FileUtilities.OpenFileStream(path), PEStreamOptions.PrefetchEntireImage);
		if (properties.Kind == MetadataImageKind.Module)
		{
			return new MetadataImageReference(moduleMetadata, properties, documentation, path, null);
		}
		return new MetadataImageReference(AssemblyMetadata.CreateFromFile(moduleMetadata, path), properties, documentation, path, null);
	}

	[Obsolete("Use CreateFromFile(assembly.Location) instead", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static MetadataReference CreateFromAssembly(Assembly assembly)
	{
		return CreateFromAssemblyInternal(assembly);
	}

	internal static MetadataReference CreateFromAssemblyInternal(Assembly assembly)
	{
		return CreateFromAssemblyInternal(assembly, default);
	}

	[Obsolete("Use CreateFromFile(assembly.Location) instead", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static MetadataReference CreateFromAssembly(Assembly assembly, MetadataReferenceProperties properties, DocumentationProvider documentation = null)
	{
		return CreateFromAssemblyInternal(assembly, properties, documentation);
	}

	internal static PortableExecutableReference CreateFromAssemblyInternal(Assembly assembly, MetadataReferenceProperties properties, DocumentationProvider documentation = null)
	{
		if ((object)assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		if (assembly.IsDynamic)
		{
			throw new NotSupportedException(CodeAnalysisResources.CantCreateReferenceToDynamicAssembly);
		}
		if (properties.Kind != MetadataImageKind.Assembly)
		{
			throw new ArgumentException(CodeAnalysisResources.CantCreateModuleReferenceToAssembly, "properties");
		}
		string assemblyLocation = CorLightup.Desktop.GetAssemblyLocation(assembly);
		if (string.IsNullOrEmpty(assemblyLocation))
		{
			throw new NotSupportedException(CodeAnalysisResources.CantCreateReferenceToAssemblyWithoutLocation);
		}
		return new MetadataImageReference(AssemblyMetadata.CreateFromStream(FileUtilities.OpenFileStream(assemblyLocation)), properties, documentation, assemblyLocation, null);
	}

	internal static bool HasMetadata(Assembly assembly)
	{
		if (!assembly.IsDynamic)
		{
			return !string.IsNullOrEmpty(CorLightup.Desktop.GetAssemblyLocation(assembly));
		}
		return false;
	}
}
