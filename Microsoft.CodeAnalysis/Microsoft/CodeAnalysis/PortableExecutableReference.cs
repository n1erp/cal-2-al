using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;

namespace Microsoft.CodeAnalysis;

public abstract class PortableExecutableReference : MetadataReference
{
	private readonly string _filePath;

	private DocumentationProvider _lazyDocumentation;

	public override string Display => FilePath;

	public string FilePath => _filePath;

	internal DocumentationProvider DocumentationProvider
	{
		get
		{
			if (_lazyDocumentation == null)
			{
				Interlocked.CompareExchange(ref _lazyDocumentation, CreateDocumentationProvider(), null);
			}
			return _lazyDocumentation;
		}
	}

	protected PortableExecutableReference(MetadataReferenceProperties properties, string fullPath = null, DocumentationProvider initialDocumentation = null)
		: base(properties)
	{
		_filePath = fullPath;
		_lazyDocumentation = initialDocumentation;
	}

	protected abstract DocumentationProvider CreateDocumentationProvider();

	public new PortableExecutableReference WithAliases(IEnumerable<string> aliases)
	{
		return WithAliases(ImmutableArray.CreateRange(aliases));
	}

	public new PortableExecutableReference WithAliases(ImmutableArray<string> aliases)
	{
		return WithProperties(Properties.WithAliases(aliases));
	}

	public new PortableExecutableReference WithEmbedInteropTypes(bool value)
	{
		return WithProperties(Properties.WithEmbedInteropTypes(value));
	}

	public new PortableExecutableReference WithProperties(MetadataReferenceProperties properties)
	{
		if (properties == Properties)
		{
			return this;
		}
		return WithPropertiesImpl(properties);
	}

	internal sealed override MetadataReference WithPropertiesImplReturningMetadataReference(MetadataReferenceProperties properties)
	{
		return WithPropertiesImpl(properties);
	}

	protected abstract PortableExecutableReference WithPropertiesImpl(MetadataReferenceProperties properties);

	protected abstract Metadata GetMetadataImpl();

	internal Metadata GetMetadataNoCopy()
	{
		return GetMetadataImpl();
	}

	public Metadata GetMetadata()
	{
		return GetMetadataNoCopy().Copy();
	}

	public MetadataId GetMetadataId()
	{
		return GetMetadataNoCopy().Id;
	}

	internal static Diagnostic ExceptionToDiagnostic(Exception e, CommonMessageProvider messageProvider, Location location, string display, MetadataImageKind kind)
	{
		if (e is BadImageFormatException)
		{
			int code = ((kind == MetadataImageKind.Assembly) ? messageProvider.ERR_InvalidAssemblyMetadata : messageProvider.ERR_InvalidModuleMetadata);
			return messageProvider.CreateDiagnostic(code, location, display, e.Message);
		}
		if (e is FileNotFoundException ex)
		{
			return messageProvider.CreateDiagnostic(messageProvider.ERR_MetadataFileNotFound, location, ex.FileName ?? string.Empty);
		}
		int code2 = ((kind == MetadataImageKind.Assembly) ? messageProvider.ERR_ErrorOpeningAssemblyFile : messageProvider.ERR_ErrorOpeningModuleFile);
		return messageProvider.CreateDiagnostic(code2, location, display, e.Message);
	}
}
