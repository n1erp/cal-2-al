using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class MissingAssemblySymbol : AssemblySymbol
{
	protected readonly AssemblyIdentity identity;

	protected readonly MissingModuleSymbol moduleSymbol;

	private ImmutableArray<ModuleSymbol> _lazyModules;

	internal sealed override bool IsMissing => true;

	internal override bool IsLinked => false;

	public override AssemblyIdentity Identity => identity;

	public override Version AssemblyVersionPattern => null;

	internal override ImmutableArray<byte> PublicKey => Identity.PublicKey;

	public override ImmutableArray<ModuleSymbol> Modules
	{
		get
		{
			if (_lazyModules.IsDefault)
			{
				_lazyModules = ImmutableArray.Create((ModuleSymbol)moduleSymbol);
			}
			return _lazyModules;
		}
	}

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public sealed override NamespaceSymbol GlobalNamespace => moduleSymbol.GlobalNamespace;

	public override ICollection<string> TypeNames => SpecializedCollections.EmptyCollection<string>();

	public override ICollection<string> NamespaceNames => SpecializedCollections.EmptyCollection<string>();

	public override bool MightContainExtensionMethods => false;

	public MissingAssemblySymbol(AssemblyIdentity identity)
	{
		this.identity = identity;
		moduleSymbol = new MissingModuleSymbol(this, 0);
	}

	internal override Symbol GetDeclaredSpecialTypeMember(SpecialMember member)
	{
		return null;
	}

	public override int GetHashCode()
	{
		return identity.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as MissingAssemblySymbol);
	}

	public bool Equals(MissingAssemblySymbol other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return identity.Equals(other.Identity);
	}

	internal override void SetLinkedReferencedAssemblies(ImmutableArray<AssemblySymbol> assemblies)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<AssemblySymbol> GetLinkedReferencedAssemblies()
	{
		return ImmutableArray<AssemblySymbol>.Empty;
	}

	internal override void SetNoPiaResolutionAssemblies(ImmutableArray<AssemblySymbol> assemblies)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<AssemblySymbol> GetNoPiaResolutionAssemblies()
	{
		return ImmutableArray<AssemblySymbol>.Empty;
	}

	internal override NamedTypeSymbol LookupTopLevelMetadataTypeWithCycleDetection(ref MetadataTypeName emittedName, ConsList<AssemblySymbol> visitedAssemblies, bool digThroughForwardedTypes)
	{
		return moduleSymbol.LookupTopLevelMetadataType(ref emittedName);
	}

	internal override NamedTypeSymbol GetDeclaredSpecialType(SpecialType type)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override bool AreInternalsVisibleToThisAssembly(AssemblySymbol other)
	{
		return false;
	}

	internal override IEnumerable<ImmutableArray<byte>> GetInternalsVisibleToPublicKeys(string simpleName)
	{
		return SpecializedCollections.EmptyEnumerable<ImmutableArray<byte>>();
	}

	public override AssemblyMetadata GetMetadata()
	{
		return null;
	}
}
