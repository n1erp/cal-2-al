using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceDotNetPackageTypeSymbol : DotNetPackageTypeSymbol
{
	private readonly Lazy<ImmutableArray<DotNetAssemblySymbol>> lazyAssemblies;

	public override ImmutableArray<Location> Locations { get; } = ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; } = ImmutableArray<SyntaxReference>.Empty;

	internal override ImmutableArray<DotNetAssemblySymbol> Assemblies => lazyAssemblies.Value;

	public ReferenceDotNetPackageTypeSymbol(ContainerSymbol containingSymbol, DotNetPackageDefinition packageDefinition)
		: base(containingSymbol)
	{
		ReferenceDotNetPackageTypeSymbol referenceDotNetPackageTypeSymbol = this;
		lazyAssemblies = ReferenceSymbolHelper.CreateLazy(() => referenceDotNetPackageTypeSymbol.GetAssembliesLazy(packageDefinition));
	}

	private ImmutableArray<DotNetAssemblySymbol> GetAssembliesLazy(DotNetPackageDefinition packageDefinition)
	{
		return packageDefinition.AssemblyDeclarations.AsImmutableOrEmpty((Func<DotNetAssemblyDefinition, DotNetAssemblySymbol>)((DotNetAssemblyDefinition assembly) => new ReferenceDotNetAssemblyDeclarationSymbol(this, assembly)));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return Assemblies.CastArray<Symbol>();
	}
}
