using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class ModuleSymbol : Symbol, IModuleSymbol, ISymbol, IEquatable<ISymbol>
{
	public abstract NamespaceSymbol GlobalNamespace { get; }

	public override AssemblySymbol ContainingAssembly => (AssemblySymbol)ContainingSymbol;

	internal sealed override ModuleSymbol ContainingModule => null;

	public sealed override SymbolKind Kind => SymbolKind.NetModule;

	internal abstract int Ordinal { get; }

	internal abstract Machine Machine { get; }

	internal abstract bool Bit32Required { get; }

	internal abstract bool IsMissing { get; }

	public sealed override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public sealed override bool IsStatic => false;

	public sealed override bool IsVirtual => false;

	public sealed override bool IsOverride => false;

	public sealed override bool IsAbstract => false;

	public sealed override bool IsSealed => false;

	public sealed override bool IsExtern => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public ImmutableArray<AssemblyIdentity> ReferencedAssemblies => GetReferencedAssemblies();

	public ImmutableArray<AssemblySymbol> ReferencedAssemblySymbols => GetReferencedAssemblySymbols();

	internal abstract bool HasUnifiedReferences { get; }

	internal abstract ICollection<string> TypeNames { get; }

	internal abstract ICollection<string> NamespaceNames { get; }

	internal abstract bool HasAssemblyCompilationRelaxationsAttribute { get; }

	internal abstract bool HasAssemblyRuntimeCompatibilityAttribute { get; }

	internal abstract CharSet? DefaultMarshallingCharSet { get; }

	INamespaceSymbol IModuleSymbol.GlobalNamespace => GlobalNamespace;

	ImmutableArray<IAssemblySymbol> IModuleSymbol.ReferencedAssemblySymbols => ImmutableArray<IAssemblySymbol>.CastUp(ReferencedAssemblySymbols);

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitModule(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitModule(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitModule(this);
	}

	internal ModuleSymbol()
	{
	}

	internal abstract ImmutableArray<AssemblyIdentity> GetReferencedAssemblies();

	internal abstract ImmutableArray<AssemblySymbol> GetReferencedAssemblySymbols();

	internal AssemblySymbol GetReferencedAssemblySymbol(int referencedAssemblyIndex)
	{
		ImmutableArray<AssemblySymbol> referencedAssemblySymbols = GetReferencedAssemblySymbols();
		if (referencedAssemblyIndex < referencedAssemblySymbols.Length)
		{
			return referencedAssemblySymbols[referencedAssemblyIndex];
		}
		AssemblySymbol containingAssembly = ContainingAssembly;
		if ((object)containingAssembly != containingAssembly.CorLibrary)
		{
			throw new ArgumentOutOfRangeException("referencedAssemblyIndex");
		}
		return null;
	}

	internal abstract void SetReferences(ModuleReferences<AssemblySymbol> moduleReferences, SourceAssemblySymbol originatingSourceAssemblyDebugOnly = null);

	internal abstract bool GetUnificationUseSiteDiagnostic(ref DiagnosticInfo result, TypeSymbol dependentType);

	internal abstract NamedTypeSymbol LookupTopLevelMetadataType(ref MetadataTypeName emittedName);

	internal virtual ImmutableArray<byte> GetHash(AssemblyHashAlgorithm algorithmId)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public NamespaceSymbol GetModuleNamespace(INamespaceSymbol namespaceSymbol)
	{
		if (namespaceSymbol == null)
		{
			throw new ArgumentNullException("namespaceSymbol");
		}
		if (namespaceSymbol is NamespaceSymbol { Extent: { Kind: NamespaceKind.Module } } namespaceSymbol2 && namespaceSymbol2.ContainingModule == this)
		{
			return namespaceSymbol2;
		}
		if (namespaceSymbol.IsGlobalNamespace || namespaceSymbol.ContainingNamespace == null)
		{
			return GlobalNamespace;
		}
		return GetModuleNamespace(namespaceSymbol.ContainingNamespace)?.GetNestedNamespace(namespaceSymbol.Name);
	}

	INamespaceSymbol IModuleSymbol.GetModuleNamespace(INamespaceSymbol namespaceSymbol)
	{
		return GetModuleNamespace(namespaceSymbol);
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitModule(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitModule(this);
	}

	public abstract ModuleMetadata GetMetadata();
}
