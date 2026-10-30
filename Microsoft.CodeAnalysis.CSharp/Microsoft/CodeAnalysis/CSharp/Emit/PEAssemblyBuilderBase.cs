using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal abstract class PEAssemblyBuilderBase : PEModuleBuilder, IAssemblyReference, IModuleReference, IUnitReference, IReference, INamedEntity
{
	private readonly SourceAssemblySymbol _sourceAssembly;

	private readonly ImmutableArray<NamedTypeSymbol> _additionalTypes;

	private ImmutableArray<IFileReference> _lazyFiles;

	private SynthesizedEmbeddedAttributeSymbol _lazyEmbeddedAttribute;

	private SynthesizedEmbeddedAttributeSymbol _lazyIsReadOnlyAttribute;

	private SynthesizedEmbeddedAttributeSymbol _lazyIsByRefLikeAttribute;

	private SynthesizedEmbeddedAttributeSymbol _lazyIsUnmanagedAttribute;

	private readonly string _metadataName;

	public override ISourceAssemblySymbolInternal SourceAssemblyOpt => _sourceAssembly;

	public override string Name => _metadataName;

	public AssemblyIdentity Identity => _sourceAssembly.Identity;

	public Version AssemblyVersionPattern => _sourceAssembly.AssemblyVersionPattern;

	public PEAssemblyBuilderBase(SourceAssemblySymbol sourceAssembly, EmitOptions emitOptions, OutputKind outputKind, ModulePropertiesForSerialization serializationProperties, IEnumerable<ResourceDescription> manifestResources, ImmutableArray<NamedTypeSymbol> additionalTypes)
		: base((SourceModuleSymbol)sourceAssembly.Modules[0], emitOptions, outputKind, serializationProperties, manifestResources)
	{
		_sourceAssembly = sourceAssembly;
		_additionalTypes = additionalTypes.NullToEmpty();
		_metadataName = ((emitOptions.OutputNameOverride == null) ? sourceAssembly.MetadataName : FileNameUtilities.ChangeExtension(emitOptions.OutputNameOverride, null));
		AssemblyOrModuleSymbolToModuleRefMap.Add(sourceAssembly, this);
	}

	internal override ImmutableArray<NamedTypeSymbol> GetAdditionalTopLevelTypes(DiagnosticBag diagnostics)
	{
		return _additionalTypes;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetEmbeddedTypes(DiagnosticBag diagnostics)
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		CreateEmbeddedAttributesIfNeeded(diagnostics);
		if ((object)_lazyEmbeddedAttribute != null)
		{
			instance.Add(_lazyEmbeddedAttribute);
		}
		if ((object)_lazyIsReadOnlyAttribute != null)
		{
			instance.Add(_lazyIsReadOnlyAttribute);
		}
		if ((object)_lazyIsUnmanagedAttribute != null)
		{
			instance.Add(_lazyIsUnmanagedAttribute);
		}
		if ((object)_lazyIsByRefLikeAttribute != null)
		{
			instance.Add(_lazyIsByRefLikeAttribute);
		}
		return instance.ToImmutableAndFree();
	}

	public sealed override IEnumerable<IFileReference> GetFiles(EmitContext context)
	{
		if (_lazyFiles.IsDefault)
		{
			ArrayBuilder<IFileReference> instance = ArrayBuilder<IFileReference>.GetInstance();
			try
			{
				ImmutableArray<ModuleSymbol> modules = _sourceAssembly.Modules;
				for (int i = 1; i < modules.Length; i++)
				{
					instance.Add((IFileReference)Translate(modules[i], context.Diagnostics));
				}
				foreach (ResourceDescription manifestResource in ManifestResources)
				{
					if (!manifestResource.IsEmbedded)
					{
						instance.Add(manifestResource);
					}
				}
				if (ImmutableInterlocked.InterlockedInitialize(ref _lazyFiles, instance.ToImmutable()) && _lazyFiles.Length > 0 && !CryptographicHashProvider.IsSupportedAlgorithm(_sourceAssembly.HashAlgorithm))
				{
					context.Diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_CryptoHashFailed), NoLocation.Singleton));
				}
			}
			finally
			{
				instance.Free();
			}
		}
		return _lazyFiles;
	}

	protected override void AddEmbeddedResourcesFromAddedModules(ArrayBuilder<ManagedResource> builder, DiagnosticBag diagnostics)
	{
		ImmutableArray<ModuleSymbol> modules = _sourceAssembly.Modules;
		int length = modules.Length;
		for (int i = 1; i < length; i++)
		{
			IFileReference fileReference = (IFileReference)Translate(modules[i], diagnostics);
			try
			{
				foreach (EmbeddedResource item in ((PEModuleSymbol)modules[i]).Module.GetEmbeddedResourcesOrThrow())
				{
					builder.Add(new ManagedResource(item.Name, (item.Attributes & ManifestResourceAttributes.Public) != 0, null, fileReference, item.Offset));
				}
			}
			catch (BadImageFormatException)
			{
				diagnostics.Add(new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, modules[i]), NoLocation.Singleton);
			}
		}
	}

	internal override SynthesizedAttributeData SynthesizeEmbeddedAttribute()
	{
		return new SynthesizedAttributeData(_lazyEmbeddedAttribute.Constructor, ImmutableArray<TypedConstant>.Empty, ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty);
	}

	protected override SynthesizedAttributeData TrySynthesizeIsReadOnlyAttribute()
	{
		if ((object)_lazyIsReadOnlyAttribute != null)
		{
			return new SynthesizedAttributeData(_lazyIsReadOnlyAttribute.Constructor, ImmutableArray<TypedConstant>.Empty, ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty);
		}
		return base.TrySynthesizeIsReadOnlyAttribute();
	}

	protected override SynthesizedAttributeData TrySynthesizeIsUnmanagedAttribute()
	{
		if ((object)_lazyIsUnmanagedAttribute != null)
		{
			return new SynthesizedAttributeData(_lazyIsUnmanagedAttribute.Constructor, ImmutableArray<TypedConstant>.Empty, ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty);
		}
		return base.TrySynthesizeIsUnmanagedAttribute();
	}

	protected override SynthesizedAttributeData TrySynthesizeIsByRefLikeAttribute()
	{
		if ((object)_lazyIsByRefLikeAttribute != null)
		{
			return new SynthesizedAttributeData(_lazyIsByRefLikeAttribute.Constructor, ImmutableArray<TypedConstant>.Empty, ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty);
		}
		return base.TrySynthesizeIsByRefLikeAttribute();
	}

	private void CreateEmbeddedAttributesIfNeeded(DiagnosticBag diagnostics)
	{
		if (NeedsGeneratedIsReadOnlyAttribute)
		{
			CreateEmbeddedAttributeItselfIfNeeded(diagnostics);
			CreateEmbeddedAttributeIfNeeded(ref _lazyIsReadOnlyAttribute, diagnostics, AttributeDescription.IsReadOnlyAttribute);
		}
		if (NeedsGeneratedIsByRefLikeAttribute)
		{
			CreateEmbeddedAttributeItselfIfNeeded(diagnostics);
			CreateEmbeddedAttributeIfNeeded(ref _lazyIsByRefLikeAttribute, diagnostics, AttributeDescription.IsByRefLikeAttribute);
		}
		if (NeedsGeneratedIsUnmanagedAttribute)
		{
			CreateEmbeddedAttributeItselfIfNeeded(diagnostics);
			CreateEmbeddedAttributeIfNeeded(ref _lazyIsUnmanagedAttribute, diagnostics, AttributeDescription.IsUnmanagedAttribute);
		}
	}

	private void CreateEmbeddedAttributeItselfIfNeeded(DiagnosticBag diagnostics)
	{
		CreateEmbeddedAttributeIfNeeded(ref _lazyEmbeddedAttribute, diagnostics, AttributeDescription.CodeAnalysisEmbeddedAttribute);
	}

	private void CreateEmbeddedAttributeIfNeeded(ref SynthesizedEmbeddedAttributeSymbol symbol, DiagnosticBag diagnostics, AttributeDescription description)
	{
		if ((object)symbol == null)
		{
			MetadataTypeName emittedName = MetadataTypeName.FromFullName(description.FullName);
			NamedTypeSymbol namedTypeSymbol = _sourceAssembly.SourceModule.LookupTopLevelMetadataType(ref emittedName);
			if (!(namedTypeSymbol is MissingMetadataTypeSymbol))
			{
				diagnostics.Add(ErrorCode.ERR_TypeReserved, namedTypeSymbol.Locations[0], description.FullName);
			}
			symbol = new SynthesizedEmbeddedAttributeSymbol(description, _sourceAssembly.DeclaringCompilation, diagnostics);
		}
	}
}
