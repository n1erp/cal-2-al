using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceDotNetTypeDeclarationSymbol : DotNetTypeDeclarationSymbol
{
	private readonly Lazy<DotNetTypeSymbol> lazyDotNetTypeSymbol;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly DotNetAssemblySymbol containingAssembly;

	private DiagnosticInfo useSiteDiagnostics;

	public override DotNetTypeSymbol DotNetType => lazyDotNetTypeSymbol.Value;

	public override ImmutableArray<Location> Locations { get; } = ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; } = ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public ReferenceDotNetTypeDeclarationSymbol(DotNetAssemblySymbol containingSymbol, DotNetTypeDeclarationDefinition typeDefinition)
		: base(containingSymbol, typeDefinition.TypeName, typeDefinition.AliasName ?? typeDefinition.TypeName)
	{
		ReferenceDotNetTypeDeclarationSymbol referenceDotNetTypeDeclarationSymbol = this;
		containingAssembly = containingSymbol;
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, typeDefinition);
		lazyDotNetTypeSymbol = ReferenceSymbolHelper.CreateLazy(() => referenceDotNetTypeDeclarationSymbol.GetDotNetTypeSymbolLazy(typeDefinition));
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (!(DotNetType == null))
		{
			return null;
		}
		return useSiteDiagnostics;
	}

	private DotNetTypeSymbol GetDotNetTypeSymbolLazy(DotNetTypeDeclarationDefinition typeDefinition)
	{
		DotNetAssemblyInfo assemblyInfo = containingAssembly.AssemblyInfo;
		if (assemblyInfo == null)
		{
			return null;
		}
		DotNetTypeInfo dotNetTypeInfo = ContainingModule.DotNetResolver.LoadType(assemblyInfo, typeDefinition.TypeName);
		if (dotNetTypeInfo == null)
		{
			useSiteDiagnostics = new NavDiagnosticInfo(ErrorCode.ERR_DotNetTypeCouldNotBeFound, typeDefinition.TypeName, assemblyInfo.FullName);
			return null;
		}
		return (DotNetTypeSymbol)ContainingModule.DotNetResolver.MapDeclaredType(this, dotNetTypeInfo);
	}
}
