using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class PEAssemblySymbol : MetadataOrSourceAssemblySymbol
{
	private readonly PEAssembly _assembly;

	private readonly DocumentationProvider _documentationProvider;

	private readonly ImmutableArray<ModuleSymbol> _modules;

	private ImmutableArray<AssemblySymbol> _noPiaResolutionAssemblies;

	private ImmutableArray<AssemblySymbol> _linkedReferencedAssemblies;

	private readonly bool _isLinked;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	internal PEAssembly Assembly => _assembly;

	public override AssemblyIdentity Identity => _assembly.Identity;

	public override Version AssemblyVersionPattern => null;

	public override ImmutableArray<ModuleSymbol> Modules => _modules;

	public override ImmutableArray<Location> Locations => PrimaryModule.MetadataLocation.Cast<MetadataLocation, Location>();

	internal override ImmutableArray<byte> PublicKey => Identity.PublicKey;

	internal DocumentationProvider DocumentationProvider => _documentationProvider;

	internal override bool IsLinked => _isLinked;

	public override bool MightContainExtensionMethods => true;

	internal PEModuleSymbol PrimaryModule => (PEModuleSymbol)_modules[0];

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	internal PEAssemblySymbol(PEAssembly assembly, DocumentationProvider documentationProvider, bool isLinked, MetadataImportOptions importOptions)
	{
		_assembly = assembly;
		_documentationProvider = documentationProvider;
		ModuleSymbol[] array = new ModuleSymbol[assembly.Modules.Length];
		for (int i = 0; i < assembly.Modules.Length; i++)
		{
			array[i] = new PEModuleSymbol(this, assembly.Modules[i], importOptions, i);
		}
		_modules = array.AsImmutableOrNull();
		_isLinked = isLinked;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (_lazyCustomAttributes.IsDefault)
		{
			if (MightContainExtensionMethods)
			{
				PrimaryModule.LoadCustomAttributesFilterExtensions(_assembly.Handle, ref _lazyCustomAttributes);
			}
			else
			{
				PrimaryModule.LoadCustomAttributes(_assembly.Handle, ref _lazyCustomAttributes);
			}
		}
		return _lazyCustomAttributes;
	}

	internal (AssemblySymbol FirstSymbol, AssemblySymbol SecondSymbol) LookupAssembliesForForwardedMetadataType(ref MetadataTypeName emittedName)
	{
		return PrimaryModule.GetAssembliesForForwardedType(ref emittedName);
	}

	internal override NamedTypeSymbol TryLookupForwardedMetadataTypeWithCycleDetection(ref MetadataTypeName emittedName, ConsList<AssemblySymbol> visitedAssemblies)
	{
		var (assemblySymbol, assemblySymbol2) = LookupAssembliesForForwardedMetadataType(ref emittedName);
		if ((object)assemblySymbol != null)
		{
			if ((object)assemblySymbol2 != null)
			{
				return CreateMultipleForwardingErrorTypeSymbol(ref emittedName, PrimaryModule, assemblySymbol, assemblySymbol2);
			}
			if (visitedAssemblies != null && visitedAssemblies.Contains(assemblySymbol))
			{
				return CreateCycleInTypeForwarderErrorTypeSymbol(ref emittedName);
			}
			visitedAssemblies = new ConsList<AssemblySymbol>(this, visitedAssemblies ?? ConsList<AssemblySymbol>.Empty);
			return assemblySymbol.LookupTopLevelMetadataTypeWithCycleDetection(ref emittedName, visitedAssemblies, digThroughForwardedTypes: true);
		}
		return null;
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
		return Assembly.Modules[0].HasGuidAttribute(Assembly.Handle, out guidString);
	}

	internal override bool AreInternalsVisibleToThisAssembly(AssemblySymbol potentialGiverOfAccess)
	{
		IVTConclusion iVTConclusion = MakeFinalIVTDetermination(potentialGiverOfAccess);
		if (iVTConclusion != IVTConclusion.Match)
		{
			return iVTConclusion == IVTConclusion.OneSignedOneNot;
		}
		return true;
	}

	internal override IEnumerable<ImmutableArray<byte>> GetInternalsVisibleToPublicKeys(string simpleName)
	{
		return Assembly.GetInternalsVisibleToPublicKeys(simpleName);
	}

	public override AssemblyMetadata GetMetadata()
	{
		return _assembly.GetNonDisposableMetadata();
	}
}
