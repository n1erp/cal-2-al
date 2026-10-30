using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public sealed class ModuleMetadata : Metadata
{
	private bool _isDisposed;

	private readonly PEModule _module;

	public bool IsDisposed
	{
		get
		{
			if (!_isDisposed)
			{
				return _module.IsDisposed;
			}
			return true;
		}
	}

	internal PEModule Module
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("ModuleMetadata");
			}
			return _module;
		}
	}

	public string Name => Module.Name;

	public override MetadataImageKind Kind => MetadataImageKind.Module;

	internal MetadataReader MetadataReader => Module.MetadataReader;

	private ModuleMetadata(PEReader peReader)
		: base(isImageOwner: true, MetadataId.CreateNewId())
	{
		_module = new PEModule(this, peReader, IntPtr.Zero, 0, includeEmbeddedInteropTypes: false, ignoreAssemblyRefs: false);
	}

	private ModuleMetadata(IntPtr metadata, int size, bool includeEmbeddedInteropTypes, bool ignoreAssemblyRefs)
		: base(isImageOwner: true, MetadataId.CreateNewId())
	{
		_module = new PEModule(this, null, metadata, size, includeEmbeddedInteropTypes, ignoreAssemblyRefs);
	}

	private ModuleMetadata(ModuleMetadata metadata)
		: base(isImageOwner: false, metadata.Id)
	{
		_module = metadata.Module;
	}

	public static ModuleMetadata CreateFromMetadata(IntPtr metadata, int size)
	{
		if (metadata == IntPtr.Zero)
		{
			throw new ArgumentNullException("metadata");
		}
		if (size <= 0)
		{
			throw new ArgumentOutOfRangeException(CodeAnalysisResources.SizeHasToBePositive, "size");
		}
		return new ModuleMetadata(metadata, size, includeEmbeddedInteropTypes: false, ignoreAssemblyRefs: false);
	}

	internal static ModuleMetadata CreateFromMetadata(IntPtr metadata, int size, bool includeEmbeddedInteropTypes, bool ignoreAssemblyRefs = false)
	{
		return new ModuleMetadata(metadata, size, includeEmbeddedInteropTypes, ignoreAssemblyRefs);
	}

	public unsafe static ModuleMetadata CreateFromImage(IntPtr peImage, int size)
	{
		if (peImage == IntPtr.Zero)
		{
			throw new ArgumentNullException("peImage");
		}
		if (size <= 0)
		{
			throw new ArgumentOutOfRangeException(CodeAnalysisResources.SizeHasToBePositive, "size");
		}
		return new ModuleMetadata(new PEReader((byte*)(void*)peImage, size));
	}

	public static ModuleMetadata CreateFromImage(IEnumerable<byte> peImage)
	{
		if (peImage == null)
		{
			throw new ArgumentNullException("peImage");
		}
		return CreateFromImage(ImmutableArray.CreateRange(peImage));
	}

	public static ModuleMetadata CreateFromImage(ImmutableArray<byte> peImage)
	{
		if (peImage.IsDefault)
		{
			throw new ArgumentNullException("peImage");
		}
		return new ModuleMetadata(new PEReader(peImage));
	}

	public static ModuleMetadata CreateFromStream(Stream peStream, bool leaveOpen = false)
	{
		return CreateFromStream(peStream, leaveOpen ? PEStreamOptions.LeaveOpen : PEStreamOptions.Default);
	}

	public static ModuleMetadata CreateFromStream(Stream peStream, PEStreamOptions options)
	{
		if (peStream == null)
		{
			throw new ArgumentNullException("peStream");
		}
		if (!peStream.CanRead || !peStream.CanSeek)
		{
			throw new ArgumentException(CodeAnalysisResources.StreamMustSupportReadAndSeek, "peStream");
		}
		if (peStream.Length == 0L && (options & PEStreamOptions.PrefetchEntireImage) != 0 && (options & PEStreamOptions.PrefetchMetadata) != 0)
		{
			new PEHeaders(peStream);
		}
		return new ModuleMetadata(new PEReader(peStream, options));
	}

	public static ModuleMetadata CreateFromFile(string path)
	{
		return CreateFromStream(FileUtilities.OpenFileStream(path));
	}

	internal new ModuleMetadata Copy()
	{
		return new ModuleMetadata(this);
	}

	protected override Metadata CommonCopy()
	{
		return Copy();
	}

	public override void Dispose()
	{
		_isDisposed = true;
		if (IsImageOwner)
		{
			_module.Dispose();
		}
	}

	public Guid GetModuleVersionId()
	{
		return Module.GetModuleVersionIdOrThrow();
	}

	public ImmutableArray<string> GetModuleNames()
	{
		return Module.GetMetadataModuleNamesOrThrow();
	}

	public MetadataReader GetMetadataReader()
	{
		return MetadataReader;
	}

	public PortableExecutableReference GetReference(DocumentationProvider documentation = null, string filePath = null, string display = null)
	{
		return new MetadataImageReference(this, MetadataReferenceProperties.Module, documentation, filePath, display);
	}
}
