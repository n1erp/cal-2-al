using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class DotNetTypeDeclarationSymbol : ObjectTypeSymbol, IDotNetTypeDeclarationSymbol, ISymbol, IEquatable<ISymbol>
{
	private readonly Lazy<bool> lazyIsControlAddIn;

	public override string Name { get; }

	public string TypeName { get; }

	public bool IsControlAddIn => lazyIsControlAddIn.Value;

	public override SymbolKind Kind => SymbolKind.DotNetTypeDeclaration;

	public override NavTypeKind NavTypeKind => NavTypeKind.None;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Complex;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override SymbolCompilationScope CompilationScope { get; } = SymbolCompilationScope.Internal;

	public abstract DotNetTypeSymbol DotNetType { get; }

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos { get; } = ObjectParser.DotNetTypeDeclarationProperties;

	public override ImmutableArray<MethodSymbol> Triggers { get; } = ImmutableArray<MethodSymbol>.Empty;

	internal override ImmutableArray<VariableSymbol> GlobalVariables { get; } = ImmutableArray<VariableSymbol>.Empty;

	IDotNetTypeSymbol IDotNetTypeDeclarationSymbol.DotNetType => DotNetType;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetTypeDeclaration(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetTypeDeclaration(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetTypeDeclaration(this, a);
	}

	protected DotNetTypeDeclarationSymbol(ContainerSymbol containingSymbol, string typeName, string alias)
		: base(containingSymbol)
	{
		TypeName = typeName;
		Name = alias;
		lazyIsControlAddIn = new Lazy<bool>(() => this.GetSimplePropertyValue<bool>(PropertyKind.IsControlAddIn) ?? false, LazyThreadSafetyMode.PublicationOnly);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return null;
	}
}
