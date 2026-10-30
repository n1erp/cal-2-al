using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class NonMissingModuleSymbol : ModuleSymbol
{
	private ModuleReferences<AssemblySymbol> _moduleReferences;

	internal sealed override bool IsMissing => false;

	internal override bool HasUnifiedReferences => GetUnifiedAssemblies().Length > 0;

	internal sealed override ImmutableArray<AssemblyIdentity> GetReferencedAssemblies()
	{
		return _moduleReferences.Identities;
	}

	internal sealed override ImmutableArray<AssemblySymbol> GetReferencedAssemblySymbols()
	{
		return _moduleReferences.Symbols;
	}

	internal ImmutableArray<UnifiedAssembly<AssemblySymbol>> GetUnifiedAssemblies()
	{
		return _moduleReferences.UnifiedAssemblies;
	}

	internal override bool GetUnificationUseSiteDiagnostic(ref DiagnosticInfo result, TypeSymbol dependentType)
	{
		AssemblySymbol containingAssembly = ContainingAssembly;
		AssemblySymbol containingAssembly2 = dependentType.ContainingAssembly;
		if (containingAssembly == containingAssembly2)
		{
			return false;
		}
		foreach (UnifiedAssembly<AssemblySymbol> unifiedAssembly in GetUnifiedAssemblies())
		{
			if ((object)unifiedAssembly.TargetAssembly == containingAssembly2)
			{
				AssemblyIdentity originalReference = unifiedAssembly.OriginalReference;
				AssemblyIdentity identity = containingAssembly2.Identity;
				ImmutableArray<Symbol> symbols = ImmutableArray.Create((Symbol)containingAssembly, (Symbol)containingAssembly2);
				DiagnosticInfo info = ((!(identity.Version > originalReference.Version)) ? new CSDiagnosticInfo(ErrorCode.ERR_AssemblyMatchBadVersion, new object[5]
				{
					containingAssembly.Name,
					containingAssembly.Identity.GetDisplayName(),
					originalReference.GetDisplayName(),
					containingAssembly2.Name,
					identity.GetDisplayName()
				}, symbols, ImmutableArray<Location>.Empty) : new CSDiagnosticInfo((identity.Version.Major == originalReference.Version.Major && identity.Version.Minor == originalReference.Version.Minor) ? ErrorCode.WRN_UnifyReferenceBldRev : ErrorCode.WRN_UnifyReferenceMajMin, new object[4]
				{
					originalReference.GetDisplayName(),
					containingAssembly.Name,
					identity.GetDisplayName(),
					containingAssembly2.Name
				}, symbols, ImmutableArray<Location>.Empty));
				if (MergeUseSiteDiagnostics(ref result, info))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal override void SetReferences(ModuleReferences<AssemblySymbol> moduleReferences, SourceAssemblySymbol originatingSourceAssemblyDebugOnly = null)
	{
		_moduleReferences = moduleReferences;
	}

	[Conditional("DEBUG")]
	internal void AssertReferencesUninitialized()
	{
	}

	[Conditional("DEBUG")]
	internal void AssertReferencesInitialized()
	{
	}

	internal sealed override NamedTypeSymbol LookupTopLevelMetadataType(ref MetadataTypeName emittedName)
	{
		NamespaceSymbol namespaceSymbol = GlobalNamespace.LookupNestedNamespace(emittedName.NamespaceSegments);
		if ((object)namespaceSymbol == null)
		{
			return new MissingMetadataTypeSymbol.TopLevel(this, ref emittedName);
		}
		return namespaceSymbol.LookupMetadataType(ref emittedName);
	}
}
