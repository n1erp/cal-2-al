using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class MissingModuleSymbol : ModuleSymbol
{
	protected readonly AssemblySymbol assembly;

	protected readonly int ordinal;

	protected readonly MissingNamespaceSymbol globalNamespace;

	internal override int Ordinal => ordinal;

	internal override Machine Machine => Machine.I386;

	internal override bool Bit32Required => false;

	internal sealed override bool IsMissing => true;

	public override string Name => "<Missing Module>";

	public override AssemblySymbol ContainingAssembly => assembly;

	public override Symbol ContainingSymbol => assembly;

	public override NamespaceSymbol GlobalNamespace => globalNamespace;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	internal override ICollection<string> NamespaceNames => SpecializedCollections.EmptyCollection<string>();

	internal override ICollection<string> TypeNames => SpecializedCollections.EmptyCollection<string>();

	internal override bool HasUnifiedReferences => false;

	internal override bool HasAssemblyCompilationRelaxationsAttribute => false;

	internal override bool HasAssemblyRuntimeCompatibilityAttribute => false;

	internal override CharSet? DefaultMarshallingCharSet => null;

	public MissingModuleSymbol(AssemblySymbol assembly, int ordinal)
	{
		this.assembly = assembly;
		this.ordinal = ordinal;
		globalNamespace = new MissingNamespaceSymbol(this);
	}

	public override int GetHashCode()
	{
		return assembly.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is MissingModuleSymbol missingModuleSymbol)
		{
			return assembly.Equals(missingModuleSymbol.assembly);
		}
		return false;
	}

	internal override NamedTypeSymbol LookupTopLevelMetadataType(ref MetadataTypeName emittedName)
	{
		return new MissingMetadataTypeSymbol.TopLevel(this, ref emittedName);
	}

	internal override ImmutableArray<AssemblyIdentity> GetReferencedAssemblies()
	{
		return ImmutableArray<AssemblyIdentity>.Empty;
	}

	internal override ImmutableArray<AssemblySymbol> GetReferencedAssemblySymbols()
	{
		return ImmutableArray<AssemblySymbol>.Empty;
	}

	internal override void SetReferences(ModuleReferences<AssemblySymbol> moduleReferences, SourceAssemblySymbol originatingSourceAssemblyDebugOnly)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override bool GetUnificationUseSiteDiagnostic(ref DiagnosticInfo result, TypeSymbol dependentType)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override ModuleMetadata GetMetadata()
	{
		return null;
	}
}
