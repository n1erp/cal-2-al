using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingAssemblySymbol : NonMissingAssemblySymbol
{
	private readonly SourceAssemblySymbol _underlyingAssembly;

	private readonly ImmutableArray<ModuleSymbol> _modules;

	private ImmutableArray<AssemblySymbol> _noPiaResolutionAssemblies;

	private ImmutableArray<AssemblySymbol> _linkedReferencedAssemblies;

	private ConcurrentDictionary<NamedTypeSymbol, NamedTypeSymbol> _noPiaUnificationMap;

	private readonly bool _isLinked;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	internal ConcurrentDictionary<NamedTypeSymbol, NamedTypeSymbol> NoPiaUnificationMap => LazyInitializer.EnsureInitialized(ref _noPiaUnificationMap, () => new ConcurrentDictionary<NamedTypeSymbol, NamedTypeSymbol>(2, 0));

	private RetargetingModuleSymbol.RetargetingSymbolTranslator RetargetingTranslator => ((RetargetingModuleSymbol)_modules[0]).RetargetingTranslator;

	public AssemblySymbol UnderlyingAssembly => _underlyingAssembly;

	public override bool IsImplicitlyDeclared => _underlyingAssembly.IsImplicitlyDeclared;

	public override AssemblyIdentity Identity => _underlyingAssembly.Identity;

	public override Version AssemblyVersionPattern => _underlyingAssembly.AssemblyVersionPattern;

	internal override ImmutableArray<byte> PublicKey => _underlyingAssembly.PublicKey;

	public override ImmutableArray<ModuleSymbol> Modules => _modules;

	internal override bool KeepLookingForDeclaredSpecialTypes => false;

	public override ImmutableArray<Location> Locations => _underlyingAssembly.Locations;

	internal override bool IsLinked => _isLinked;

	public override ICollection<string> TypeNames => _underlyingAssembly.TypeNames;

	public override ICollection<string> NamespaceNames => _underlyingAssembly.NamespaceNames;

	public override bool MightContainExtensionMethods => _underlyingAssembly.MightContainExtensionMethods;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	public RetargetingAssemblySymbol(SourceAssemblySymbol underlyingAssembly, bool isLinked)
	{
		_underlyingAssembly = underlyingAssembly;
		ModuleSymbol[] array = new ModuleSymbol[underlyingAssembly.Modules.Length];
		array[0] = new RetargetingModuleSymbol(this, (SourceModuleSymbol)underlyingAssembly.Modules[0]);
		for (int i = 1; i < underlyingAssembly.Modules.Length; i++)
		{
			PEModuleSymbol pEModuleSymbol = (PEModuleSymbol)underlyingAssembly.Modules[i];
			array[i] = new PEModuleSymbol(this, pEModuleSymbol.Module, pEModuleSymbol.ImportOptions, i);
		}
		_modules = array.AsImmutableOrNull();
		_isLinked = isLinked;
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _underlyingAssembly.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
	}

	internal override IEnumerable<ImmutableArray<byte>> GetInternalsVisibleToPublicKeys(string simpleName)
	{
		return _underlyingAssembly.GetInternalsVisibleToPublicKeys(simpleName);
	}

	internal override bool AreInternalsVisibleToThisAssembly(AssemblySymbol other)
	{
		return _underlyingAssembly.AreInternalsVisibleToThisAssembly(other);
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return RetargetingTranslator.GetRetargetedAttributes(_underlyingAssembly.GetAttributes(), ref _lazyCustomAttributes);
	}

	internal override NamedTypeSymbol GetDeclaredSpecialType(SpecialType type)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<AssemblySymbol> GetNoPiaResolutionAssemblies()
	{
		return _noPiaResolutionAssemblies;
	}

	internal override void SetNoPiaResolutionAssemblies(ImmutableArray<AssemblySymbol> assemblies)
	{
		_noPiaResolutionAssemblies = assemblies;
	}

	internal override void SetLinkedReferencedAssemblies(ImmutableArray<AssemblySymbol> assemblies)
	{
		_linkedReferencedAssemblies = assemblies;
	}

	internal override ImmutableArray<AssemblySymbol> GetLinkedReferencedAssemblies()
	{
		return _linkedReferencedAssemblies;
	}

	internal override bool GetGuidString(out string guidString)
	{
		return _underlyingAssembly.GetGuidString(out guidString);
	}

	internal override NamedTypeSymbol TryLookupForwardedMetadataTypeWithCycleDetection(ref MetadataTypeName emittedName, ConsList<AssemblySymbol> visitedAssemblies)
	{
		NamedTypeSymbol namedTypeSymbol = _underlyingAssembly.TryLookupForwardedMetadataType(ref emittedName);
		if ((object)namedTypeSymbol == null)
		{
			return null;
		}
		return RetargetingTranslator.Retarget(namedTypeSymbol, RetargetOptions.RetargetPrimitiveTypesByName);
	}

	public override AssemblyMetadata GetMetadata()
	{
		return _underlyingAssembly.GetMetadata();
	}
}
