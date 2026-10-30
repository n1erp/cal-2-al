using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceDotNetAssemblyDeclarationSymbol : DotNetAssemblySymbol
{
	private readonly Lazy<DotNetAssemblyInfo> lazyAssemblyInfo;

	private readonly Lazy<ImmutableArray<DotNetTypeDeclarationSymbol>> lazyTypeDeclarations;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private DiagnosticInfo useSiteDiagnostics;

	private readonly DotNetAssemblyDefinition assemblyDefinition;

	public override string Name { get; }

	public override ImmutableArray<Location> Locations { get; } = ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; } = ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	internal override DotNetAssemblyInfo AssemblyInfo => lazyAssemblyInfo.Value;

	internal override ImmutableArray<DotNetTypeDeclarationSymbol> TypeDeclarations => lazyTypeDeclarations.Value;

	public ReferenceDotNetAssemblyDeclarationSymbol(DotNetPackageTypeSymbol containingPackage, DotNetAssemblyDefinition assemblyDefinition)
		: base(containingPackage)
	{
		Name = assemblyDefinition.Name;
		this.assemblyDefinition = assemblyDefinition;
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, assemblyDefinition);
		lazyAssemblyInfo = ReferenceSymbolHelper.CreateLazy(() => GetAssemblyInfoLazy());
		lazyTypeDeclarations = ReferenceSymbolHelper.CreateLazy(GetTypeDeclarationsLazy);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return TypeDeclarations.CastArray<Symbol>();
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (AssemblyInfo != null)
		{
			return null;
		}
		return useSiteDiagnostics;
	}

	private DotNetAssemblyInfo GetAssemblyInfoLazy()
	{
		string version = GetProperty(PropertyKind.Version)?.Value?.ToString();
		string culture = GetProperty(PropertyKind.Culture)?.Value?.ToString();
		string publicKeyToken = GetProperty(PropertyKind.PublicKeyToken)?.Value?.ToString();
		string assemblyName = DotNetUtilities.GetAssemblyName(Name, version, culture, publicKeyToken);
		DotNetAssemblyInfo dotNetAssemblyInfo = ContainingModule.DotNetResolver.LoadAssembly(assemblyName);
		if (dotNetAssemblyInfo == null)
		{
			string text = ContainingModule.DotNetResolver.AssemblyLocator.ProbingPaths.Join(", ");
			useSiteDiagnostics = new NavDiagnosticInfo(ErrorCode.ERR_DotNetAssemblyCouldNotBeFound, assemblyName, text);
		}
		return dotNetAssemblyInfo;
	}

	private ImmutableArray<DotNetTypeDeclarationSymbol> GetTypeDeclarationsLazy()
	{
		return assemblyDefinition.TypeDeclarations.AsImmutableOrEmpty((Func<DotNetTypeDeclarationDefinition, DotNetTypeDeclarationSymbol>)((DotNetTypeDeclarationDefinition typeDeclaration) => new ReferenceDotNetTypeDeclarationSymbol(this, typeDeclaration)));
	}
}
