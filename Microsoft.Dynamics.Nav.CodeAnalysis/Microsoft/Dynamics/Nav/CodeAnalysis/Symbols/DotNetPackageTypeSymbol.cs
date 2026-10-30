using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class DotNetPackageTypeSymbol : ObjectTypeSymbol, IDotNetPackageTypeSymbol, IObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers, IRootTypeSymbol
{
	private ImmutableArray<DotNetTypeDeclarationSymbol> lazyTypeDeclarations;

	public override string Name { get; } = "DotNet";

	public override SymbolKind Kind { get; } = SymbolKind.DotNetPackage;

	public override TypeCategoryKind TypeCategory { get; } = TypeCategoryKind.ApplicationObject;

	public override NavTypeKind NavTypeKind { get; }

	public override SymbolCompilationScope CompilationScope { get; } = SymbolCompilationScope.Internal;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos { get; } = ImmutableDictionary<string, PropertyTypeInfo>.Empty;

	public override ImmutableArray<MethodSymbol> Triggers { get; } = ImmutableArray<MethodSymbol>.Empty;

	internal abstract ImmutableArray<DotNetAssemblySymbol> Assemblies { get; }

	internal override TypeSymbol BaseType { get; }

	internal override ImmutableArray<VariableSymbol> GlobalVariables { get; } = ImmutableArray<VariableSymbol>.Empty;

	internal ImmutableArray<DotNetTypeDeclarationSymbol> FlattenedTypeDeclarations
	{
		get
		{
			if (lazyTypeDeclarations != null)
			{
				return lazyTypeDeclarations;
			}
			ArrayBuilder<DotNetTypeDeclarationSymbol> instance = ArrayBuilder<DotNetTypeDeclarationSymbol>.GetInstance();
			foreach (DotNetAssemblySymbol assembly in Assemblies)
			{
				instance.AddRange(assembly.TypeDeclarations);
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyTypeDeclarations, instance.ToImmutableAndFree(), default);
			return lazyTypeDeclarations;
		}
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetPackageType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetPackageType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetPackageType(this, a);
	}

	protected DotNetPackageTypeSymbol(ContainerSymbol containingSymbol)
		: base(containingSymbol)
	{
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return null;
	}
}
