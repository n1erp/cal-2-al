using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class DotNetAssemblySymbol : ContainerSymbol, IDotNetAssemblySymbol, ISymbol, IEquatable<ISymbol>
{
	public override Symbol ContainingSymbol { get; }

	public override SymbolKind Kind => SymbolKind.DotNetAssembly;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.DotNetAssemblyProperties;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override SymbolCompilationScope CompilationScope { get; } = SymbolCompilationScope.Internal;

	DotNetAssemblyInfo IDotNetAssemblySymbol.AssemblyInfo => AssemblyInfo;

	internal abstract DotNetAssemblyInfo AssemblyInfo { get; }

	internal abstract ImmutableArray<DotNetTypeDeclarationSymbol> TypeDeclarations { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetAssembly(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetAssembly(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetAssembly(this, a);
	}

	protected DotNetAssemblySymbol(DotNetPackageTypeSymbol containingPackage)
	{
		ContainingSymbol = containingPackage;
	}
}
