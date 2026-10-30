using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface IModuleSymbol : ISymbol, IEquatable<ISymbol>
{
	INamespaceSymbol GlobalNamespace { get; }

	ImmutableArray<AssemblyIdentity> ReferencedAssemblies { get; }

	ImmutableArray<IAssemblySymbol> ReferencedAssemblySymbols { get; }

	INamespaceSymbol GetModuleNamespace(INamespaceSymbol namespaceSymbol);

	ModuleMetadata GetMetadata();
}
